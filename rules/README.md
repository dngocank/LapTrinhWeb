# BỘ QUY TẮC DỰ ÁN (PROJECT RULES) - BTL_PTTKYC

Hệ thống Quản lý Cửa hàng Điện thoại Di động & Thiết bị Công nghệ (PhoneStore Management System).

Thư mục `rules/` chứa toàn bộ các tiêu chuẩn, quy ước kỹ thuật và hướng dẫn bắt buộc dành cho đội ngũ phát triển cũng như trợ lý AI (Antigravity AI Agent).

---

## Danh Mục Quy Tắc Chi Tiết

| STT | File Quy Tắc | Mô Tả Trọng Tâm |
| :--- | :--- | :--- |
| **01** | [01-scope-libraries.md](file:///d:/Downloads/BTL_PTTKYC/rules/01-scope-libraries.md) | Giới hạn phạm vi can thiệp, kiểm soát NuGet packages & thư viện client. |
| **02** | [02-architecture-coding-standards.md](file:///d:/Downloads/BTL_PTTKYC/rules/02-architecture-coding-standards.md) | Kiến trúc 3 tầng (Repository Pattern), quy chuẩn đặt tên PascalCase/camelCase, chú thích tiếng Việt. |
| **03** | [03-database-integration.md](file:///d:/Downloads/BTL_PTTKYC/rules/03-database-integration.md) | Tận dụng 19 Tables, 7 Views, 13 Stored Procedures, 6 Functions, 7 Triggers của `PhoneStoreDB`, chống SQL Injection. |
| **04** | [04-security-error-handling.md](file:///d:/Downloads/BTL_PTTKYC/rules/04-security-error-handling.md) | Mã hóa BCrypt, phân quyền Roles, validation dữ liệu và trích xuất lỗi `SqlException` từ Trigger/Procedure. |
| **05** | [05-frontend-razor-views.md](file:///d:/Downloads/BTL_PTTKYC/rules/05-frontend-razor-views.md) | Chuẩn UI/UX bán lẻ phong cách 24hStore.vn, badges trạng thái, cấm `<style>` và `<script>` nội dòng trong Views. |
| **06** | [06-ai-communication.md](file:///d:/Downloads/BTL_PTTKYC/rules/06-ai-communication.md) | Chuẩn link file markdown, giải thích nghiệp vụ trước khi code, đề xuất Roadmap kế tiếp. |
| **07** | [07-docs-workflow.md](file:///d:/Downloads/BTL_PTTKYC/rules/07-docs-workflow.md) | Quy trình duy trì tài liệu `docs/changelog.md`, `docs/roadmap.md`, `docs/troubleshooting.md`. |

---

## Nguyên Tắc Thực Thi Bắt Buộc

1. **Tuyệt đối không tự ý cài đặt thêm ORM khác** (như EF Core, Dapper) hay các thư viện frontend mới mà chưa có sự đồng ý của người dùng.
2. **Không viết câu lệnh SQL trực tiếp trong Controllers hay Views**. Toàn bộ truy xuất dữ liệu phải nằm trong tầng `Data/` (Repositories) và gọi thông qua Stored Procedures / Views / Functions có sẵn.
3. **Mọi script JavaScript tương tác phải tập trung tại `wwwroot/js/site.js`**, toàn bộ CSS nằm tại `wwwroot/css/site.css`. Không viết inline `<script>` hay `<style>` trong file `.cshtml`.
4. **Luôn ghi nhận thay đổi** vào `docs/changelog.md` sau mỗi phiên làm việc.
