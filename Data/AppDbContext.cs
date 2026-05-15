using Microsoft.EntityFrameworkCore;
using OrderService.Models;

namespace OrderService.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Booking> Bookings => Set<Booking>();

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyUpdatedAt();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ApplyUpdatedAt();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var booking = modelBuilder.Entity<Booking>();
        booking.ToTable("bookings");

        booking.HasKey(x => x.Id);
        booking.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        booking.Property(x => x.KeycloakId)
            .HasColumnName("keycloak_id")
            .IsRequired()
            .HasMaxLength(64)
            .HasColumnType("character varying(64)");

        booking.Property(x => x.RoomTypeId)
            .HasColumnName("room_type_id")
            .IsRequired();

        booking.Property(x => x.CheckIn)
            .HasColumnName("check_in")
            .IsRequired();

        booking.Property(x => x.CheckOut)
            .HasColumnName("check_out")
            .IsRequired();

        booking.Property(x => x.RoomCount)
            .HasColumnName("room_count")
            .IsRequired();

        booking.Property(x => x.GuestCount)
            .HasColumnName("guest_count")
            .IsRequired();

        booking.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        booking.Property(x => x.Amount)
            .HasColumnName("amount")
            .HasPrecision(18, 2)
            .IsRequired();

        booking.Property(x => x.ExpiresAt)
            .HasColumnName("expires_at")
            .HasColumnType("timestamp with time zone");

        booking.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now()");

        booking.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired()
            .HasColumnType("timestamp with time zone");
    }

    private void ApplyUpdatedAt()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<Booking>())
        {
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }
    }
}
