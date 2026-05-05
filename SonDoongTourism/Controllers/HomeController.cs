using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SonDoongTourism.Data;
using SonDoongTourism.Models; // Đảm bảo đúng tên namespace của bạn
using System.Threading.Tasks;

namespace SonDoongTourism.Controllers
{
    public class HomeController : Controller
    {
        // Khai báo biến để kết nối Database
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Sửa lại hàm Index này
        public async Task<IActionResult> Index()
{
    // 1. Kiểm tra nếu Database chưa có Tour nào thì tự động thêm 3 Tour mẫu
    if (!_context.Tours.Any())
    {
        _context.Tours.AddRange(
            new Tour
            {
                Name = "Thám Hiểm Sơn Đoòng",
                Description = "Hành trình thám hiểm đỉnh cao 4 ngày 3 đêm.",
                Price = 7200000,
                DurationDays = 4,
                ImageUrl = "/img/sondoong.jpg"
            },
            new Tour
            {
                Name = "Khám Phá Hang Én",
                Description = "Cắm trại tại bãi cát tuyệt đẹp dưới vòm hang khổng lồ.",
                Price = 7600000,
                DurationDays = 2,
                ImageUrl = "/img/image_pkr1639446571.jpg"
            },
            new Tour
            {
                Name = "Hệ Thống Hang Tú Làn",
                Description = "Bơi xuyên qua các dòng sông ngầm, ngắm nhìn thạch nhũ.",
                Price = 15000000,
                DurationDays = 3,
                ImageUrl = "/img/tulang.jpg"
            }
        );
        
        // Lưu dữ liệu vào Database
        await _context.SaveChangesAsync();
    }

    // 2. Lấy danh sách Tour ra và gửi sang Giao diện (View)
    var tours = await _context.Tours.ToListAsync();
    return View(tours);
}

        public IActionResult Privacy()
        {
            return View();
        }
        // 1. Hiển thị form Đặt Tour
        public async Task<IActionResult> Book(int id)
        {
            var tour = await _context.Tours.FindAsync(id);
            if (tour == null) return NotFound();

            ViewBag.TourName = tour.Name;
            ViewBag.TourPrice = tour.Price;
            return View(new Booking { TourId = id });
        }

       // Xử lý khi khách hàng bấm nút Đặt Tour
        // 1. Hàm hiển thị form (GET)
        [HttpGet]
        public async Task<IActionResult> book(int id)
        {
            var tour = await _context.Tours.FindAsync(id);
            if (tour == null) 
            {
                return NotFound("Không tìm thấy Tour này!");
            }

            // Phải có 2 dòng này để truyền tên và giá sang giao diện
            ViewBag.TourName = tour.Name;
            ViewBag.TourPrice = tour.Price;
            
            // Bắt buộc phải có (new Booking...) ở đây để Model không bị Null
            return View(new Booking { TourId = id });
        }

        // 2. Hàm xử lý lưu vào Database (POST)
        [HttpPost]
        public async Task<IActionResult> Book(Booking booking)
        {
            // Bỏ qua kiểm tra lỗi của thuộc tính Tour (Khóa ngoại) vì mình không nhập từ form
            booking.Id = 0;
            ModelState.Remove("Tour");
            ModelState.Remove("Id");
            if (ModelState.IsValid)
            {
                
                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction("Success");
            }

            // Nếu nhập sai/nhập thiếu thì load lại form, cấp lại ViewBag
            var tour = await _context.Tours.FindAsync(booking.TourId);
            if (tour != null)
            {
                ViewBag.TourName = tour.Name;
                ViewBag.TourPrice = tour.Price;
            }
            return View(booking);
        }

public IActionResult Details(int id)
{
    // Tìm tour trong Database có Id khớp với Id khách hàng vừa bấm
    var tour = _context.Tours.FirstOrDefault(t => t.Id == id);
    
    // Nếu không tìm thấy thì báo lỗi 404
    if (tour == null)
    {
        return NotFound(); 
    }
    
    // Tìm thấy thì gói dữ liệu gửi sang màn hình giao diện
    return View(tour);
}
public IActionResult Success()
{
    return View();
}
public IActionResult BookingList()
{
    // Lấy toàn bộ danh sách khách hàng đã đặt tour từ Database
    var danhSachDatTour = _context.Bookings.ToList();
    
    // Gửi danh sách này sang giao diện để hiển thị
    return View(danhSachDatTour);
}


}
}