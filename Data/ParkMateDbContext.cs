using Microsoft.EntityFrameworkCore;
using ParkMate.Models;

namespace ParkMate.Data;

public class ParkMateDbContext : DbContext
{
    public ParkMateDbContext(DbContextOptions<ParkMateDbContext> options)
        : base(options)
    {
    }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<OtpVerification> OtpVerifications => Set<OtpVerification>();
    public DbSet<Login> Logins => Set<Login>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Slot> Slots => Set<Slot>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Receipt> Receipts => Set<Receipt>();
    public DbSet<Feedback> Feedbacks => Set<Feedback>();
    public DbSet<Pricing> Pricings => Set<Pricing>();
    public DbSet<Refund> Refunds => Set<Refund>();
    public DbSet<RefundReceipt> RefundReceipts => Set<RefundReceipt>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ============================================
        // 1. TABLE NAMES
        // ============================================

        modelBuilder.Entity<Role>().ToTable("role");
        modelBuilder.Entity<User>().ToTable("users");
        modelBuilder.Entity<OtpVerification>().ToTable("otp_verification");
        modelBuilder.Entity<Login>().ToTable("login");
        modelBuilder.Entity<Vehicle>().ToTable("vehicle");
        modelBuilder.Entity<Slot>().ToTable("slot");
        modelBuilder.Entity<Booking>().ToTable("booking");
        modelBuilder.Entity<Payment>().ToTable("payment");
        modelBuilder.Entity<Receipt>().ToTable("receipt");
        modelBuilder.Entity<Feedback>().ToTable("feedback");
        modelBuilder.Entity<Pricing>().ToTable("pricing");
        modelBuilder.Entity<Refund>().ToTable("refunds");
        modelBuilder.Entity<RefundReceipt>().ToTable("refund_receipt");


        // ============================================
        // 2. PRIMARY KEYS
        // ============================================

        modelBuilder.Entity<Role>()
            .HasKey(x => x.RoleId);

        modelBuilder.Entity<User>()
            .HasKey(x => x.UId);

        modelBuilder.Entity<OtpVerification>()
            .HasKey(x => x.OtpId);

        modelBuilder.Entity<Login>()
            .HasKey(x => x.LoginId);

        modelBuilder.Entity<Vehicle>()
            .HasKey(x => x.VId);

        modelBuilder.Entity<Slot>()
            .HasKey(x => x.SlotId);

        modelBuilder.Entity<Booking>()
            .HasKey(x => x.BId);

        modelBuilder.Entity<Payment>()
            .HasKey(x => x.PId);

        modelBuilder.Entity<Receipt>()
            .HasKey(x => x.RecId);

        modelBuilder.Entity<Feedback>()
            .HasKey(x => x.FId);

        modelBuilder.Entity<Pricing>()
            .HasKey(x => x.PriceId);

        modelBuilder.Entity<Refund>()
            .HasKey(x => x.RefundId);

        modelBuilder.Entity<RefundReceipt>()
            .HasKey(x => x.RefundRecId);


        // ============================================
        // 3. ROLE → USERS
        // ============================================

