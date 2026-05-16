using OrderService.Data;

namespace OrderService.Extensions;

public static class CapExtensions
{
    public static IServiceCollection AddOrderMessaging(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("ConnectionStrings:Default is required.");
        var kafkaBootstrapServers = configuration["Kafka:BootstrapServers"]
            ?? throw new InvalidOperationException("Kafka:BootstrapServers is required.");

        services.AddCap(x =>
        {
            x.DefaultGroupName = "order-service";
            x.FailedRetryCount = 10;
            x.FailedRetryInterval = 60;
            x.UseStorageLock = true;
            x.UseEntityFramework<AppDbContext>();
            x.UsePostgreSql(connectionString);
            x.UseKafka(kafkaBootstrapServers);
        });

        return services;
    }
}
