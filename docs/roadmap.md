# LỘ TRÌNH PHÁT TRIỂN DỰ ÁN (PROJECT ROADMAP)

Theo dõi tiến độ tổng thể của Bài tập lớn Phân tích Thiết kế Hệ thống Yêu cầu (BTL_PTTKYC).

---

## Trạng Thái Tổng Thể

| Phân Hệ | Đầu Việc | Trạng Thái | Ghi Chú |
| :--- | :--- | :---: | :--- |
| **Cơ Sở Dữ Liệu** | Thiết kế 20 bảng chuẩn hóa BCNF | `[Completed]` | Đã có ràng buộc toàn vẹn, PK, FK, CHECK |
| **Cơ Sở Dữ Liệu** | Xây dựng 8 Views tổng hợp danh mục, báo cáo | `[Completed]` | `vw_ProductCatalog`, `vw_InvoiceFull`,... |
| **Cơ Sở Dữ Liệu** | Lập trình 6 Functions & 7 Triggers nghiệp vụ | `[Completed]` | Tự động hóa tính tồn kho, tổng tiền, thăng hạng |
| **Cơ Sở Dữ Liệu** | 13 Stored Procedures cho Backend ADO.NET | `[Completed]` | Đầy đủ Transaction, TRY/CATCH, chống SQLi |
| **Tầng Dữ Liệu (Data)** | Xây dựng Repository Pattern với `Microsoft.Data.SqlClient` | `[Completed]` | `ProductRepository`, `OrderRepository`,... |
| **Bảo Mật & Xác Thực** | Mã hóa mật khẩu bằng BCrypt (`BCrypt.Net-Next`) | `[Completed]` | Khởi tạo tài khoản Admin, Sales, Warehouse |
| **Bảo Mật & Phân Quyền** | Phân quyền truy cập theo vai trò (Roles) | `[Completed]` | Quản trị Admin, Nhân viên Bán hàng, Thủ kho |
| **Giao Diện Khách Hàng** | Storefront phong cách 24hStore.vn | `[Completed]` | Font Roboto, Banner, Card sản phẩm, Bộ lọc |
| **Bán Hàng Trực Tuyến** | Giỏ hàng Session & Đặt hàng One-page checkout | `[Completed]` | Lưu thông tin khách hàng, địa chỉ, ghi chú |
| **Quản Lý Bán Lẻ (Sales)** | Tạo hóa đơn tại quầy & Ghi nhận thanh toán đa kênh | `[Completed]` | Tiền mặt, Chuyển khoản, Momo |
| **Quản Lý Kho Hàng** | Quản lý phiếu nhập kho & Cá thể hóa mã Serial/IMEI | `[Completed]` | Truy vết xuất/nhập/tồn theo từng máy |
| **Chăm Sóc & Hậu Mãi** | Tra cứu & Đăng ký bảo hành điện tử | `[Completed]` | Tự động kích hoạt khi hóa đơn thanh toán |
| **Đánh Giá & Phản Hồi** | Khách hàng đánh giá sao và bình luận sản phẩm | `[Completed]` | Admin có thể duyệt và phản hồi đánh giá |
| **Quy Hoạch Thư Mục** | Sắp xếp lại toàn bộ thư mục & hợp nhất database | `[Completed]` | `Database/`, `docs/`, `scripts/`, `rules/` |
| **Báo Cáo & Phân Tích** | Biểu đồ trực quan hóa doanh thu tháng / năm | `[In Progress]` | Dashboard trực quan cho quản trị viên |
