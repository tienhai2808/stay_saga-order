using Grpc.Property;
using OrderService.Providers;

namespace OrderService.Services;

public class BookingService(PropertyProvider propertyProvider)
{
    private readonly PropertyProvider _propertyProvider = propertyProvider;

    public async Task PingPropertyAsync(CancellationToken cancellationToken)
    {
        await _propertyProvider.PingAsync(cancellationToken);
    }
}
