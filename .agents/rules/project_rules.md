# CÁC QUY TẮC CƠ BẢN DÀNH CHO DỰ ÁN BTL_PTTKYC (PHONE STORE MANAGEMENT)

> **Dự án**: Hệ thống Quản lý Cửa hàng Điện thoại Di động & Thiết bị Công nghệ (BTL_PTTKYC)  
> **Công nghệ**: ASP.NET Core MVC (.NET 10), C#, ADO.NET, Microsoft SQL Server (`PhoneStoreDB`)  
> **Thư mục quy tắc chi tiết**: [rules/](file:///d:/Downloads/BTL_PTTKYC/rules/)

---

## 1. Giới Hạn Can Thiệp & Quản Lý Thư Viện
- Chỉ sửa đổi đúng phạm vi nhiệm vụ (Storefront bán hàng, giỏ hàng, quản lý kho IMEI/Serial, quản lý đơn hàng Sales, quản trị Admin). Tuyệt đối không tự ý refactor cấu trúc hay các module đang chạy ổn định.
- Thư viện đã chuẩn hóa: `Microsoft.Data.SqlClient` (v7.1.1), `BCrypt.Net-Next` (v4.2.0), `Newtonsoft.Json` (v13.0.4).
- KHÔNG tự tiện cài đặt thêm ORM khác (EF Core, Dapper) hay thư viện frontend lạ nếu chưa có yêu cầu cụ thể từ người dùng. Khi cần thêm phải giải thích và xin xác nhận.

## 2. Kiến Trúc & Phong Cách Viết Mã (Coding Standards)
- **Kiến trúc Phân tầng rõ ràng (Repository Pattern)**:
  - `Models/`: Data Models ánh xạ thực thể, ViewModels cho giao diện và DTOs.
  - `Data/`: Tách riêng toàn bộ tầng truy xuất dữ liệu vào `Data/` (`ProductRepository`, `OrderRepository`, `WarehouseRepository`, `AccountRepository`, `ReviewRepository`).
  - `Controllers/`: Chỉ xử lý điều hướng, xác thực phân quyền, gọi Repository và trả về View/JSON. Tuyệt đối KHÔNG viết câu lệnh SQL trực tiếp trong Controller.
  - `Views/`: Razor Views chỉ phụ trách giao diện, render ViewModel, hiển thị badge. Không truy vấn CSDL hay xử lý nghiệp vụ phức tạp tại View.
- **Quy chuẩn đặt tên**: C# Classes/Methods/Properties/Public: `PascalCase`. Biến cục bộ/parameters: `camelCase`. Private fields: `_camelCase`. Razor Views: Trùng tên Action Method.
- **Chú thích (Comments)**: Tiếng Việt ngắn gọn tại các hàm nghiệp vụ trọng tâm (trừ kho, xuất IMEI, xử lý SqlException từ Trigger/Procedure).

## 3. Nguyên Tắc Tương Tác Cơ Sở Dữ Liệu (Database Integration)
- Tận dụng tối đa CSDL `PhoneStoreDB` trong [PhoneStoreDB_Full.sql](file:///d:/Downloads/BTL_PTTKYC/Database/PhoneStoreDB_Full.sql):
  - **20 Tables**: `Brand`, `Category`, `MembershipRank`, `Product`, `Supplier`, `Customer`, `Employee`, `Account`, `Warehouse`, `Product_Serial` (IMEI/Serial), `Import_Receipt`, `Import_Detail`, `Promotion`, `Invoice`, `Invoice_Detail`, `Payment`, `Warranty`, `Stock_Movement`, `Review`.
  - **8 Views**: `vw_ProductCatalog`, `vw_InvoiceFull`, `vw_MonthlyRevenue`, `vw_BestSellingProducts`, `vw_LowStockProducts`, `vw_CustomerSummary`, `vw_WarrantyStatus`, `vw_AccountSecurity`.
  - **13 Stored Procedures**: Sản phẩm (`usp_Product_Search`, `usp_Product_Insert`, `usp_Product_Update`, `usp_Product_Delete`), Khách hàng (`usp_Customer_Insert`, `usp_Customer_Update`), Đơn hàng (`usp_CreateInvoice`, `usp_CancelInvoice`, `usp_AddPayment`, `usp_GetInvoiceDetail`), Kho (`usp_CreateImportReceipt`), Báo cáo & Bảo hành (`usp_GetRevenueReport`, `usp_RegisterWarranty`).
  - **6 Functions**: `fn_GetInvoiceSubTotal`, `fn_GetProductStock`, `fn_GetPromotionPercent`, `fn_GetCustomerRank`, `fn_GetRevenueByDateRange`, `fn_GetInvoicesByCustomer`.
  - **7 Triggers**: `trg_ImportDetail_AfterInsert`, `trg_InvoiceDetail_CheckBeforeInsert`, `trg_InvoiceDetail_AfterInsert`, `trg_Invoice_UpdateCustomerRank`, `trg_Invoice_AfterUpdateStatus`, `trg_Payment_AfterInsert`, `trg_Warranty_SetStatus`.
- **Chống SQL Injection**: Tuyệt đối KHÔNG cộng chuỗi SQL. Luôn dùng `SqlParameter`.

## 4. An Toàn & Xử Lý Lỗi (Security & Error Handling)
- Chuỗi kết nối đặt trong `appsettings.json`. Mật khẩu băm bằng `BCrypt.Net.BCrypt.HashPassword` và kiểm tra bằng `BCrypt.Verify`.
- Phân quyền theo vai trò (Roles): `Admin`, `Sales`, `Warehouse`, `Customer`.
- Validation: Client-side dùng Data Annotations và jQuery Validation Unobtrusive; Server-side kiểm tra `ModelState.IsValid`.
- Ngoại lệ: Không để khối catch rỗng. Bắt `SqlException` từ SP/Trigger, trích xuất `ex.Message` hiển thị thân thiện qua `TempData["Error"]` hoặc `ModelState.AddModelError`.

## 5. Quy Chuẩn Frontend & Razor Views (UI/UX 24hStore.vn Style)
- Giao diện Storefront & Admin theo phong cách chuỗi bán lẻ công nghệ 24hStore.vn: Font Roboto, thẻ sản phẩm trực quan, bộ lọc đa năng.
- Badges: Tồn kho (`In stock` xanh lá, `Low stock` vàng cam, `Out of stock` đỏ); Đơn hàng (`Pending` vàng cam, `Confirmed` xanh dương, `Paid`/`Delivered` xanh lá, `Cancelled` đỏ); Serial/IMEI (`InStock` xanh lá, `Sold` xám, `Warranty` tím/vàng).
- **Tách biệt Script & Style**: Mọi JavaScript tương tác BẮT BUỘC nằm trong [wwwroot/js/site.js](file:///d:/Downloads/BTL_PTTKYC/wwwroot/js/site.js). Toàn bộ CSS nằm trong [wwwroot/css/site.css](file:///d:/Downloads/BTL_PTTKYC/wwwroot/css/site.css). TUYỆT ĐỐI KHÔNG viết `<style>` hay `<script>` nội dòng trong Razor Views.

## 6. Cách Trình Bày Phản Hồi Của AI (Assistant Communication)
- Link file markdown: Luôn ghi rõ đường dẫn chuẩn link markdown `[filename](file:///path/to/file)` trước mỗi khối code.
- Giải thích súc tích nghiệp vụ trước khi cung cấp mã nguồn.
- Chỉ rõ các bước tiếp theo trong Roadmap sau mỗi nhiệm vụ.

## 7. Quy Tắc Tài Liệu & Workflow Liên Tục
- [docs/changelog.md](file:///d:/Downloads/BTL_PTTKYC/docs/changelog.md): Cập nhật mỗi khi thêm tính năng, sửa lỗi, chỉnh CSDL/Repository.
- [docs/roadmap.md](file:///d:/Downloads/BTL_PTTKYC/docs/roadmap.md): Cập nhật trạng thái đầu việc (`Pending`, `In Progress`, `Completed`).
- [docs/troubleshooting.md](file:///d:/Downloads/BTL_PTTKYC/docs/troubleshooting.md): Ghi nhận ngay lập tức lỗi SQL Server, Trigger, build .NET 10.
