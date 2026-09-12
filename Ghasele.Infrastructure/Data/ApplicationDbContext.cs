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
        public DbSet<CleanerItemPrice> CleanerItemPrices { get; set; }
        public DbSet<UserLocation> UserLocations { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<MarketingCode> MarketingCodes { get; set; }
        public DbSet<PendingRegistration> PendingRegistrations { get; set; }
        public DbSet<AppSettings> AppSettings { get; set; }
        public DbSet<DeliveryWindow> DeliveryWindows { get; set; }

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
                entity.Property(e => e.Role).HasConversion<string>();
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
                entity.Property(e => e.Type).HasConversion<string>().HasMaxLength(20);

                // Every guest opening the Orders tab filters on this, so it needs to be an
                // index lookup rather than a scan of the whole Orders table.
                entity.HasIndex(e => e.DeviceToken);

                // Same 500 as Users.FcmToken - it holds the same kind of value.
                entity.Property(e => e.FcmToken).HasMaxLength(500);

                // UserId is optional so a guest order can exist with no user row. Cascade stays
                // explicit: deleting an account still removes its orders, which the account
                // deletion flow relies on. Guest orders have no owner to cascade from.
                entity.HasOne(o => o.User)
                      .WithMany()
                      .HasForeignKey(o => o.UserId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(o => o.Trip)
                      .WithMany(t => t.Orders)
                      .HasForeignKey(o => o.TripId)
                      .OnDelete(DeleteBehavior.SetNull);

                // Restrict, not SetNull or Cascade: a booked order must keep pointing at the
                // window it was scheduled into. Deleting a window with orders on it should
                // fail loudly rather than silently unschedule them - the operator can
                // deactivate it instead, which hides it from new bookings and keeps history.
                entity.HasOne(o => o.DeliveryWindow)
                      .WithMany()
                      .HasForeignKey(o => o.DeliveryWindowId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.Restrict);

                // The customer app asks "what is booked from today onwards" on every slot
                // fetch, and dispatch groups the day's orders by window.
                entity.HasIndex(e => new { e.DeliveryWindowId, e.ScheduledDate });
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
                entity.Property(e => e.NameAr).IsRequired().HasMaxLength(100);
                entity.Property(e => e.NameEn).IsRequired().HasMaxLength(100);
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
                entity.Property(e => e.TypeNameAr).IsRequired().HasMaxLength(100);
                entity.Property(e => e.TypeNameEn).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IronPrice).HasPrecision(18, 2);
                entity.Property(e => e.CleaningPrice).HasPrecision(18, 2);
                entity.Property(e => e.BothPrice).HasPrecision(18, 2);

                entity.HasData(
                    new ItemType
                    {
                        Id = Guid.Parse("f9e1e1e1-1234-4a5b-bcde-111111111111"),
                        TypeNameAr = "قميص",
                        TypeNameEn = "Shirt",
                        IronPrice = 0.50m,
                        CleaningPrice = 0.75m,
                        BothPrice = 1.00m,
                        IsDeleted = false
                    },
                    new ItemType
                    {
                        Id = Guid.Parse("f9e1e1e1-1234-4a5b-bcde-222222222222"),
                        TypeNameAr = "بنطلون",
                        TypeNameEn = "Trousers",
                        IronPrice = 0.75m,
                        CleaningPrice = 1.00m,
                        BothPrice = 1.25m,
                        IsDeleted = false
                    },
                    new ItemType
                    {
                        Id = Guid.Parse("f9e1e1e1-1234-4a5b-bcde-333333333333"),
                        TypeNameAr = "بدلة رجالية",
                        TypeNameEn = "Men's Suit",
                        IronPrice = 2.50m,
                        CleaningPrice = 3.50m,
                        BothPrice = 5.00m,
                        IsDeleted = false
                    },
                    new ItemType
                    {
                        Id = Guid.Parse("f9e1e1e1-1234-4a5b-bcde-444444444444"),
                        TypeNameAr = "فستان سهرة",
                        TypeNameEn = "Evening Dress",
                        IronPrice = 4.00m,
                        CleaningPrice = 8.00m,
                        BothPrice = 12.00m,
                        IsDeleted = false
                    },
                    new ItemType
                    {
                        Id = Guid.Parse("f9e1e1e1-1234-4a5b-bcde-555555555555"),
                        TypeNameAr = "جاكيت",
                        TypeNameEn = "Jacket",
                        IronPrice = 1.50m,
                        CleaningPrice = 2.00m,
                        BothPrice = 2.50m,
                        IsDeleted = false
                    },
                    new ItemType
                    {
                        Id = Guid.Parse("f9e1e1e1-1234-4a5b-bcde-666666666666"),
                        TypeNameAr = "لحاف/بطانية كبير",
                        TypeNameEn = "Large Blanket",
                        IronPrice = 0.00m,
                        CleaningPrice = 6.00m,
                        BothPrice = 6.00m,
                        IsDeleted = false
                    },
                    new ItemType
                    {
                        Id = Guid.Parse("f9e1e1e1-1234-4a5b-bcde-777777777777"),
                        TypeNameAr = "ثوب/دشداشة",
                        TypeNameEn = "Thobe",
                        IronPrice = 1.00m,
                        CleaningPrice = 1.25m,
                        BothPrice = 1.75m,
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
                entity.Property(e => e.NameAr).IsRequired().HasMaxLength(100);
                entity.Property(e => e.NameEn).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);

                entity.HasOne(d => d.User)
                      .WithMany()
                      .HasForeignKey(d => d.UserId)
                      .OnDelete(DeleteBehavior.SetNull);
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

            modelBuilder.Entity<PendingRegistration>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.PhoneNumber).IsUnique();
                // Password/name are only supplied by the final step, so both are nullable
                // while the signup is still at the phone/OTP stage.
                entity.Property(e => e.PasswordHash);
                entity.Property(e => e.FullName).HasMaxLength(100);
                entity.Property(e => e.Otp).IsRequired().HasMaxLength(6);
            });

            modelBuilder.Entity<AppSettings>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NormalDeliveryPrice).HasPrecision(18, 2);
                entity.Property(e => e.ExpressDeliveryPrice).HasPrecision(18, 2);

                // Both seeded at the 1.00 that used to be hardcoded in OrderService, so
                // deploying this changes no prices until an admin edits them.
                // Fully qualified: the DbSet property above is also called AppSettings and
                // would otherwise win name resolution here.
                entity.HasData(new AppSettings
                {
                    Id = Ghasele.Domain.Entities.AppSettings.SingletonId,
                    NormalDeliveryPrice = 1.00m,
                    ExpressDeliveryPrice = 1.00m,
                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                });
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

            modelBuilder.Entity<SupportTicket>(entity =>
            {
                // Same reason as Orders.DeviceToken: it is how a guest finds their own tickets.
                entity.HasIndex(e => e.DeviceToken);
            });

            modelBuilder.Entity<CleanerItemPrice>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.IronPrice).HasPrecision(18, 2);
                entity.Property(e => e.CleaningPrice).HasPrecision(18, 2);
                entity.Property(e => e.BothPrice).HasPrecision(18, 2);

                // One agreed rate per item type per cleaner. Enforced in the database as well as
                // the save path: two rows for the same pair would make "what do we pay for a
                // shirt here" ambiguous, and pricing would silently pick whichever came first.
                entity.HasIndex(e => new { e.CleanerId, e.ItemTypeId }).IsUnique();

                // Removing a laundry takes its rate card with it - the rates mean nothing
                // without the cleaner, and orders already priced keep their own copy.
                entity.HasOne(e => e.Cleaner)
                      .WithMany()
                      .HasForeignKey(e => e.CleanerId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Item types are soft-deleted (ItemType.IsDeleted), so this never fires in
                // practice; Cascade keeps the table consistent if one is ever hard-deleted.
                entity.HasOne(e => e.ItemType)
                      .WithMany()
                      .HasForeignKey(e => e.ItemTypeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                // The two figures the line was priced with, frozen at collection time.
                entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
                entity.Property(e => e.UnitCleanerPrice).HasPrecision(18, 2);
            });

            modelBuilder.Entity<DeliveryWindow>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Capacity).HasDefaultValue(20);
                entity.HasIndex(e => e.StartTime);

                // The three windows the operator asked for out of the box; fully editable
                // from the dashboard afterwards.
                var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                entity.HasData(
                    new DeliveryWindow
                    {
                        Id = Guid.Parse("d0000001-0000-4000-8000-000000000001"),
                        StartTime = new TimeOnly(13, 0), EndTime = new TimeOnly(14, 0),
                        Capacity = 20, IsActive = true, CreatedAt = seedDate
                    },
                    new DeliveryWindow
                    {
                        Id = Guid.Parse("d0000001-0000-4000-8000-000000000002"),
                        StartTime = new TimeOnly(15, 0), EndTime = new TimeOnly(16, 0),
                        Capacity = 20, IsActive = true, CreatedAt = seedDate
                    },
                    new DeliveryWindow
                    {
                        Id = Guid.Parse("d0000001-0000-4000-8000-000000000003"),
                        StartTime = new TimeOnly(19, 0), EndTime = new TimeOnly(20, 0),
                        Capacity = 20, IsActive = true, CreatedAt = seedDate
                    });
            });
        }
    }
}
