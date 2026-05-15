using Grpc.Property;

namespace OrderService.Providers;

public class PropertyProvider(PropertyService.PropertyServiceClient propertyServiceClient)
{
    private readonly PropertyService.PropertyServiceClient _propertyServiceClient = propertyServiceClient;

    public async Task PingAsync(CancellationToken cancellationToken = default)
    {
        await _propertyServiceClient.PingAsync(new PingRequest(), cancellationToken: cancellationToken);
    }

    public async Task<ReserveResponse> ReserveAsync(
        long roomTypeId,
        int roomCount,
        int guestCount,
        DateTime checkIn,
        DateTime checkOut,
        CancellationToken cancellationToken = default
    )
    {
        var request = new ReserveRequest
        {
            RoomTypeId = roomTypeId,
            RoomCount = roomCount,
            GuestCount = guestCount,
            CheckIn = checkIn.ToString("yyyy-MM-dd"),
            CheckOut = checkOut.ToString("yyyy-MM-dd")
        };

        return await _propertyServiceClient.ReserveAsync(request, cancellationToken: cancellationToken);
    }

    public async Task ReleaseAsync(
        long roomTypeId,
        int roomCount,
        DateTime checkIn,
        DateTime checkOut,
        CancellationToken cancellationToken = default
    )
    {
        var request = new ReleaseRequest
        {
            RoomTypeId = roomTypeId,
            RoomCount = roomCount,
            CheckIn = checkIn.ToString("yyyy-MM-dd"),
            CheckOut = checkOut.ToString("yyyy-MM-dd")
        };

        await _propertyServiceClient.ReleaseAsync(request, cancellationToken: cancellationToken);
    }
}
