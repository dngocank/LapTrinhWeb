namespace BTL_PTTKYC.Models
{
    public class ProductViewModel
    {
        public int ProductId { get; set; }
        public int BrandId { get; set; }
        public int CategoryId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal ImportPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public int Quantity { get; set; }
        public string StockStatus { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string? ImageUrl { get; set; }
        public string? Description { get; set; }

        // Extra display properties
        public decimal OriginalPrice => SellingPrice > 0 ? Math.Round(SellingPrice * 1.15m / 10000) * 10000 : 0;
        public decimal DiscountAmount => OriginalPrice > SellingPrice ? (OriginalPrice - SellingPrice) : 0;
        public int DiscountPercent => OriginalPrice > SellingPrice ? (int)Math.Round((OriginalPrice - SellingPrice) / OriginalPrice * 100) : 0;
        public decimal TradeInPrice => SellingPrice > 2500000m ? (SellingPrice - 2000000m) : SellingPrice;
        public double AverageRating { get; set; } = 5.0;
        public int ReviewCount { get; set; } = 0;

        // Trích xuất mô tả tổng quan (bỏ phần bảng thông số)
        public string OverviewDescription
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Description)) return "Sản phẩm công nghệ chính hãng VN/A được phân phối tại hệ thống chuỗi bán lẻ KittyPhone.";
                var marker = "--- THÔNG SỐ KỸ THUẬT ---";
                var idx = Description.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
                string text = idx >= 0 ? Description.Substring(0, idx).Trim() : Description.Trim();
                return text.Replace("24hStore", "KittyPhone", StringComparison.OrdinalIgnoreCase).Replace("CellphoneS", "KittyPhone", StringComparison.OrdinalIgnoreCase);
            }
        }

        // Trích xuất bảng thông số kỹ thuật chi tiết
        public List<(string Name, string Value)> Specifications
        {
            get
            {
                var list = new List<(string Name, string Value)>();
                if (string.IsNullOrWhiteSpace(Description)) return list;

                var marker = "--- THÔNG SỐ KỸ THUẬT ---";
                var idx = Description.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
                string textToParse = idx >= 0 ? Description.Substring(idx + marker.Length) : Description;

                var lines = textToParse.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var rawLine in lines)
                {
                    var line = rawLine.Trim();
                    if (string.IsNullOrEmpty(line)) continue;
                    var clean = line.TrimStart('-', '•', '*', ' ', '\t');
                    var colonIdx = clean.IndexOf(':');
                    if (colonIdx > 0 && colonIdx < clean.Length - 1)
                    {
                        var key = clean.Substring(0, colonIdx).Trim();
                        var val = clean.Substring(colonIdx + 1).Trim();
                        if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(val))
                        {
                            list.Add((key, val));
                        }
                    }
                }
                return list;
            }
        }
    }
}
