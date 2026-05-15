using System.ComponentModel.DataAnnotations;

namespace OrderService.DTOs;

public class CreateBookingRequestDto
{
    [Required(ErrorMessage = "Room type id is required.")]
    [RegularExpression("^[0-9]+$", ErrorMessage = "Room type id must contain digits only.")]
    [MaxLength(20, ErrorMessage = "Room type id must be at most 20 characters.")]
    public string RoomTypeId { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Room count must be greater than 0.")]
    public int RoomCount { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Guest count must be greater than 0.")]
    public int GuestCount { get; set; }

    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
}