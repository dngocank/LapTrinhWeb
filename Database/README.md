# HƯỚNG DẪN CƠ SỞ DỮ LIỆU (DATABASE SETUP GUIDE) - PHONESTOREDB

Thư mục này chứa toàn bộ định nghĩa cơ sở dữ liệu và dữ liệu mẫu chuẩn của hệ thống Quản lý Cửa hàng Điện thoại Di động & Thiết bị Công nghệ (`BTL_PTTKYC`).

---

## 1. File Script Chính

- **[PhoneStoreDB_Full.sql](file:///d:/Downloads/BTL_PTTKYC/Database/PhoneStoreDB_Full.sql)**: File script T-SQL **duy nhất và toàn diện**, tích hợp toàn bộ:
  - **20 Bảng Dữ Liệu**: `Brand`, `Category`, `MembershipRank`, `Product`, `Supplier`, `Customer`, `Employee`, `Account`, `Warehouse`, `Product_Serial` (IMEI/Serial), `Import_Receipt`, `Import_Detail`, `Promotion`, `Invoice`, `Invoice_Detail`, `Payment`, `Warranty`, `Stock_Movement`, `Review`.
  - **8 Views**: `vw_ProductCatalog`, `vw_InvoiceFull`, `vw_MonthlyRevenue`, `vw_BestSellingProducts`, `vw_LowStockProducts`, `vw_CustomerSummary`, `vw_WarrantyStatus`, `vw_AccountSecurity`.
  - **6 Functions**: `fn_GetInvoiceSubTotal`, `fn_GetProductStock`, `fn_GetPromotionPercent`, `fn_GetCustomerRank`, `fn_GetRevenueByDateRange`, `fn_GetInvoicesByCustomer`.
  - **7 Triggers**: Tự động tính tổng tiền phiếu nhập, kiểm tra tồn kho trước khi bán, trừ kho khi lập hóa đơn, tự động nâng hạng thẻ thành viên, hoàn tồn kho khi hủy hóa đơn, chuyển trạng thái thanh toán và bảo hành.
  - **13 Stored Procedures**: CRUD sản phẩm, khách hàng, tạo đơn hàng kèm giao dịch, hủy đơn hàng có lý do và người phê duyệt, quản lý phiếu nhập và báo cáo.
  - **Bộ Dữ Liệu Mẫu Đầy Đủ (Seed Data)**: Toàn bộ danh mục phong cách 24hStore (iPhone Duo, iPhone 18/17/16/15/14/13/11, Samsung Galaxy, Xiaomi, Oppo, Vivo, iPad, Tablet, Apple Watch, Smartwatch, Tai nghe AirPods, Phụ kiện Baseus, Havit, Sạc cáp chính hãng).

---

## 2. Cách Chạy Script Trên SQL Server

### Cách 1: Sử dụng SQL Server Management Studio (SSMS) hoặc Azure Data Studio
1. Mở **SSMS** hoặc **Azure Data Studio**, kết nối đến SQL Server Instance của bạn (ví dụ `localhost` hoặc `.` hoặc `.\SQLEXPRESS`).
2. Mở file [Database/PhoneStoreDB_Full.sql](file:///d:/Downloads/BTL_PTTKYC/Database/PhoneStoreDB_Full.sql).
3. Nhấn **Execute (F5)** để khởi tạo toàn bộ CSDL `PhoneStoreDB`.

### Cách 2: Sử dụng dòng lệnh `sqlcmd`
```powershell
sqlcmd -S localhost -E -i "Database\PhoneStoreDB_Full.sql"
```

---

## 3. Tài Khoản Mặc Định Ban Đầu

| Tên Đăng Nhập | Mật Khẩu Khởi Tạo | Vai Trò (Role) | Chức Năng Chính |
| :--- | :--- | :--- | :--- |
| `admin` | `admin123` | **Admin** | Quản trị toàn hệ thống, phê duyệt hủy đơn, báo cáo doanh thu |
| `sales` | `sales123` | **Sales** | Bán lẻ, tư vấn, tạo đơn hàng, thanh toán tại quầy |
| `kho` | `kho123` | **Warehouse** | Quản lý kho, nhập hàng, quản lý mã Serial/IMEI, bảo hành |

*(Lưu ý: Mật khẩu được tự động băm BCrypt chuẩn an toàn thông qua cơ chế `DbInitializer.cs` khi khởi chạy ứng dụng).*
