using Grpc.Property;

namespace OrderService.Providers;

public class PropertyProvider(PropertyService.PropertyServiceClient propertyServiceClient)
{
    private readonly PropertyService.PropertyServiceClient _propertyServiceClient = propertyServiceClient;

    public async Task PingAsync(CancellationToken cancellationToken = default)
    {
        await _propertyServiceClient.PingAsync(new PingRequest(), cancellationToken: cancellationToken);
    }
}