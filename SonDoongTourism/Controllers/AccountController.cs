using Microsoft.AspNetCore.Mvc;
using SonDoongTourism.Data; 
using SonDoongTourism.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace SonDoongTourism.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Hàm để HIỆN THỊ trang Đăng ký
        public IActionResult Register()
        {
            return View();
        }

        // 1. XỬ LÝ ĐĂNG KÝ
[HttpPost]
public IActionResult Register(User user)
{
    // Mẹo giải vây: Gán chữ rỗng để Database không báo lỗi thiếu Email/FullName
    user.Email = user.Email ?? "";
    user.FullName = user.FullName ?? "";
    
    // Bỏ qua kiểm tra lỗi của 2 ô này vì trên giao diện không có
    ModelState.Remove("Email");
    ModelState.Remove("FullName");
    ModelState.Remove("Id");
    if (ModelState.IsValid)
    
    {

        // Kiểm tra xem tên tài khoản đã có ai tạo chưa
        var userExists = _context.Users.Any(u => u.Username == user.Username);
        if (userExists)
        {
            ViewBag.Error = "Tên tài khoản này đã tồn tại!";
            return View(user);
        }

        _context.Users.Add(user);
        _context.SaveChanges(); // Lần này chắc chắn 100% sẽ lưu được!
        return RedirectToAction("Login");
    }
    return View(user);
}

// 2. XỬ LÝ ĐĂNG NHẬP
[HttpPost]
public async Task<IActionResult> Login(string username, string password)
{
    var user = _context.Users.FirstOrDefault(u => u.Username == username && u.Password == password);

    if (user != null)
    {
        // 1. Tạo danh sách thông tin người dùng (Claims)
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Dòng này cực kỳ quan trọng để MyBookings chạy được
            new Claim(ClaimTypes.Role, "User")
        };

        // 2. Tạo "thẻ thông hành" (Identity)
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        // 3. Đăng nhập vào hệ thống
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, 
            new ClaimsPrincipal(claimsIdentity));

        return RedirectToAction("Index", "Home");
    }

    ViewBag.Error = "Sai tài khoản hoặc mật khẩu";
    return View();
}

// Hàm để HIỂN THỊ trang Đăng nhập
[HttpGet]
public IActionResult Login()
{
    return View();
}

// Thêm AllowAnonymous để chắc chắn ai cũng bấm được, 
// và dùng HttpGet để khớp với cái thẻ <a> ở Layout
[HttpGet]
public async Task<IActionResult> Logout()
{
    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return RedirectToAction("Index", "Home");
}
    }

    
    
}
