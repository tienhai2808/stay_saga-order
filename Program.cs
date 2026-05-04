using OrderService.Services;
using Grpc.Property;
using Common.Extensions;
using Common.Middleware;
using OrderService.Providers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSnowflakeIdGenerator(builder.Configuration);
builder.Services.AddApiControllers();
builder.Services.AddOpenApi();
builder.Services.AddKeycloakJwtAuth(builder.Configuration);
builder.Services.AddScoped<BookingService>();
builder.Services.AddScoped<PropertyProvider>();

var propertyGrpcAddress = builder.Configuration["GrpcClients:PropertyService:Address"];
if (!Uri.TryCreate(propertyGrpcAddress, UriKind.Absolute, out var propertyGrpcUri))
{
    throw new InvalidOperationException("GrpcClients:PropertyService:Address must be a valid absolute URI.");
}

builder.Services.AddGrpcClient<PropertyService.PropertyServiceClient>((_, clientOptions) =>
{
    clientOptions.Address = propertyGrpcUri;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<HttpExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
