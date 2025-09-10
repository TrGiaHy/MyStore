using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Model.DBContext
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Products> Products { get; set; }
        public DbSet<Categories> Categories { get; set; }
        public DbSet<Orders> Orders { get; set; }
        public DbSet<OrderItems> OrderItems { get; set; }
        public DbSet<ShoppingCarts> ShoppingCarts { get; set; }
        public DbSet<CartItems> CartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Bỏ tiền tố AspNet của các bảng: mặc định
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                var tableName = entityType.GetTableName();
                if (tableName.StartsWith("AspNet"))
                {
                    entityType.SetTableName(tableName.Substring(6));
                }
            }

            // Relationships config
            builder.Entity<Products>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryID)
                .OnDelete(DeleteBehavior.NoAction);


            builder.Entity<ProductImages>()
                .HasOne(pi => pi.Product)
                .WithMany(p => p.ProductImages)
                .HasForeignKey(pi => pi.ProductID)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<CartItems>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<CartItems>()
                .HasOne(ci => ci.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<OrderItems>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<OrderItems>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.NoAction);
            // 1. Seed Roles
            var customerRole = new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Customer",
                NormalizedName = "CUSTOMER"
            };
            var sellerRole = new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Seller",
                NormalizedName = "SELLER"
            };

            builder.Entity<IdentityRole>().HasData(customerRole, sellerRole);

            // 2. Seed Users
            var hasher = new PasswordHasher<AppUser>();

            var user1 = new AppUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "customer1",
                NormalizedUserName = "CUSTOMER1",
                Email = "customer1@email.com",
                NormalizedEmail = "CUSTOMER1@EMAIL.COM",
                EmailConfirmed = true,
                FullName = "Nguyen Van A",
                Address = "123 Đường ABC, Quận 1, TP.HCM",
                IsActive = true,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            user1.PasswordHash = hasher.HashPassword(user1, "Password123!");

            var user2 = new AppUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "seller1",
                NormalizedUserName = "SELLER1",
                Email = "seller1@email.com",
                NormalizedEmail = "SELLER1@EMAIL.COM",
                EmailConfirmed = true,
                FullName = "Tran Thi B",
                Address = "456 Đường XYZ, Quận 3, TP.HCM",
                IsActive = true,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            user2.PasswordHash = hasher.HashPassword(user2, "Password123!");

            var user3 = new AppUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "giahuy",
                NormalizedUserName = "GIAHUY",
                Email = "giahuy@email.com",
                NormalizedEmail = "GIAHUY@EMAIL.COM",
                EmailConfirmed = true,
                FullName = "Tran Gia Huy",
                Address = "456 Đường XYZ, Quận 3, TP.HCM",
                IsActive = true,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            user3.PasswordHash = hasher.HashPassword(user3, "1");

            builder.Entity<AppUser>().HasData(user1, user2, user3);

            // 3. Seed UserRoles (gán user vào role)
            builder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>
                {
                    UserId = user1.Id,
                    RoleId = customerRole.Id
                },
                new IdentityUserRole<string>
                {
                    UserId = user2.Id,
                    RoleId = sellerRole.Id
                }
            );
            // Seed Categories
            var category1 = new Categories
            {
                CategoryId = Guid.NewGuid(),
                Name = "Điện thoại",
                Description = "Các loại điện thoại thông minh",
                IsActive = true
            };
            var category2 = new Categories
            {
                CategoryId = Guid.NewGuid(),
                Name = "Laptop",
                Description = "Máy tính xách tay các loại",
                IsActive = true
            };

            builder.Entity<Categories>().HasData(category1, category2);

            // Seed Products
            builder.Entity<Products>().HasData(
                new Products
                {
                    ProductId = Guid.NewGuid(),
                    Name = "iPhone 15 Pro",
                    Description = "Điện thoại Apple mới nhất",
                    Price = 29990000,
                    StockQuantity = 10,
                    CreatedDate = DateTime.Now,
                    CategoryID = category1.CategoryId,
                    IsActive = true
                },
                new Products
                {
                    ProductId = Guid.NewGuid(),
                    Name = "Dell XPS 13",
                    Description = "Laptop mỏng nhẹ cao cấp",
                    Price = 25990000,
                    StockQuantity = 5,
                    CreatedDate = DateTime.Now,
                    CategoryID = category2.CategoryId,
                    IsActive = true
                }
            );
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                    => optionsBuilder.UseSqlServer("Server=TRANGIAHUY;Database=MyStore;uid=sa;pwd=1035;encrypt=true;trustServerCertificate=true;");
        //=> optionsBuilder.UseSqlServer("Data Source=SQL9001.site4now.net;Initial Catalog=db_abe0a2_mystore;User Id=db_abe0a2_mystore_admin;Password=GiaHuy@1035");
    }
}
