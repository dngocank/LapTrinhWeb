# HỆ THỐNG QUẢN LÝ CỬA HÀNG ĐIỆN THOẠI & THIẾT BỊ CÔNG NGHỆ (KITTYPHONE)
### Bài Tập Lớn: Phân Tích & Thiết Kế Hệ Thống Yêu Cầu (BTL_PTTKYC)

> **Công nghệ**: ASP.NET Core MVC (.NET 10), C#, Microsoft SQL Server (`PhoneStoreDB`), ADO.NET (`Microsoft.Data.SqlClient`), BCrypt.Net-Next.  
> **Nhận diện giao diện**: Storefront & Dashboard thiết kế theo chuẩn chuỗi bán lẻ công nghệ 24hStore.vn (Responsive, Clean, Tối ưu chuyển đổi).

---

## 1. Cấu Trúc Dự Án Đã Quy Hoạch (Project Structure)

```
BTL_PTTKYC/
│
├── .agents/                        # Cấu hình Antigravity AI Agent
│   ├── rules/
│   │   └── project_rules.md        # Quy tắc làm việc chính của AI Agent
│   └── skills/
│       └── design-system-24hstore-vn/
│           └── SKILL.md            # Skill định nghĩa Design Token 24hStore.vn
│
├── Controllers/                    # Tầng điều hướng & tiếp nhận request (MVC Controllers)
│   ├── AccountController.cs        # Đăng nhập, đăng ký, đăng xuất, phân quyền
│   ├── AdminController.cs          # Quản trị sản phẩm, duyệt hủy đơn, tài khoản, đánh giá
│   ├── CartController.cs           # Giỏ hàng Session & Đặt hàng One-page checkout
│   ├── HomeController.cs           # Trang chủ, danh mục sản phẩm, bộ lọc, chi tiết
│   ├── SalesController.cs          # Bán hàng tại quầy, tạo hóa đơn, thu ngân
│   └── WarehouseController.cs      # Quản lý kho, nhập hàng, quản lý IMEI/Serial, bảo hành
│
├── Data/                           # Tầng truy xuất dữ liệu (Repository Pattern ADO.NET)
│   ├── AccountRepository.cs        # Nghiệp vụ tài khoản, xác thực BCrypt
│   ├── DbInitializer.cs            # Tự động đồng bộ schema và băm mật khẩu ban đầu
│   ├── OrderRepository.cs          # Tạo đơn, duyệt hủy đơn, thanh toán, hóa đơn
│   ├── ProductRepository.cs        # Tìm kiếm, lọc sản phẩm, CRUD sản phẩm, tồn kho
│   ├── ReviewRepository.cs         # Đánh giá sao, bình luận khách hàng, trả lời
│   └── WarehouseRepository.cs      # Quản lý nhập kho, cá thể hóa IMEI/Serial, bảo hành
│
├── Database/                       # Toàn bộ script Cơ sở dữ liệu chuẩn hóa
│   ├── PhoneStoreDB_Full.sql       # Script CSDL DUY NHẤT & TOÀN DIỆN (20 Bảng, 8 Views, 6 Funcs, 7 Triggers, 13 SPs, Seed Data)
│   └── README.md                   # Hướng dẫn chi tiết cách chạy script trên SQL Server
│
├── docs/                           # Tài liệu kỹ thuật & quy trình dự án
│   ├── changelog.md                # Lịch sử cập nhật mã nguồn theo thời gian
│   ├── design_system.md            # Chuẩn giao diện, token màu sắc, typography 24hStore
│   ├── roadmap.md                  # Theo dõi tiến độ các phân hệ của bài tập lớn
│   └── troubleshooting.md          # Cẩm nang xử lý lỗi kết nối, trigger, mật khẩu
│
├── Models/                         # Data Models, ViewModels & DTOs
│   ├── AccountModel.cs             # Model tài khoản, đăng nhập, phân quyền
│   ├── CartItemModel.cs            # Model sản phẩm trong giỏ hàng
│   ├── ErrorViewModel.cs           # Model thông báo lỗi hệ thống
│   ├── OrderModels.cs              # ViewModel đơn hàng, checkout, chi tiết hóa đơn
│   ├── ProductViewModel.cs         # ViewModel hiển thị sản phẩm, bộ lọc
│   ├── ReviewModel.cs              # Model đánh giá, xếp hạng sao
│   └── WarehouseModels.cs          # ViewModel nhập kho, Serial/IMEI, bảo hành
│
├── rules/                          # Bộ quy chuẩn phát triển chi tiết cho dự án
│   ├── 01-scope-libraries.md       # Giới hạn can thiệp & quản lý thư viện NuGet
│   ├── 02-architecture-coding-standards.md # Chuẩn kiến trúc 3 tầng & coding convention
│   ├── 03-database-integration.md  # Nguyên tắc tương tác CSDL, chống SQL Injection
│   ├── 04-security-error-handling.md # Bảo mật BCrypt, phân quyền Roles, bắt lỗi
│   ├── 05-frontend-razor-views.md  # Chuẩn giao diện Razor Views, cấm inline style/script
│   ├── 06-ai-communication.md      # Quy chuẩn phản hồi của AI assistant
│   ├── 07-docs-workflow.md         # Quy trình duy trì tài liệu và workflow liên tục
│   └── README.md                   # Bảng tổng hợp các quy tắc phát triển
│
├── scripts/                        # Các công cụ và script tiện ích
│   └── run_with_cloudflare.bat     # Khởi chạy ứng dụng và kích hoạt Cloudflare Tunnel
│
├── Views/                          # Razor Views (UI Giao diện)
│   ├── Account/                    # Giao diện Đăng nhập / Đăng ký
│   ├── Admin/                      # Giao diện Quản trị hệ thống & Báo cáo
│   ├── Cart/                       # Giao diện Giỏ hàng & Thanh toán Checkout
│   ├── Home/                       # Giao diện Trang chủ Storefront, Chi tiết sản phẩm
│   ├── Sales/                      # Giao diện Nhân viên Bán hàng tại quầy
│   ├── Shared/                     # Layout chung (_Layout.cshtml, _ValidationScriptsPartial)
│   └── Warehouse/                  # Giao diện Thủ kho (Nhập hàng, Quản lý IMEI, Bảo hành)
│
├── wwwroot/                        # Tệp tĩnh công khai
│   ├── css/site.css                # Toàn bộ CSS phong cách 24hStore.vn
│   └── js/site.js                  # Toàn bộ JavaScript tương tác client-side
│
├── appsettings.json                # Cấu hình kết nối SQL Server & Application settings
├── BTL_PTTKYC.csproj               # File dự án .NET 10 & danh sách NuGet packages
└── Program.cs                      # Điểm khởi chạy ứng dụng & Dependency Injection
```

