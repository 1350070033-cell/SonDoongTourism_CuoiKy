using System.ComponentModel.DataAnnotations;

namespace SonDoongTourism.Models
{
    public class Wishlist
    {
        [Key]
        public int Id { get; set; }
        
        // Ai là người thích?
        public int UserId { get; set; }
        public virtual User? User { get; set; }
        
        // Thích tour nào?
        public int TourId { get; set; }
        public virtual Tour? Tour { get; set; }
    }
}