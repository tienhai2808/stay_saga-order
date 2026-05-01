using OrderService.Services;
using Grpc.Property;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddScoped<BookingService>();

var propertyGrpcAddress = builder.Configuration["GrpcClients:PropertyService:Address"];
if (!Uri.TryCreate(propertyGrpcAddress, UriKind.Absolute, out var propertyGrpcUri))
{
    throw new InvalidOperationException("GrpcClients:PropertyService:Address must be a valid absolute URI.");
}

var grpcClientBuilder = builder.Services.AddGrpcClient<PropertyService.PropertyServiceClient>((_, clientOptions) =>
{
    clientOptions.Address = propertyGrpcUri;
});

if (builder.Environment.IsDevelopment())
{
    grpcClientBuilder.ConfigurePrimaryHttpMessageHandler(() =>
    {
        return new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
    });
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
