using Microsoft.EntityFrameworkCore;
using SonDoongTourism.Models;


namespace SonDoongTourism.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Tạo các bảng trong Database
        public DbSet<Tour> Tours { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        // Thêm dòng này vào file ApplicationDbContext.cs
        public DbSet<Wishlist> Wishlists { get; set; }
public DbSet<User> Users { get; set; }
    }
}