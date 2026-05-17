using Common.Constants;
using Common.Events;
using Common.Exceptions;
using Common.Mappers;
using DotNetCore.CAP;
using Grpc.Core;
using Grpc.Property;
using IdGen;
using OrderService.Data;
using OrderService.DTOs;
using OrderService.Models;
using OrderService.Providers;
using OrderService.Repositories;

namespace OrderService.Services;

public class BookingService(
    IIdGenerator<long> idGenerator,
    PropertyProvider propertyProvider,
    BookingRepository bookingRepo,
    AppDbContext dbContext,
    ICapPublisher capPublisher
)
{
    private readonly IIdGenerator<long> _idGenerator = idGenerator;
    private readonly ICapPublisher _capPublisher = capPublisher;
    private readonly PropertyProvider _propertyProvider = propertyProvider;
    private readonly BookingRepository _bookingRepo = bookingRepo;
    private readonly AppDbContext _dbContext = dbContext;

    public async Task PingPropertyAsync(CancellationToken cancellationToken)
    {
        await _propertyProvider.PingAsync(cancellationToken);
    }

    public async Task<long> CreateBookingAsync(
        string keycloakId,
        CreateBookingRequestDto dto,
        CancellationToken cancellationToken
    )
    {
        if (!long.TryParse(dto.RoomTypeId.Trim(), out var roomTypeId) || roomTypeId <= 0)
            throw new BadRequestException("Invalid room type id");

        var checkInDate = dto.CheckIn.Date;
        var checkOutDate = dto.CheckOut.Date;

        if (checkInDate == DateTime.MinValue.Date || checkOutDate == DateTime.MinValue.Date)
            throw new BadRequestException("Check-in and check-out are required");

        if (checkOutDate <= checkInDate)
            throw new BadRequestException("Check-out must be after check-in");

        ReserveResponse response;
        try
        {
            response = await _propertyProvider.ReserveAsync(
                roomTypeId,
                dto.RoomCount,
                dto.GuestCount,
                checkInDate,
                checkOutDate,
                cancellationToken
            );
        }
        catch (RpcException ex)
        {
            throw RpcExceptionMapper.Map(ex, "Property", "reserve", cancellationToken);
        }

        try
        {
            var bookingId = _idGenerator.CreateId();
            var booking = new Booking
            {
                Id = bookingId,
                KeycloakId = keycloakId,
                RoomTypeId = roomTypeId,
                CheckIn = DateTime.SpecifyKind(checkInDate, DateTimeKind.Utc),
                CheckOut = DateTime.SpecifyKind(checkOutDate, DateTimeKind.Utc),
                RoomCount = dto.RoomCount,
                GuestCount = dto.GuestCount,
                Status = BookingStatuses.Pending,
                Amount = (decimal)response.Amount,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            };

            using var transaction = _dbContext.Database.BeginTransaction(_capPublisher, autoCommit: true);

            await _bookingRepo.CreateAsync(booking, cancellationToken);

            await _capPublisher.PublishAsync(TopicConstants.BookingCreatedTopic, new BookingCreatedEvent
            {
                BookingId = booking.Id,
                KeycloakId = keycloakId,
                RoomTypeId = roomTypeId,
                RoomCount = booking.RoomCount,
                CheckIn = DateOnly.FromDateTime(booking.CheckIn),
                CheckOut = DateOnly.FromDateTime(booking.CheckOut),
                Amount = booking.Amount,
                OccurredAt = DateTime.UtcNow
            }, callbackName: null, cancellationToken);

            return booking.Id;
        }
        catch (Exception)
        {
            try
            {
                await _propertyProvider.ReleaseAsync(
                    roomTypeId,
                    dto.RoomCount,
                    checkInDate,
                    checkOutDate,
                    CancellationToken.None
                );
            }
            catch (RpcException rpcEx)
            {
                throw RpcExceptionMapper.Map(rpcEx, "Property", "reserve", cancellationToken);
            }

            throw;
        }
    }
}
