# NHẬT KÝ THAY ĐỔI DỰ ÁN (PROJECT CHANGELOG)

Tài liệu ghi nhận toàn bộ các mốc phát triển, tính năng mới, tái cấu trúc và sửa lỗi của dự án **BTL_PTTKYC (PhoneStore Management System)**.

---

## [2026-10-18] - Hệ Thống Tài Khoản Khách Hàng, Xác Thực SĐT/Gmail & Ràng Buộc Đánh Giá Chống Spam

### Đã Thêm / Cải Tiến (Added & Improved)
- **Tài khoản Khách Hàng & Xác Thực Hai Yếu Tố (Customer Account & OTP Verification)**:
  - Bổ sung cấu trúc lưu trữ và xác thực tài khoản vào bảng `Customer` trong `DbInitializer.cs`: `email`, `password_hash`, `is_email_verified`, `is_phone_verified`, `otp_code`, `otp_expiry`.
  - Triển khai chức năng Đăng ký tài khoản (`/Customer/Register`) với đầy đủ ràng buộc regex kiểm tra Họ tên, SĐT Việt Nam (10 số, đầu 03/05/07/08/09), Gmail/Email hợp lệ, và mật khẩu bảo mật băm bằng `BCrypt`.
  - Cơ chế xác thực mã OTP 6 số (`/Customer/VerifyOtp`) bảo vệ cả SĐT và Gmail, có bộ trợ giúp OTP giả lập để kiểm thử bài tập lớn dễ dàng.
  - Trang Đăng nhập khách hàng (`/Customer/Login`) và Trang Cá Nhân (`/Customer/Profile`) với Thẻ thành viên số (Digital Membership Card) hiển thị Hạng thẻ, tích lũy chi tiêu và danh sách các mặt hàng đã mua.
  - Tự động điền thông tin khách hàng tại trang Thanh toán (`/Cart/Checkout`) khi đã đăng nhập.
- **Ràng Buộc Đánh Giá Chống Spam Tuyệt Đối (Purchased-Only Review Gatekeeper)**:
  - Bổ sung `customer_id`, `invoice_id`, `is_verified_purchase` vào bảng `Review` và `ReviewModel`.
  - Triển khai cơ chế kiểm tra bảo mật ở cả tầng UI và Backend (`HomeController.AddReview` & `ReviewRepository.AddVerifiedReview`): CHỈ những khách hàng đã có đơn hàng hợp lệ chứa sản phẩm đó mới được viết đánh giá.
  - Khách chưa đăng nhập: Hiển thị banner giải thích chính sách và nút Đăng nhập/Đăng ký.
  - Khách đã đăng nhập nhưng chưa mua: Hiển thị thông báo lịch sự "Chỉ dành cho khách hàng đã mua mặt hàng này" kèm nút Mua ngay.
  - Khách đã mua sản phẩm: Hiển thị huy hiệu `Verified Buyer` và mở form đánh giá với thông tin đã xác thực, khóa không cho mạo danh.
  - Hiển thị huy hiệu `[Đã mua tại KittyPhone]` trên từng card đánh giá ở trang chi tiết sản phẩm và trong bảng quản trị Reviews của Sales / Admin.
- **Khắc Phục Toàn Diện Lỗi Màu Sắc Giao Diện Đánh Giá**:
  - Loại bỏ hoàn toàn lỗi chữ trắng `text-white` trên nền sáng làm chìm nội dung tiêu đề và modal.
  - Thiết kế lại toàn bộ CSS `.review-card`, `.review-reply-box`, `.review-rating-summary`, `.review-gate-card` đồng bộ 100% với Design System của website (Key Visual xanh Navy `#0b1e47`, Blue `#2563eb`, Sao vàng cam `#f59e0b`, Badge xanh lá `#047857`, nền trắng sạch và chữ tương phản cao `#0f172a` / `#1e293b`).

---

## [2026-10-18] - Tái Cấu Trúc Toàn Diện Thư Mục Dự Án & Hợp Nhất Cơ Sở Dữ Liệu

### Đã Thêm / Cải Tiến (Added & Improved)
- **Hợp nhất Cơ sở dữ liệu (`Database/PhoneStoreDB_Full.sql`)**:
  - Gộp thành công 5 file SQL phân tán thành 1 file master duy nhất bao gồm 20 bảng, 8 views, 6 functions, 7 triggers, 13 stored procedures.
  - Tích hợp sẵn toàn bộ cột mở rộng: `Product.image_url`, `Product.description`, `Invoice.shipping_address`, `Invoice.order_notes`, `Invoice.payment_method_selected`, các cột hủy đơn (`cancel_reason`, `cancel_requested_by`, `cancel_approved_by`).
  - Hợp nhất toàn bộ dữ liệu mẫu iPhone phong cách 24hStore và hệ sinh thái thiết bị Samsung, Xiaomi, iPad, Apple Watch, Phụ kiện Baseus/Havit.
  - Cung cấp tài liệu [Database/README.md](file:///d:/Downloads/BTL_PTTKYC/Database/README.md) hướng dẫn cài đặt trên SSMS và Azure Data Studio.
- **Quy hoạch Tài liệu & Tiêu chuẩn (`docs/`)**:
  - Khởi tạo thư mục [docs/](file:///d:/Downloads/BTL_PTTKYC/docs) theo đúng quy tắc dự án.
  - Thiết lập [docs/changelog.md](file:///d:/Downloads/BTL_PTTKYC/docs/changelog.md), [docs/roadmap.md](file:///d:/Downloads/BTL_PTTKYC/docs/roadmap.md), [docs/troubleshooting.md](file:///d:/Downloads/BTL_PTTKYC/docs/troubleshooting.md) và [docs/design_system.md](file:///d:/Downloads/BTL_PTTKYC/docs/design_system.md).
- **Chuẩn hóa Antigravity Skill (`.agents/skills/`)**:
  - Di chuyển `skill.md` vào đúng vị trí nhận diện tự động của Antigravity AI Agent: `.agents/skills/design-system-24hstore-vn/SKILL.md`.
- **Quy hoạch Công cụ khởi chạy (`scripts/`)**:
  - Chuyển `run_with_cloudflare.bat` vào thư mục `scripts/run_with_cloudflare.bat` với cơ chế tự động định vị thư mục gốc dự án.

### Đã Loại Bỏ / Dọn Dẹp (Removed & Cleaned)
- Xóa các file script cơ sở dữ liệu tạm thời/rải rác ở thư mục gốc: `PhoneStoreDB_full.sql`, `update_schema.sql`, `scratch_update.sql`, `seed_24hstore_iphone.sql`, `seed_full_store_catalog.sql`.
- Xóa file trùng lặp `PROJECT_RULES.md` ở thư mục gốc (đã có bộ quy tắc chi tiết trong `rules/` và `.agents/rules/project_rules.md`).
- Xóa `design.md` và `skill.md` dôi dư ở thư mục gốc.

---

## [2026-10-15] - Phát Triển Giao Diện Storefront & Tính Năng Đặt Hàng Trực Tuyến

### Đã Thêm (Added)
- Xây dựng giao diện Storefront 24hStore trên ASP.NET Core MVC Razor Views.
- Bổ sung bộ lọc thương hiệu, danh mục, khoảng giá và tìm kiếm thời gian thực.
- Hoàn thiện luồng thanh toán giỏ hàng One-page checkout lưu vào `Invoice` và `Invoice_Detail`.
- Tích hợp hệ thống quản lý kho Serial/IMEI và kích hoạt bảo hành tự động.
