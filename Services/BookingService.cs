using Grpc.Property;

namespace OrderService.Services;

public class BookingService(PropertyService.PropertyServiceClient propertyServiceClient)
{
    private readonly PropertyService.PropertyServiceClient _propertyServiceClient = propertyServiceClient;

    public async Task PingPropertyAsync(CancellationToken cancellationToken = default)
    {
        await _propertyServiceClient.PingAsync(new PingRequest(), cancellationToken: cancellationToken);
    }
}
