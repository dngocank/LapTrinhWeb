using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BTL_PTTKYC.Data;
using BTL_PTTKYC.Models;

namespace BTL_PTTKYC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatbotController : ControllerBase
    {
        private readonly ILogger<ChatbotController> _logger;
        private readonly IConfiguration _configuration;
        private readonly ProductRepository _productRepo;
        private readonly PromotionRepository _promotionRepo;
        private static readonly HttpClient _httpClient = new HttpClient();

        public ChatbotController(
            ILogger<ChatbotController> logger,
            IConfiguration configuration,
            ProductRepository productRepo,
            PromotionRepository promotionRepo)
        {
            _logger = logger;
            _configuration = configuration;
            _productRepo = productRepo;
            _promotionRepo = promotionRepo;
        }

        public class ChatbotRequest
        {
            [JsonProperty("message")]
            public string Message { get; set; } = string.Empty;

            [JsonProperty("history")]
            public List<ChatbotMessageItem>? History { get; set; }
        }

        public class ChatbotMessageItem
        {
            [JsonProperty("role")]
            public string Role { get; set; } = "user";

            [JsonProperty("content")]
            public string Content { get; set; } = string.Empty;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatbotRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Message))
            {
                return BadRequest(new { success = false, error = "Nội dung tin nhắn không được để trống." });
            }

            string userMessage = request.Message.Trim();
            var history = request.History ?? new List<ChatbotMessageItem>();

            // 1. Thử chuyển tiếp (proxy) tới NodeJS KittyBot Microservice (Port 3001) nếu đang chạy
            string microserviceUrl = _configuration["Groq:MicroserviceUrl"] ?? "http://localhost:3001";
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3.5));
                var proxyPayload = JsonConvert.SerializeObject(new
                {
                    message = userMessage,
                    history = history
                });
                var content = new StringContent(proxyPayload, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{microserviceUrl.TrimEnd('/')}/api/chat", content, cts.Token);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var jsonObj = JObject.Parse(responseJson);
                    return Ok(jsonObj);
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("NodeJS Microservice ({Url}) chưa khởi động hoặc timeout ({Msg}). Chuyển sang bộ máy C# nội tại.", microserviceUrl, ex.Message);
            }

            // 2. TỰ ĐỘNG DỰ PHÒNG CẤP CAO TRONG ASP.NET CORE (C# Direct AI + Rule-Based Database Engine)
            // Lấy danh sách sản phẩm & khuyến mãi từ Database PhoneStoreDB
            List<ProductViewModel> allProducts = new List<ProductViewModel>();
            try
            {
                allProducts = _productRepo.GetAllProducts();
            }
            catch (Exception dbEx)
            {
                _logger.LogWarning("Không thể lấy danh sách sản phẩm từ DB: {Msg}", dbEx.Message);
            }

            var promotions = new List<PromotionModel>();
            try
            {
                promotions = _promotionRepo.GetActivePromotions();
            }
            catch { }

            string apiKey = (Environment.GetEnvironmentVariable("GROQ_API_KEY") ?? _configuration["Groq:ApiKey"] ?? string.Empty).Trim();
            string model = _configuration["Groq:Model"] ?? "qwen/qwen3.8-27b";

            // Nếu có Groq API Key -> Gọi thẳng Groq Cloud API từ C#
            if (!string.IsNullOrEmpty(apiKey) && !apiKey.StartsWith("gsk_your"))
            {
                try
                {
                    var aiReply = await CallGroqDirectAsync(apiKey, model, userMessage, history, allProducts, promotions);
                    if (!string.IsNullOrEmpty(aiReply))
                    {
                        return Ok(new
                        {
                            success = true,
                            reply = aiReply,
                            engine = $"Groq Online AI ({model}) - ASP.NET Direct",
                            tier = 1,
                            timestamp = DateTime.UtcNow.ToString("o")
                        });
                    }
                }
                catch (Exception groqEx)
                {
                    _logger.LogWarning("Lỗi gọi trực tiếp Groq API từ C#: {Msg}", groqEx.Message);
                }
            }

            // 3. Fallback Tầng 2: C# Rule-Based Engine (Offline 0đ, truy vấn trực tiếp CSDL PhoneStoreDB)
            string offlineReply = GenerateRuleBasedReply(userMessage, allProducts, promotions);
            return Ok(new
            {
                success = true,
                reply = offlineReply,
                engine = "KittyBot Rule-Based Engine (ASP.NET Core Offline 0đ)",
                tier = 2,
                timestamp = DateTime.UtcNow.ToString("o")
            });
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            bool dbOk = false;
            try
            {
                var brands = _productRepo.GetBrands();
                dbOk = brands != null;
            }
            catch { }

            string apiKey = (Environment.GetEnvironmentVariable("GROQ_API_KEY") ?? _configuration["Groq:ApiKey"] ?? string.Empty).Trim();

            return Ok(new
            {
                status = "OK",
                service = "KittyBot ASP.NET Core Integrated Assistant",
                database = dbOk ? "Connected" : "Disconnected",
                groqConfigured = !string.IsNullOrEmpty(apiKey) && !apiKey.StartsWith("gsk_your"),
                model = _configuration["Groq:Model"] ?? "qwen/qwen3.8-27b",
                microserviceUrl = _configuration["Groq:MicroserviceUrl"] ?? "http://localhost:3001",
                timestamp = DateTime.UtcNow.ToString("o")
            });
        }

        private async Task<string?> CallGroqDirectAsync(
            string apiKey,
            string model,
            string userMessage,
            List<ChatbotMessageItem> history,
            List<ProductViewModel> products,
            List<PromotionModel> promotions)
        {
            // Xây dựng ngữ cảnh sản phẩm tóm tắt từ CSDL thực tế để AI đọc hiểu
            var sbContext = new StringBuilder();
            sbContext.AppendLine("Bạn là KittyBot ✦ - Chuyên viên tư vấn công nghệ và thiết bị di động chính thức tại hệ thống KittyPhone (chuỗi bán lẻ điện thoại cao cấp).");
            sbContext.AppendLine("NHIỆM VỤ: Tư vấn thân thiện, nhiệt tình, am hiểu sâu về điện thoại. Khách quan theo nhu cầu và ngân sách của khách hàng.");
            sbContext.AppendLine("Dưới đây là thông tin THỰC TẾ trong kho hàng của KittyPhone (hãy ưu tiên dựa trên dữ liệu này để tư vấn chính xác giá và tồn kho):");
            
            // Lọc ra các sản phẩm tiêu biểu để đưa vào context (tối đa 25 sản phẩm)
            var sampleProducts = products.Where(p => p.IsActive).Take(25).ToList();
            foreach (var p in sampleProducts)
            {
                sbContext.AppendLine($"- {p.ProductName} | Hãng: {p.BrandName} | Giá: {p.SellingPrice:N0}đ | Kho: {p.Quantity} máy (Trạng thái: {p.StockStatus})");
            }

            if (promotions.Count > 0)
            {
                sbContext.AppendLine("Khuyến mãi hiện hành tại cửa hàng:");
                foreach (var promo in promotions.Take(5))
                {
                    sbContext.AppendLine($"- {promo.PromotionName}: Giảm {promo.DiscountPercent}% | {promo.Description}");
                }
            }

            sbContext.AppendLine("QUY TẮC:");
            sbContext.AppendLine("1. Luôn trả lời bằng tiếng Việt lịch thiệp, dễ thương, chuyên nghiệp, sử dụng icon công nghệ (📱, ⚡, 🎮, 📸, 🔋, 💰, 🛒).");
            sbContext.AppendLine("2. In đậm **tên máy**, **giá bán**, cấu hình chính.");
            sbContext.AppendLine("3. Nhắc đến chính sách: Bảo hành 12 tháng chính hãng, 1 đổi 1 trong 30 ngày, trả góp 0% tại KittyPhone khi phù hợp.");

            var messages = new List<object>
            {
                new { role = "system", content = sbContext.ToString() }
            };

            foreach (var h in history.TakeLast(6))
            {
                messages.Add(new { role = h.Role, content = h.Content });
            }
            messages.Add(new { role = "user", content = userMessage });

            var payload = new
            {
                model = model,
                messages = messages,
                temperature = 0.6,
                max_tokens = 1024
            };

            using var requestMsg = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");
            requestMsg.Headers.Add("Authorization", $"Bearer {apiKey}");
            requestMsg.Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            var response = await _httpClient.SendAsync(requestMsg, cts.Token);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Groq API returned error: {StatusCode} - {Body}", response.StatusCode, responseString);
                return null;
            }

            var resultObj = JObject.Parse(responseString);
            var reply = resultObj["choices"]?[0]?["message"]?["content"]?.ToString();
            return reply?.Trim();
        }

        private string GenerateRuleBasedReply(string message, List<ProductViewModel> products, List<PromotionModel> promotions)
        {
            string lower = message.ToLowerInvariant();

            // 1. Chào hỏi
            if (Regex.IsMatch(lower, @"^(xin chào|chào|hello|hi|alo|chào shop|hey|bắt đầu)"))
            {
                return "Chào bạn! Mình là **KittyBot ✦** - Chuyên viên tư vấn công nghệ tại **KittyPhone**.\n\n" +
                       "Mình có thể hỗ trợ bạn ngay:\n" +
                       "- 📱 **Gợi ý chọn điện thoại** theo ngân sách và nhu cầu (Chơi game, chụp ảnh đẹp, pin trâu...)\n" +
                       "- ⚡ **Tra cứu cấu hình & tồn kho thực tế** tại cửa hàng\n" +
                       "- 🎁 **Cập nhật khuyến mãi & voucher giảm giá** mới nhất.\n\n" +
                       "Bạn đang quan tâm dòng máy nào hoặc có tầm giá dự kiến bao nhiêu để mình tư vấn ngay nhé! 🛒✨";
            }

            // 2. Khuyến mãi / Voucher
            if (Regex.IsMatch(lower, @"(khuyến mãi|khuyen mai|voucher|giảm giá|giam gia|ưu đãi|uu dai|quà|qua tang)"))
            {
                if (promotions.Count == 0)
                {
                    return "🎁 Hiện tại KittyPhone đang áp dụng chính sách **Bảo hành chính hãng 12 tháng**, **1 đổi 1 trong 30 ngày**, **hỗ trợ trả góp 0%** và **miễn phí giao hàng toàn quốc** cho mọi đơn hàng điện thoại!";
                }

                var sb = new StringBuilder("🎁 **CÁC CHƯƠNG TRÌNH KHUYẾN MÃI ĐANG DIỄN RA TẠI KITTYPHONE:**\n\n");
                foreach (var promo in promotions.Take(4))
                {
                    sb.AppendLine($"- **{promo.PromotionName}** (Giảm tới **{promo.DiscountPercent}%**)");
                    if (!string.IsNullOrEmpty(promo.Description))
                    {
                        sb.AppendLine($"  *{promo.Description}*");
                    }
                }
                sb.AppendLine("\nBạn muốn mua dòng máy nào để mình áp dụng mức giảm giá tốt nhất cho bạn nhé!");
                return sb.ToString();
            }

            // 3. Phân tích hãng
            string? matchedBrand = null;
            if (lower.Contains("iphone") || lower.Contains("apple") || lower.Contains("ip")) matchedBrand = "Apple";
            else if (lower.Contains("samsung") || lower.Contains("galaxy")) matchedBrand = "Samsung";
            else if (lower.Contains("xiaomi") || lower.Contains("redmi") || lower.Contains("poco")) matchedBrand = "Xiaomi";
            else if (lower.Contains("oppo") || lower.Contains("reno")) matchedBrand = "OPPO";
            else if (lower.Contains("vivo")) matchedBrand = "Vivo";

            // 4. Phân tích ngân sách
            decimal? minPrice = null;
            decimal? maxPrice = null;

            var underMatch = Regex.Match(lower, @"(dưới|<|thấp hơn|tối đa|ko quá|không quá)\s*(\d+(?:[.,]\d+)?)\s*(tr|triệu|củ|m|k)?");
            if (underMatch.Success)
            {
                if (decimal.TryParse(underMatch.Groups[2].Value.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal val))
                {
                    maxPrice = val * 1000000m;
                }
            }

            var rangeMatch = Regex.Match(lower, @"(\d+(?:[.,]\d+)?)\s*(?:-|đến|tới)\s*(\d+(?:[.,]\d+)?)\s*(?:tr|triệu|củ|m)");
            if (rangeMatch.Success)
            {
                if (decimal.TryParse(rangeMatch.Groups[1].Value.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal minVal) &&
                    decimal.TryParse(rangeMatch.Groups[2].Value.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal maxVal))
                {
                    minPrice = minVal * 1000000m;
                    maxPrice = maxVal * 1000000m;
                }
            }

            // Lọc danh sách máy
            var filtered = products.Where(p => p.IsActive).ToList();
            if (!string.IsNullOrEmpty(matchedBrand))
            {
                filtered = filtered.Where(p => (p.BrandName ?? "").Contains(matchedBrand, StringComparison.OrdinalIgnoreCase) || p.ProductName.Contains(matchedBrand, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            if (minPrice.HasValue)
            {
                filtered = filtered.Where(p => p.SellingPrice >= minPrice.Value).ToList();
            }
            if (maxPrice.HasValue)
            {
                filtered = filtered.Where(p => p.SellingPrice <= maxPrice.Value).ToList();
            }

            // Nếu người dùng hỏi máy cụ thể (VD: iPhone 15, Galaxy S25...)
            var exactMatched = products.Where(p => p.IsActive && lower.Contains(p.ProductName.ToLowerInvariant())).Take(3).ToList();
            if (exactMatched.Count > 0)
            {
                var sb = new StringBuilder("📱 **THÔNG TIN DÒNG MÁY BẠN ĐANG QUAN TÂM TẠI KITTYPHONE:**\n\n");
                foreach (var p in exactMatched)
                {
                    sb.AppendLine($"- **{p.ProductName}**");
                    sb.AppendLine($"  💰 Giá niêm yết: **{p.SellingPrice:N0}đ**");
                    sb.AppendLine($"  📦 Tình trạng kho: **{(p.Quantity > 0 ? $"Còn hàng ({p.Quantity} máy)" : "Tạm hết hàng")}**");
                    if (!string.IsNullOrEmpty(p.Description))
                    {
                        var shortDesc = p.Description.Length > 150 ? p.Description.Substring(0, 150) + "..." : p.Description;
                        sb.AppendLine($"  ⚡ {shortDesc}");
                    }
                    sb.AppendLine();
                }
                sb.AppendLine("🛡️ *Cam kết máy mới 100% chính hãng, bảo hành 12 tháng, 1 đổi 1 trong 30 ngày. Hỗ trợ trả góp 0% lãi suất.*");
                return sb.ToString();
            }

            // Trả về danh sách gợi ý theo tiêu chí
            if (filtered.Count > 0)
            {
                var topList = filtered.Take(5).ToList();
                var sb = new StringBuilder("📱 **KITTYBOT GỢI Ý CÁC MẪU ĐIỆN THOẠI PHÙ HỢP VỚI BẠN:**\n\n");
                foreach (var p in topList)
                {
                    sb.AppendLine($"- **{p.ProductName}** – **{p.SellingPrice:N0}đ** (Tồn kho: {p.Quantity} máy)");
                    if (!string.IsNullOrEmpty(p.Description))
                    {
                        var lines = p.Description.Split('\n').Select(l => l.Trim()).Where(l => !string.IsNullOrEmpty(l)).Take(2);
                        foreach (var l in lines)
                        {
                            sb.AppendLine($"  → {l}");
                        }
                    }
                }
                sb.AppendLine("\n🎯 Bạn đang ưng ý mẫu nào nhất trong số này, hoặc cần mình so sánh cụ thể không ạ?");
                return sb.ToString();
            }

            // Trường hợp chung chung
            return "Dạ KittyBot đã ghi nhận câu hỏi của bạn! Để KittyBot tư vấn chính xác nhất, bạn có thể cho mình biết thêm:\n" +
                   "1. **Ngân sách dự kiến** (VD: dưới 10 triệu, 15-20 triệu...)?\n" +
                   "2. **Thương hiệu yêu thích** (Apple iPhone, Samsung, Xiaomi, OPPO...)?\n" +
                   "3. **Nhu cầu chính** (Cày game nặng, chụp ảnh đẹp sống ảo, pin trâu dùng cả ngày, làm việc...)?\n\n" +
                   "KittyBot sẽ tra cứu kho hàng và đề xuất chiếc máy ưng ý nhất cho bạn ngay nhé! 🛒✨";
        }
    }
}
