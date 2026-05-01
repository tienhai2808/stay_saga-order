using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using OrderService.Services;

namespace OrderService.Controllers;

[ApiController]
[Route("bookings")]
public class BookingController(BookingService bookingService) : ControllerBase
{
    private readonly BookingService _bookingService = bookingService;

    [HttpGet("ping")]
    public async Task<IActionResult> PingPropertyService(CancellationToken cancellationToken)
    {
        try
        {
            await _bookingService.PingPropertyAsync(cancellationToken);
            return Ok(new { message = "property_service is reachable via gRPC" });
        }
        catch (RpcException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new
            {
                message = "Failed to call property_service via gRPC",
                grpcStatus = ex.StatusCode.ToString()
            });
        }
    }
}
