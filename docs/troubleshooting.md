# CẨM NANG XỬ LÝ SỰ CỐ (TROUBLESHOOTING GUIDE)

Tài liệu ghi nhận các sự cố thường gặp, nguyên nhân cốt lõi và phương án khắc phục nhanh chóng trong quá trình phát triển dự án **BTL_PTTKYC**.

---

## 1. Sự Cố Kết Nối SQL Server (Cannot Connect to SQL Server)

### Hiện tượng
- Ứng dụng báo lỗi: `SqlException: A network-related or instance-specific error occurred while establishing a connection to SQL Server.`
- Các trang web không load được dữ liệu hoặc văng lỗi HTTP 500.

### Nguyên nhân
1. Dịch vụ SQL Server chưa được khởi chạy (Service Stopped).
2. Chuỗi kết nối trong `appsettings.json` chưa đúng tên Server Name hoặc Instance Name.
3. SQL Server chưa cấu hình `TrustServerCertificate=True`.

### Cách khắc phục
1. Mở **Services (services.msc)**, tìm dịch vụ `SQL Server (MSSQLSERVER)` hoặc `SQL Server (SQLEXPRESS)` và chọn **Start**.
2. Kiểm tra chuỗi kết nối trong [appsettings.json](file:///d:/Downloads/BTL_PTTKYC/appsettings.json):
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=PhoneStoreDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;"
   }
   ```
   *(Nếu dùng SQL Express, đổi `Server=localhost` thành `Server=localhost\\SQLEXPRESS` hoặc `Server=.\\SQLEXPRESS`).*

---

## 2. Lỗi Trigger Chặn Thao Tác Bán Hàng Khi Hết Tồn Kho (Error 50003)

### Hiện tượng
- Báo lỗi: `SqlException: Not enough product quantity in stock.` (Mã lỗi `50003`).

### Nguyên nhân
- Trigger `trg_InvoiceDetail_CheckBeforeInsert` chặn thao tác thêm sản phẩm vào hóa đơn nếu số lượng đặt mua vượt quá số lượng tồn kho thực tế (`quantity` trong bảng `Product`).

### Cách khắc phục
1. Kiểm tra số lượng tồn kho khả dụng của sản phẩm qua View `vw_ProductCatalog`.
2. Tạo thêm phiếu nhập kho (gọi Stored Procedure `usp_CreateImportReceipt` hoặc chức năng Nhập kho trên trang Quản lý Kho) để bổ sung tồn kho cho sản phẩm trước khi xuất bán.

---

## 3. Lỗi Không Mở Lại Hóa Đơn Đã Hủy (Error 50004)

### Hiện tượng
- Báo lỗi: `SqlException: A cancelled invoice cannot be reopened.` (Mã lỗi `50004`).

### Nguyên nhân
- Trigger `trg_Invoice_AfterUpdateStatus` bảo vệ tính toàn vẹn: khi hóa đơn đã chuyển sang trạng thái `Cancelled`, toàn bộ hàng hóa đã được tự động hoàn tồn kho, do đó hệ thống tuyệt đối cấm chuyển trạng thái từ `Cancelled` về `Pending` hay `Paid`.

### Cách khắc phục
- Nếu khách hàng muốn tiếp tục mua hàng, tạo một đơn hàng/hóa đơn mới thay vì cố gắng sửa lại hóa đơn đã bị hủy.

---

## 4. Lỗi Đăng Nhập Tài Khoản Mặc Định Thất Bại

### Hiện tượng
- Nhập tài khoản `admin` / `admin123` nhưng hệ thống báo "Tên đăng nhập hoặc mật khẩu không chính xác".

### Nguyên nhân
- Khi vừa chạy lại file script SQL, cột `password_hash` của tài khoản đang ở giá trị placeholder `'REPLACE_WITH_BCRYPT_HASH'`.

### Cách khắc phục
- Lớp `DbInitializer.cs` được gọi tự động khi khởi động ứng dụng trong `Program.cs`. Hãy chạy lại ứng dụng bằng lệnh:
  ```powershell
  dotnet run
  ```
  `DbInitializer` sẽ tự động quét các tài khoản chưa có hash BCrypt hợp lệ và tự động sinh mã băm chuẩn cho:
  - `admin` -> `admin123`
  - `sales` -> `sales123`
  - `kho`   -> `kho123`

---

## 5. Sự Cố Khởi Chạy Cloudflare Tunnel

### Hiện tượng
- Chạy script `scripts/run_with_cloudflare.bat` báo lỗi `Khong tim thay cloudflared.exe!`.

### Nguyên nhân
- File thực thi `cloudflared.exe` chưa được đặt tại các đường dẫn mặc định (`d:\cloudflared.exe` hoặc `d:\Downloads\NhaGiaKim\cloudflared.exe`).

### Cách khắc phục
1. Tải `cloudflared-windows-amd64.exe` từ trang chính thức của Cloudflare và đổi tên thành `cloudflared.exe`.
2. Đặt file vào ổ đĩa `D:\cloudflared.exe`.
3. Chạy lại script [scripts/run_with_cloudflare.bat](file:///d:/Downloads/BTL_PTTKYC/scripts/run_with_cloudflare.bat).