        modelBuilder.Entity<User>()
            .HasOne(x => x.Role)
            .WithMany(x => x.Users)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict);


        // ============================================
        // 4. USER → OTP
        // ============================================

        modelBuilder.Entity<OtpVerification>()
            .HasOne(x => x.User)
            .WithMany(x => x.OtpVerifications)
            .HasForeignKey(x => x.UId)
            .OnDelete(DeleteBehavior.Restrict);


        // ============================================
        // 5. USER → LOGIN
        // ============================================

        modelBuilder.Entity<Login>()
            .HasOne(x => x.User)
            .WithMany(x => x.Logins)
            .HasForeignKey(x => x.UId)
            .OnDelete(DeleteBehavior.Restrict);


        // ============================================
        // 6. USER → VEHICLES
        // ============================================

        modelBuilder.Entity<Vehicle>()
            .HasOne(x => x.User)
            .WithMany(x => x.Vehicles)
            .HasForeignKey(x => x.UId)
            .OnDelete(DeleteBehavior.Restrict);


        // ============================================
        // 7. USER → BOOKINGS
        // ============================================

        modelBuilder.Entity<Booking>()
            .HasOne(x => x.User)
            .WithMany(x => x.Bookings)
            .HasForeignKey(x => x.UId)
            .OnDelete(DeleteBehavior.Restrict);


        // ============================================
        // 8. VEHICLE → BOOKINGS
        // ============================================

        modelBuilder.Entity<Booking>()
            .HasOne(x => x.Vehicle)
            .WithMany(x => x.Bookings)
            .HasForeignKey(x => x.VId)
            .OnDelete(DeleteBehavior.Restrict);


        // ============================================
        // 9. SLOT → BOOKINGS
        // ============================================

        modelBuilder.Entity<Booking>()
            .HasOne(x => x.Slot)
            .WithMany(x => x.Bookings)
            .HasForeignKey(x => x.SlotId)
            .OnDelete(DeleteBehavior.Restrict);


        // ============================================
        // 10. BOOKING → PAYMENTS
        // ============================================

        modelBuilder.Entity<Payment>()
            .HasOne(x => x.Booking)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.BId)
            .OnDelete(DeleteBehavior.Restrict);


        // ============================================
        // 11. PAYMENT → RECEIPTS
        // ============================================

        modelBuilder.Entity<Receipt>()
            .HasOne(x => x.Payment)
            .WithMany(x => x.Receipts)
            .HasForeignKey(x => x.PId)
            .OnDelete(DeleteBehavior.Restrict);


        // ============================================
        // 12. BOOKING → FEEDBACK
        // ============================================

        modelBuilder.Entity<Feedback>()
            .HasOne(x => x.Booking)
            .WithMany(x => x.Feedbacks)
            .HasForeignKey(x => x.BId)
            .OnDelete(DeleteBehavior.Restrict);


        // ============================================
        // 13. BOOKING → REFUND
        // ============================================

        modelBuilder.Entity<Refund>()
            .HasOne(x => x.Booking)
            .WithOne(x => x.Refund)
            .HasForeignKey<Refund>(x => x.BId)
            .OnDelete(DeleteBehavior.Restrict);


        // ============================================
        // 14. USER → REFUNDS
        // ============================================

        modelBuilder.Entity<Refund>()
            .HasOne(x => x.User)
            .WithMany(x => x.Refunds)
            .HasForeignKey(x => x.UId)
            .OnDelete(DeleteBehavior.Restrict);


        // ============================================
        // 15. REFUND → REFUND RECEIPT
        // ============================================

        modelBuilder.Entity<RefundReceipt>()
            .HasOne(x => x.Refund)
            .WithOne(x => x.RefundReceipt)
            .HasForeignKey<RefundReceipt>(x => x.RefundId)
            .OnDelete(DeleteBehavior.Restrict);


        // ============================================
        // 16. UNIQUE FIELDS
        // ============================================

        modelBuilder.Entity<User>()
            .HasIndex(x => x.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(x => x.Contact)
            .IsUnique();

        modelBuilder.Entity<Vehicle>()
            .HasIndex(x => x.VNum)
            .IsUnique();

        modelBuilder.Entity<Slot>()
            .HasIndex(x => x.SlotNum)
            .IsUnique();

        modelBuilder.Entity<Pricing>()
            .HasIndex(x => x.VType)
            .IsUnique();

        modelBuilder.Entity<Refund>()
            .HasIndex(x => x.BId)
            .IsUnique();

        modelBuilder.Entity<RefundReceipt>()
            .HasIndex(x => x.RefundId)
            .IsUnique();

        modelBuilder.Entity<Receipt>()
            .HasIndex(x => x.RecNo)
            .IsUnique();

        modelBuilder.Entity<RefundReceipt>()
            .HasIndex(x => x.RecNo)
            .IsUnique();
    }
}