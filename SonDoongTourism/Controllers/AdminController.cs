using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SonDoongTourism.Models; // Thay bằng tên project của bạn
using SonDoongTourism.Data;
using System.IO;

namespace SonDoongTourism.Controllers
{

    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. DASHBOARD: Thống kê tổng quan
        public async Task<IActionResult> Index()
        {
            ViewBag.TotalTours = await _context.Tours.CountAsync();
            ViewBag.TotalBookings = await _context.Bookings.CountAsync();
            ViewBag.TotalBookings = await _context.Bookings.Where(b => b.Status == "Đã duyệt").CountAsync();
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            // Tính tổng doanh thu
            ViewBag.TotalRevenue = await _context.Bookings.Where(b => b.Status == "Đã duyệt").SumAsync(b => b.Tour.Price); 
            
            return View();
        }

        // 2. QUẢN LÝ TOUR (Danh sách sản phẩm)
        public async Task<IActionResult> ManageTours()
        {
            var tours = await _context.Tours.ToListAsync();
            return View(tours);
        }

        // Thêm Tour mới (GET)
        public IActionResult CreateTour() => View();

        // Thêm Tour mới (POST)
        [HttpPost]
        public async Task<IActionResult> CreateTour(Tour tour)
        {
            if (ModelState.IsValid)
            {
                _context.Tours.Add(tour);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ManageTours));
            }
            return View(tour);
        }

        // 3. QUẢN LÝ ĐƠN HÀNG (Booking)
        public async Task<IActionResult> ManageBookings()
        {
            var bookings = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Tour)
                .OrderByDescending(b => b.Id)
                .ToListAsync();
            return View(bookings);
        }

        // Xóa đơn hàng hoặc cập nhật trạng thái
        [HttpPost]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ManageBookings));
        }

        [HttpPost]
    public async Task<IActionResult> ApproveBooking(int id)
    {
        // Tìm đơn hàng dựa vào ID
        var booking = await _context.Bookings.FindAsync(id);
        
        if (booking != null && booking.Status == "Chờ duyệt")
        {
            // Chuyển trạng thái sang Đã duyệt
            booking.Status = "Đã duyệt";
            booking.PaymentStatus = "Đã thanh toán"; 
            
            await _context.SaveChangesAsync();
        }
        
        // Quay lại trang danh sách đơn
        return RedirectToAction("ManageBookings");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteTour(int id)
{
    // Tìm tour trong database theo ID
    var tour = await _context.Tours.FindAsync(id);
    
    if (tour != null)
    {
        _context.Tours.Remove(tour); // Ra lệnh xóa
        await _context.SaveChangesAsync(); // Lưu thay đổi vào SQL Server
    }

    // SAU KHI XÓA XONG: Chuyển hướng về trang quản lý để cập nhật danh sách
    // (Đảm bảo tên hàm là ManageTours giống trong ảnh của bạn)
    return RedirectToAction("ManageTours"); 
}
    

    [HttpPost]
public async Task<IActionResult> CreateTour(Tour tour, IFormFile imageFile) // Thêm tham số IFormFile
{
    // 1. Kiểm tra xem người dùng có chọn ảnh không
    if (imageFile != null && imageFile.Length > 0)
    {
        // 2. Tạo tên file mới (thêm mã ngẫu nhiên để không bị trùng tên)
        var fileName = Path.GetFileNameWithoutExtension(imageFile.FileName) + "_" + Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
        
        // 3. Đường dẫn lưu file (Lưu vào thư mục wwwroot/images/)
        var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
        
        // Tạo thư mục nếu chưa có
        if (!Directory.Exists(imagePath)) {
            Directory.CreateDirectory(imagePath);
        }

        var exactPath = Path.Combine(imagePath, fileName);

        // 4. Copy file ảnh từ máy tính vào thư mục web
        using (var stream = new FileStream(exactPath, FileMode.Create))
        {
            await imageFile.CopyToAsync(stream);
        }

        // 5. Lưu đường dẫn ảnh vào Database
        tour.ImageUrl = "/images/" + fileName; 
    }

    // Lưu Tour vào CSDL
    _context.Tours.Add(tour);
    await _context.SaveChangesAsync();
    
    return RedirectToAction("ManageTours"); // Quay lại trang quản lý
    }

    
}
}