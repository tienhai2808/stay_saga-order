using OrderService.Data;
using OrderService.Models;

namespace OrderService.Repositories;

public class BookingRepository(AppDbContext db)
{
    private readonly AppDbContext _db = db;

    public async Task CreateAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync(cancellationToken);
    }
}