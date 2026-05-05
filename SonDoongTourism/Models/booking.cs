using System;
using System.ComponentModel.DataAnnotations;

namespace SonDoongTourism.Models
{
    public class Booking
    {
        [Key] // Thêm dòng này để Id tự động tăng, không lo bị trùng
    
        public int Id { get; set; }
        
        public int TourId { get; set; }
        public Tour Tour { get; set; } // Khóa ngoại liên kết với Tour
        
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [Display(Name = "Họ và Tên")]
        public string? CustomerName { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string? Phone { get; set; }
        
        [Display(Name = "Ngày đặt")]
        public DateTime BookingDate { get; set; } = DateTime.Now;

        public int UserId { get; set; }
        public virtual User User { get; set; }
        
        public string? PaymentMethod { get; set; } = "Tiền mặt"; 

        public string? PaymentStatus { get; set; } = "Chưa thanh toán";

        public string? Status { get; set; } = "Chờ duyệt";
    }
}