---

## 2. Hướng Dẫn Cài Đặt & Chạy Ứng Dụng

### Bước 1: Khởi tạo Cơ Sở Dữ Liệu
1. Mở công cụ quản trị SQL Server (**SSMS** hoặc **Azure Data Studio**).
2. Mở file [Database/PhoneStoreDB_Full.sql](file:///d:/Downloads/BTL_PTTKYC/Database/PhoneStoreDB_Full.sql) và nhấn **Execute (F5)**.
3. Toàn bộ cơ sở dữ liệu `PhoneStoreDB` cùng dữ liệu mẫu chuẩn (sản phẩm 24hStore, thương hiệu Apple/Samsung/Xiaomi, tài khoản, khách hàng, phiếu nhập) sẽ được thiết lập tự động.

### Bước 2: Cấu hình chuỗi kết nối
Kiểm tra chuỗi kết nối trong file [appsettings.json](file:///d:/Downloads/BTL_PTTKYC/appsettings.json):
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=PhoneStoreDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;"
}
```

### Bước 3: Chạy ứng dụng
Mở terminal tại thư mục gốc dự án và chạy:
```powershell
dotnet run
```
Truy cập ứng dụng tại trình duyệt: `http://localhost:5190`

---

## 3. Danh Sách Tài Khoản Mặc Định Ban Đầu

| Tên Đăng Nhập | Mật Khẩu | Vai Trò | Phạm Vi Chức Năng |
| :--- | :--- | :--- | :--- |
| `admin` | `admin123` | **Admin** | Quản trị sản phẩm, phê duyệt hủy đơn hàng, quản lý tài khoản, xem thống kê |
| `sales` | `sales123` | **Sales** | Bán hàng trực tiếp tại quầy, tạo hóa đơn bán lẻ, xử lý thanh toán đa kênh |
| `kho` | `kho123` | **Warehouse** | Tạo phiếu nhập kho, quản lý danh sách IMEI/Serial từng máy, đăng ký bảo hành |
| Khách hàng | Tự đăng ký | **Customer** | Mua sắm online, theo dõi trạng thái đơn hàng, gửi đánh giá sản phẩm |
