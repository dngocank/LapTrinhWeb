# 2. Kiến Trúc & Phong Cách Viết Mã (Coding Standards)

## 2.1. Kiến Trúc Phân Tầng Rõ Ràng (Repository Pattern)

Dự án áp dụng mô hình phân tách trách nhiệm nghiêm ngặt giữa các tầng:

### 1. Models (Thực thể & Dữ liệu)
- **Data Models**: Ánh xạ bảng cơ sở dữ liệu (`OrderModels.cs`, `WarehouseModels.cs`, `AccountModel.cs`, ...).
- **ViewModels**: Chuyên biệt cho hiển thị dữ liệu hoặc tiếp nhận dữ liệu form (`ProductViewModel.cs`, `CheckoutViewModel`, `LoginViewModel`, ...).
- **DTOs**: Dùng để vận chuyển dữ liệu giữa các phân hệ hoặc API.

### 2. Data / Repositories (Tầng Truy Xuất Dữ Liệu)
- Tách riêng toàn bộ tầng truy xuất dữ liệu vào thư mục `Data/`:
  - `ProductRepository.cs`
  - `OrderRepository.cs`
  - `WarehouseRepository.cs`
  - `AccountRepository.cs`
  - `ReviewRepository.cs`
- Toàn bộ truy vấn SQL (gọi Stored Procedures, Views, Functions) phải nằm gọn trong các lớp Repository này.

### 3. Controllers (Tầng Điều Hướng & Nghiệp Vụ Web)
- Điều phối luồng request, kiểm tra xác thực và phân quyền (Roles), gọi xuống Repository và trả về View hoặc JSON:
  - `AccountController.cs`, `AdminController.cs`, `CartController.cs`
  - `HomeController.cs`, `SalesController.cs`, `WarehouseController.cs`
- **Cấm kỵ**: Tuyệt đối KHÔNG viết câu lệnh SQL trực tiếp hay mở `SqlConnection` trong Controller.

### 4. Views (Tầng Giao Diện Razor)
- Razor Views (`.cshtml`) chỉ phụ trách giao diện, nhận và render dữ liệu từ ViewModel, hiển thị badge trạng thái tồn kho, trạng thái đơn hàng.
- Tuyệt đối không truy vấn CSDL hay xử lý nghiệp vụ nặng tại View.

---

## 2.2. Quy Chuẩn Đặt Tên (Naming Conventions)

- **C# Classes, Methods, Properties, Public Members**: Dùng chuẩn `PascalCase`
  - Ví dụ: `ProductRepository`, `CreateOnlineOrder`, `SellingPrice`, `InvoiceStatus`.
- **Biến cục bộ (Local variables), tham số hàm (Parameters)**: Dùng chuẩn `camelCase`
  - Ví dụ: `productId`, `orderTotal`, `connectionString`, `cartItem`.
- **Biến thành viên private (Private fields)**: Bắt đầu bằng dấu gạch dưới `_camelCase`
  - Ví dụ: `_connectionString`, `_configuration`, `_productRepo`.
- **Razor Views**: Đặt tên trùng khớp chính xác với Action Method tương ứng
  - Ví dụ: Action `Index()` -> `Index.cshtml`, Action `Detail()` -> `Detail.cshtml`, Action `Checkout()` -> `Checkout.cshtml`, Action `Inventory()` -> `Inventory.cshtml`.

---

## 2.3. Chú Thích (Comments)
- Sử dụng tiếng Việt ngắn gọn, xúc tích tại các hàm xử lý nghiệp vụ quan trọng:
  - Logic trừ tồn kho và xuất Serial/IMEI.
  - Bắt lỗi `SqlException` từ Trigger hoặc Procedure của SQL Server.
  - Xử lý mã hóa mật khẩu và phân quyền người dùng.
