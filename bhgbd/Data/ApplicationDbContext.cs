using bhgbd.Models;
using Microsoft.EntityFrameworkCore;

namespace bhgbd.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Staff> Staffs { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Tên bảng DB
            modelBuilder.Entity<Category>().ToTable("categories");
            modelBuilder.Entity<Product>().ToTable("products");
            modelBuilder.Entity<ProductVariant>().ToTable("productvariants");
            modelBuilder.Entity<Customer>().ToTable("customers");
            modelBuilder.Entity<Staff>().ToTable("staffs");
            modelBuilder.Entity<Admin>().ToTable("admins");
            modelBuilder.Entity<User>().ToTable("users");

            // 2. Cấu hình Enum sang String
            modelBuilder.Entity<Customer>()
                .Property(c => c.gender)
                .HasConversion<string>();

            modelBuilder.Entity<Staff>()
                .Property(c => c.gender)
                .HasConversion<string>();

            modelBuilder.Entity<Admin>()
                .Property(c => c.gender)
                .HasConversion<string>();

            modelBuilder.Entity<User>()
                .Property(u => u.role)
                .HasConversion<string>();

            modelBuilder.Entity<Order>()
                .Property(o => o.status)
                .HasConversion<string>();

            // 3. Cấu hình Khóa chính tự tăng cho Customer
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.customerId);
                entity.Property(e => e.customerId)
                      .ValueGeneratedOnAdd();
            });

            // 4. Cấu hình Quan hệ Khóa ngoại với User (ĐÃ SỬA LỖI XUNG ĐỘT)
            // Admin (1 - 1 với User qua adminId = userId)
            modelBuilder.Entity<Admin>()
                .HasKey(a => a.adminId);
            modelBuilder.Entity<Admin>()
                .HasOne(a => a.User)
                .WithOne()
                .HasForeignKey<Admin>(a => a.adminId);

            // Staff (1 - 1 với User qua staffId = userId)
            modelBuilder.Entity<Staff>()
                .HasKey(s => s.staffId);
            modelBuilder.Entity<Staff>()
                .HasOne(s => s.User)
                .WithOne()
                .HasForeignKey<Staff>(s => s.staffId);

            // Customer (Quan hệ 1 - 1 / 1 - Many với User qua KHÓA NGOẠI userId)
            modelBuilder.Entity<Customer>()
                .HasOne(c => c.User)
                .WithMany() // Nếu User không có ICollection<Customer>
                .HasForeignKey(c => c.userId) // BẮT BUỘC KHÓA NGOẠI LÀ userId, KHÔNG PHẢI customerId
                .OnDelete(DeleteBehavior.Cascade);

            // 5. Cấu hình khóa ngoại Sản phẩm
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.categoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductVariant>()
                .HasOne(pv => pv.Product)
                .WithMany(p => p.ProductVariants)
                .HasForeignKey(pv => pv.productId)
                .OnDelete(DeleteBehavior.Cascade);

            // 6. Cấu hình khóa ngoại Đơn hàng
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany()
                .HasForeignKey(o => o.customerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.orderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.ProductVariant)
                .WithMany()
                .HasForeignKey(od => od.variantId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}