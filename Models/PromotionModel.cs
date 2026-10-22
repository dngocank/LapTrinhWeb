using System;

namespace BTL_PTTKYC.Models
{
    public class PromotionModel
    {
        public int PromotionId { get; set; }
        public string PromotionName { get; set; } = string.Empty;
        public decimal DiscountPercent { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Description { get; set; }

        // Ràng buộc khung giờ (ví dụ Flash sale: 0h - 2h)
        public int? StartHour { get; set; }
        public int? EndHour { get; set; }

        // Ràng buộc hạng thành viên (ví dụ MinRankId = 2: Bạc/Silver trở lên)
        public int? MinRankId { get; set; }
        public string? MinRankName => MinRankId switch
        {
            2 => "Hạng Bạc (Silver) trở lên",
            3 => "Hạng Vàng (Gold) trở lên",
            4 => "Hạng Kim Cương (Platinum)",
            _ => null
        };

        // Kiểm tra xem hôm nay có nằm trong khoảng ngày diễn ra không
        public bool IsActive => DateTime.Today >= StartDate.Date && DateTime.Today <= EndDate.Date;

        // Kiểm tra xem hiện tại có đang trong khung giờ vàng không
        public bool IsInActiveHour
        {
            get
            {
                if (!StartHour.HasValue || !EndHour.HasValue) return true;
                int currentHour = DateTime.Now.Hour;
                return currentHour >= StartHour.Value && currentHour < EndHour.Value;
            }
        }

        // Voucher có khả dụng ngay lúc này hay không
        public bool IsCurrentlyAvailable => IsActive && IsInActiveHour;

        // Mô tả điều kiện ràng buộc ngắn gọn
        public string RequirementNotice
        {
            get
            {
                var notices = new List<string>();
                if (StartHour.HasValue && EndHour.HasValue)
                {
                    notices.Add($"Khung giờ vàng: {StartHour:00}:00 - {EndHour:00}:00");
                }
                if (MinRankId.HasValue && MinRankId.Value > 1)
                {
                    notices.Add($"Dành riêng {MinRankName}");
                }
                return string.Join(" | ", notices);
            }
        }

        // Kiểm tra quyền sử dụng của một khách hàng cụ thể
        public bool CanCustomerApply(int? customerRankId, out string failureReason)
        {
            failureReason = string.Empty;

            if (DateTime.Today < StartDate.Date)
            {
                failureReason = $"Chương trình chưa bắt đầu (Mở ngày {StartDate:dd/MM/yyyy}).";
                return false;
            }

            if (DateTime.Today > EndDate.Date)
            {
                failureReason = "Chương trình khuyến mãi đã kết thúc.";
                return false;
            }

            if (StartHour.HasValue && EndHour.HasValue && !IsInActiveHour)
            {
                failureReason = $"Chương trình chỉ áp dụng trong khung giờ vàng {StartHour:00}:00 - {EndHour:00}:00 (Hiện tại: {DateTime.Now:HH:mm}).";
                return false;
            }

            if (MinRankId.HasValue && MinRankId.Value > 1)
            {
                int rank = customerRankId.GetValueOrDefault(1);
                if (rank < MinRankId.Value)
                {
                    failureReason = $"Chương trình chỉ dành cho hội viên {MinRankName}. Hạng hiện tại của bạn chưa đủ điều kiện áp dụng.";
                    return false;
                }
            }

            return true;
        }

        public string PromoCode => $"KITTY{Math.Round(DiscountPercent)}";
    }
}
