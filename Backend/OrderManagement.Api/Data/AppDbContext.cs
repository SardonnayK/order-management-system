using Microsoft.EntityFrameworkCore;
using OrderManagement.Api.Models;

namespace OrderManagement.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.Property(c => c.Id).HasMaxLength(100);
            entity.Property(c => c.Email).IsRequired().HasMaxLength(320);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(o => o.ClientReference).IsRequired().HasMaxLength(100);
            entity.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(o => o.Currency).IsRequired().HasMaxLength(3);
            entity.Property(o => o.Notes).HasMaxLength(2000);

            // SQLite stores DateTime without its kind; restore Utc on read so the
            // value serializes with the trailing 'Z' the frontend expects.
            entity.Property(o => o.CreatedAtUtc)
                .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            // An order cannot exist without a customer: required FK via shadow property.
            entity.HasOne(o => o.Customer)
                .WithMany()
                .HasForeignKey("CustomerId")
                .IsRequired();

            // A customer cannot reuse the same client reference number.
            entity.HasIndex("CustomerId", nameof(Order.ClientReference)).IsUnique();

            // Line items have no identity of their own; they live and die with the order.
            // The owned table gets a synthetic single-column autoincrement key because
            // SQLite cannot generate values for a column inside a composite primary key.
            entity.OwnsMany(o => o.Items, items =>
            {
                items.ToTable("OrderItems");
                items.WithOwner().HasForeignKey("OrderId");
                items.HasKey("Id");
                items.Property(i => i.Sku).IsRequired().HasMaxLength(100);
                items.Property(i => i.Name).IsRequired().HasMaxLength(200);
            });
        });
    }
}
