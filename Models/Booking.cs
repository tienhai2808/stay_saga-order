namespace OrderService.Models;

public class Booking
{
    public long Id { get; set; }
    public string KeycloakId { get; set; } = string.Empty;
    public long RoomTypeId { get; set; }
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public int RoomCount { get; set; }
    public int GuestCount { get; set; }
    public BookingStatus Status { get; set; }
    public decimal Amount { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}