namespace OrderService.DTOs;

public record BookingCreatedEventDto
{
    public long BookingId { get; init; }
    public string KeycloakId { get; init; } = string.Empty;
    public long RoomTypeId { get; init; }
    public DateOnly CheckIn  { get; init; }
    public DateOnly CheckOut { get; init; }
    public int RoomCount    { get; init; }
    public decimal Amount { get; init; }
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}