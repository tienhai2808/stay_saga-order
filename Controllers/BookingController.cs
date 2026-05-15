using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Common.DTOs;
using Common.Exceptions;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.DTOs;
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

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateBooking(
        CreateBookingRequestDto dto,
        CancellationToken cancellationToken
    )
    {
        var keycloakId = GetCurrentKeycloakId();

        var bookingId = await _bookingService.CreateBookingAsync(keycloakId, dto, cancellationToken);

        var response = HttpApiResponseDto<object>.Success(
            new { id = bookingId.ToString() },
            "Booking created successfully"
        );

        return StatusCode(StatusCodes.Status201Created, response);
    }

    private string GetCurrentKeycloakId()
    {
        var keycloakId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? throw new UnauthorizedException("Invalid access token");
        return keycloakId;
    }
}
