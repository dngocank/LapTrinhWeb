# 1. Giới Hạn Can Thiệp & Quản Lý Thư Viện

## 1.1. Giới Hạn Can Thiệp (Scope of Work)
- **Chỉ sửa đổi đúng phạm vi**: Chỉ thay đổi các file liên quan trực tiếp đến nhiệm vụ hoặc chức năng đang triển khai (Storefront bán hàng, giỏ hàng, quản lý kho Serial/IMEI, quản lý đơn hàng Sales, quản trị Admin).
- **Nguyên tắc bất biến**: Tuyệt đối KHÔNG tự ý refactor cấu trúc dự án hay can thiệp vào các module đang chạy ổn định khi không được yêu cầu.

## 1.2. Quản Lý Thư Viện (NuGet / Client Libraries)
Dự án đã được chuẩn hóa với các package khai báo chính thức trong [BTL_PTTKYC.csproj](file:///d:/Downloads/BTL_PTTKYC/BTL_PTTKYC.csproj):

| Thư Viện (NuGet) | Phiên Bản | Mục Đích Sử Dụng |
| :--- | :--- | :--- |
| **Microsoft.Data.SqlClient** | `v7.1.1` | Kết nối và tương tác với SQL Server qua ADO.NET thuần (Repository Pattern). |
| **BCrypt.Net-Next** | `v4.2.0` | Băm và xác thực mật khẩu an toàn cho tài khoản (`Account`). |
| **Newtonsoft.Json** | `v13.0.4` | Xử lý tuần tự hóa JSON và lưu trữ Session giỏ hàng (`CartItemModel`). |

### Quy tắc nghiêm ngặt:
1. **KHÔNG tự ý cài đặt thêm ORM khác** (như Entity Framework Core, Dapper) hay các thư viện frontend bên ngoài (npm / third-party lib) nếu chưa có yêu cầu cụ thể từ người dùng.
2. Khi thực sự cần bổ sung thư viện mới, phải giải thích rõ lý do kỹ thuật, lợi ích và xin xác nhận trước khi thực hiện.
