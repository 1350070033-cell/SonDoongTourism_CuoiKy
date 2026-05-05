using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SonDoongTourism.Data;
using SonDoongTourism.Models;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace SonDoongTourism.Controllers
{
    public class TourController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TourController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Hiển thị danh sách Tour
        public async Task<IActionResult> Index()
        {
            var tours = await _context.Tours.ToListAsync();
            return View(tours);
        }

        // Hiển thị chi tiết 1 Tour và Form đặt tour
        public async Task<IActionResult> Details(int id)
        {
            var tour = await _context.Tours.FirstOrDefaultAsync(m => m.Id == id);
            if (tour == null) return NotFound();

            ViewBag.Tour = tour; // Truyền dữ liệu tour sang View
            return View(new Booking { TourId = tour.Id });
        }

        // Xử lý khi khách hàng bấm nút "Đặt Tour"
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(Booking booking)
        {
            booking.Id = 0;

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier); 
            if (int.TryParse(userIdString, out int currentUserId))
            {
                booking.UserId = currentUserId; // Nhét ID vào đơn hàng
            }
            else
            {
                // Nếu không có ID (chưa đăng nhập), bắt quay lại trang chủ
                return RedirectToAction("Login", "Account");
            }
            
            // THÊM 2 DÒNG NÀY: Bỏ qua kiểm tra các đối tượng liên kết
            ModelState.Remove("Tour");
            ModelState.Remove("User");

            if (ModelState.IsValid)
            {
                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();
                
                // Mình khuyên bạn nên đổi Redirect sang trang Lịch sử luôn cho trực quan
                return RedirectToAction("MyBookings", new { userId = booking.UserId });
            }
            
            // Dành cho trường hợp form có lỗi, quay lại trang chi tiết
            var tour = await _context.Tours.FindAsync(booking.TourId);
            ViewBag.Tour = tour;
            return View("Details", booking);
        }

        // Trang thông báo đặt thành công
        public IActionResult Success()
        {
            return View();
        }

        // Xem lịch sử đơn hàng của khách
    [Authorize]
public async Task<IActionResult> MyBookings()
{
    // 1. TỰ ĐỘNG LẤY ID CỦA NGƯỜI ĐANG ĐĂNG NHẬP (Lấy từ Cookie bảo mật)
    var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

    // 2. Kiểm tra xem có lấy được ID không
    if (string.IsNullOrEmpty(userIdString))
    {
        return RedirectToAction("Login", "Account"); // Không có thì bắt đăng nhập lại
    }

    int userId = int.Parse(userIdString);

    // 3. Tìm các đơn hàng thuộc về đúng người dùng đó
    var myBookings = await _context.Bookings
        .Include(b => b.Tour) // Lấy kèm thông tin Tour
        .Where(b => b.UserId == userId)
        .OrderByDescending(b => b.Id)
        .ToListAsync();

    // 4. Trả dữ liệu ra giao diện
    return View(myBookings);
}

    // Khách hàng bấm Hủy đơn
    [HttpPost]
    public async Task<IActionResult> CancelBooking(int bookingId)
    {
        var booking = await _context.Bookings.FindAsync(bookingId);
        
        // Chỉ cho phép khách tự hủy khi admin chưa duyệt
        if (booking != null && booking.Status == "Chờ duyệt") 
        {
            booking.Status = "Đã hủy";
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("MyBookings", new { userId = booking?.UserId });
    }

    [Authorize]
    [HttpPost]
// CHÚ Ý: Đã bỏ tham số "int userId" đi, chỉ giữ lại "int tourId"
public async Task<IActionResult> AddToWishlist(int tourId) 
{
    // 1. Tự động lấy ID người dùng từ thẻ nhớ (Cookie)
    var userIdString = User.FindFirstValue("UserId"); 
    
    // Kiểm tra xem đã lấy được ID chưa (đã đăng nhập chưa)
    if (int.TryParse(userIdString, out int currentUserId))
    {
        // 2. Kiểm tra xem tour này đã có trong danh sách yêu thích của người này chưa
        var exists = await _context.Wishlists.AnyAsync(w => w.TourId == tourId && w.UserId == currentUserId);
        Console.WriteLine($"---> KIỂM TRA: TourId nhận được = {tourId}");
        Console.WriteLine($"---> KIỂM TRA: UserId lấy từ Cookie = {currentUserId}");
        if (!exists)
        {
            // Dùng currentUserId lấy được từ Cookie để lưu vào Database
            var wishItem = new Wishlist { TourId = tourId, UserId = currentUserId };
            _context.Wishlists.Add(wishItem);
            await _context.SaveChangesAsync(); // <-- Sẽ không còn báo lỗi FOREIGN KEY nữa!
        }

        return RedirectToAction("Details", new { id = tourId }); // Quay lại trang chi tiết tour
    }
    else
    {
        // Nếu chưa đăng nhập, chuyển hướng về trang Đăng nhập
        return RedirectToAction("Login", "Account");
    }
    
}
    }
}