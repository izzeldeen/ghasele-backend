using Ghasele.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ghasele.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
     
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<SupportTicket> SupportTickets { get; set; } // Added SupportTickets
        public DbSet<Cleaner> Cleaners { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<ItemType> ItemTypes { get; set; }
        public DbSet<UserLocation> UserLocations { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<MarketingCode> MarketingCodes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Body).IsRequired().HasMaxLength(1000);
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).HasMaxLength(100).IsRequired(false);
                entity.HasIndex(e => e.Email).IsUnique().HasFilter("\"Email\" IS NOT NULL");
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.FcmToken).HasMaxLength(500);
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Lat).IsRequired();
                entity.Property(e => e.Long).IsRequired();
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
                entity.Property(e => e.NetAmount).HasPrecision(18, 2);
                entity.Property(e => e.DeliveryAmount).HasPrecision(18, 2);
                entity.Property(e => e.CleanerAmount).HasPrecision(18, 2);
                entity.Property(e => e.Status).HasConversion<string>();
                
                entity.HasOne(o => o.User)
                      .WithMany()
                      .HasForeignKey(o => o.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(o => o.Trip)
                      .WithMany(t => t.Orders)
                      .HasForeignKey(o => o.TripId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Trip>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ReferenceNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).HasConversion<string>();
                
                entity.HasOne(t => t.Cleaner)
                      .WithMany()
                      .HasForeignKey(t => t.CleanerId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(t => t.Driver)
                      .WithMany()
                      .HasForeignKey(t => t.AssignedDriverId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Cleaner>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ItemType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ServiceType).HasConversion<string>();
                
                entity.HasOne(i => i.Order)
                      .WithMany(o => o.Items)
                      .HasForeignKey(i => i.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ItemType>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TypeName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IronPrice).HasPrecision(18, 2);
                entity.Property(e => e.IronCost).HasPrecision(18, 2);
                entity.Property(e => e.CleaningPrice).HasPrecision(18, 2);
                entity.Property(e => e.CleaningCost).HasPrecision(18, 2);
                entity.Property(e => e.BothPrice).HasPrecision(18, 2);
                entity.Property(e => e.BothCost).HasPrecision(18, 2);

                entity.HasData(
                    new ItemType
                    {
                        Id = Guid.Parse("f9e1e1e1-1234-4a5b-bcde-111111111111"),
                        TypeName = "قميص",
                        IronPrice = 0.50m,
                        IronCost = 0.20m,
                        CleaningPrice = 0.75m,
                        CleaningCost = 0.30m,
                        BothPrice = 1.00m,
                        BothCost = 0.40m,
                        IsDeleted = false
                    },
                    new ItemType
                    {
                        Id = Guid.Parse("f9e1e1e1-1234-4a5b-bcde-222222222222"),
                        TypeName = "بنطلون",
                        IronPrice = 0.75m,
                        IronCost = 0.30m,
                        CleaningPrice = 1.00m,
                        CleaningCost = 0.40m,
                        BothPrice = 1.25m,
                        BothCost = 0.50m,
                        IsDeleted = false
                    },
                    new ItemType
                    {
                        Id = Guid.Parse("f9e1e1e1-1234-4a5b-bcde-333333333333"),
                        TypeName = "بدلة رجالية",
                        IronPrice = 2.50m,
                        IronCost = 1.00m,
                        CleaningPrice = 3.50m,
                        CleaningCost = 1.50m,
                        BothPrice = 5.00m,
                        BothCost = 2.00m,
                        IsDeleted = false
                    },
                    new ItemType
                    {
                        Id = Guid.Parse("f9e1e1e1-1234-4a5b-bcde-444444444444"),
                        TypeName = "فستان سهرة",
                        IronPrice = 4.00m,
                        IronCost = 1.50m,
                        CleaningPrice = 8.00m,
                        CleaningCost = 3.00m,
                        BothPrice = 12.00m,
                        BothCost = 5.00m,
                        IsDeleted = false
                    },
                    new ItemType
                    {
                        Id = Guid.Parse("f9e1e1e1-1234-4a5b-bcde-555555555555"),
                        TypeName = "جاكيت",
                        IronPrice = 1.50m,
                        IronCost = 0.60m,
                        CleaningPrice = 2.00m,
                        CleaningCost = 0.80m,
                        BothPrice = 2.50m,
                        BothCost = 1.00m,
                        IsDeleted = false
                    },
                    new ItemType
                    {
                        Id = Guid.Parse("f9e1e1e1-1234-4a5b-bcde-666666666666"),
                        TypeName = "لحاف/بطانية كبير",
                        IronPrice = 0.00m,
                        IronCost = 0.00m,
                        CleaningPrice = 6.00m,
                        CleaningCost = 2.50m,
                        BothPrice = 6.00m,
                        BothCost = 2.50m,
                        IsDeleted = false
                    },
                    new ItemType
                    {
                        Id = Guid.Parse("f9e1e1e1-1234-4a5b-bcde-777777777777"),
                        TypeName = "ثوب/دشداشة",
                        IronPrice = 1.00m,
                        IronCost = 0.40m,
                        CleaningPrice = 1.25m,
                        CleaningCost = 0.50m,
                        BothPrice = 1.75m,
                        BothCost = 0.70m,
                        IsDeleted = false
                    }
                );
            });

            modelBuilder.Entity<UserLocation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<Driver>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
            });

            modelBuilder.Entity<MarketingCode>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.Code).IsUnique();
                entity.Property(e => e.DiscountPercentage).HasPrecision(5, 2);
                entity.Property(e => e.SharePercentage).HasPrecision(5, 2);
                entity.Property(e => e.MarketerName).IsRequired().HasMaxLength(100);
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasOne(o => o.MarketingCode)
                      .WithMany()
                      .HasForeignKey(o => o.MarketingCodeId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.Property(e => e.MarketingDiscount).HasPrecision(18, 2);
                entity.Property(e => e.MarketerShare).HasPrecision(18, 2);
            });
        }
    }
}
