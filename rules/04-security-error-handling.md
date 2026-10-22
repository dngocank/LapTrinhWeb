# 4. An Toàn & Xử Lý Lỗi (Security & Error Handling)

## 4.1. Bảo Mật Cấu Hình & Tài Khoản
- **Chuỗi kết nối (Connection String)**: Đặt tập trung trong [appsettings.json](file:///d:/Downloads/BTL_PTTKYC/appsettings.json) (`ConnectionStrings:DefaultConnection`). Tuyệt đối không hard-code thông tin nhạy cảm trong mã nguồn C#.
- **Băm mật khẩu người dùng**:
  - Toàn bộ mật khẩu tài khoản (`Account.password_hash`) bắt buộc băm bằng `BCrypt.Net.BCrypt.HashPassword` trước khi lưu vào CSDL.
  - Khi đăng nhập, xác thực bằng hàm `BCrypt.Net.BCrypt.Verify(inputPassword, storedHash)`.
  - Không bao giờ lưu hay so sánh mật khẩu dưới dạng văn bản thô (plain text).
- **Phân quyền người dùng (Roles)**:
  - Hệ thống hỗ trợ 4 vai trò rõ rệt: `Admin`, `Sales`, `Warehouse`, `Customer`.
  - Mọi Controller / Action thuộc khu vực quản trị phải kiểm tra Role trước khi xử lý nghiệp vụ.

---

## 4.2. Xác Thực Dữ Liệu (Validation)
- **Client-side**:
  - Tận dụng Data Annotations trong Models/ViewModels (`[Required]`, `[Phone]`, `[Range]`, `[StringLength]`).
  - Hỗ trợ kiểm tra tức thời phía giao diện qua thư viện jQuery Validation Unobtrusive.
- **Server-side**:
  - Luôn luôn kiểm tra `ModelState.IsValid` tại Controller trước khi chuyển dữ liệu xuống Repository.
- **Quy chuẩn dữ liệu nghiệp vụ bán lẻ**:
  - Số điện thoại: Định dạng chuẩn 10 chữ số tại Việt Nam.
  - Giá bán & Giá nhập: Luôn lớn hơn 0 (`SellingPrice > 0`, `ImportPrice > 0`).
  - Số lượng tồn kho: Không được âm (`Quantity >= 0`).
  - Ngày tháng: Ngày bảo hành, ngày giao hàng phải hợp lệ so với ngày tạo đơn.

---

## 4.3. Xử Lý Ngoại Lệ (Exception Handling)
- **Không để khối `catch` rỗng**: Mọi khối `try-catch` phải xử lý lỗi triệt để, ghi log hoặc trả về thông điệp dễ hiểu cho người dùng.
- **Bắt và xử lý `SqlException` từ Stored Procedures & Triggers**:
  - Trích xuất `ex.Message` để lấy thông báo nghiệp vụ do Trigger/Procedure bắn ra (ví dụ: *"Sản phẩm đã hết hàng trong kho"*, *"Đơn hàng đã thanh toán không thể hủy"*, ...).
  - Trình bày thông báo thân thiện ra giao diện thông qua `TempData["Error"]` hoặc `ModelState.AddModelError(string.Empty, ex.Message)`.
  - Tránh để lộ lỗi kỹ thuật dạng raw stack trace ra giao diện người dùng cuối.
