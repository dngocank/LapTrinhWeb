using System.ComponentModel.DataAnnotations;

namespace BTL_PTTKYC.Models
{
    public class ReviewModel
    {
        public int ReviewId { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [RegularExpression(@"^0[0-9]{9}$", ErrorMessage = "Số điện thoại không hợp lệ (10 chữ số)")]
        public string? Phone { get; set; }

        [Range(1, 5, ErrorMessage = "Vui lòng chọn đánh giá từ 1 đến 5 sao")]
        public int Rating { get; set; } = 5;

        [Required(ErrorMessage = "Vui lòng viết cảm nhận / đánh giá của bạn")]
        [StringLength(1000)]
        public string Comment { get; set; } = string.Empty;

        public string? Reply { get; set; }

        public int? CustomerId { get; set; }

        public int? InvoiceId { get; set; }

        public bool IsVerifiedPurchase { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
