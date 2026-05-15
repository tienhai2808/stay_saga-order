using Common.Exceptions;
using Common.Mappers;
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
    BookingRepository bookingRepo
)
{
    private readonly IIdGenerator<long> _idGenerator = idGenerator;
    private readonly PropertyProvider _propertyProvider = propertyProvider;
    private readonly BookingRepository _bookingRepo = bookingRepo;

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
            var booking = new Booking
            {
                Id = _idGenerator.CreateId(),
                KeycloakId = keycloakId,
                RoomTypeId = roomTypeId,
                CheckIn = DateTime.SpecifyKind(checkInDate, DateTimeKind.Utc),
                CheckOut = DateTime.SpecifyKind(checkOutDate, DateTimeKind.Utc),
                RoomCount = dto.RoomCount,
                GuestCount = dto.GuestCount,
                Status = BookingStatus.Pending,
                Amount = (decimal)response.Amount,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            };

            await _bookingRepo.CreateAsync(booking, cancellationToken);

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
