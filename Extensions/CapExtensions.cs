using Common.Exceptions;
using OrderService.Data;

namespace OrderService.Extensions;

public static class CapExtensions
{
    public static IServiceCollection AddProducerMessaging(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InternalServerException("ConnectionStrings:Default is required.");
        var kafkaBootstrapServers = configuration["Kafka:BootstrapServers"]
            ?? throw new InternalServerException("Kafka:BootstrapServers is required.");

        services.AddCap(x =>
        {
            x.DefaultGroupName = "order-service";
            x.Version = string.Empty;
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
