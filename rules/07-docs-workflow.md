# 7. Quy Tắc Tài Liệu & Workflow Liên Tục (Documentation & Continuous Workflow)

Mọi thay đổi trong dự án phải được phản ánh và duy trì trong thư mục [docs/](file:///d:/Downloads/BTL_PTTKYC/docs):

---

## 7.1. docs/changelog.md
- **Mục đích**: Ghi nhận lịch sử cập nhật mã nguồn theo thời gian.
- **Quy tắc**:
  - Cập nhật mỗi khi triển khai tính năng mới, sửa lỗi (bug fixes) hoặc điều chỉnh cấu trúc CSDL / Repository.
  - Định dạng chuẩn: Ngày cập nhật `[YYYY-MM-DD]`, tiêu đề ngắn gọn và danh sách các thay đổi chính.

---

## 7.2. docs/roadmap.md
- **Mục đích**: Theo dõi tiến độ tổng thể của Bài tập lớn (BTL_PTTKYC).
- **Quy tắc**:
  - Duy trì trạng thái của các đầu việc: `[Pending]` (Chờ làm), `[In Progress]` (Đang thực hiện), `[Completed]` (Đã hoàn thành).
  - Đối chiếu sát sao với yêu cầu đề bài Phân tích thiết kế hệ thống và nghiệp vụ quản lý cửa hàng điện thoại.

---

## 7.3. docs/troubleshooting.md
- **Mục đích**: Cẩm nang xử lý sự cố kỹ thuật dành cho đội ngũ phát triển.
- **Quy tắc**:
  - Ghi nhận ngay lập tức mọi lỗi gặp phải trong quá trình:
    1. Cấu hình chuỗi kết nối và truy cập Microsoft SQL Server.
    2. Chạy file script khởi tạo CSDL [Database/PhoneStoreDB_Full.sql](file:///d:/Downloads/BTL_PTTKYC/Database/PhoneStoreDB_Full.sql).
    3. Lỗi Trigger chặn thao tác bán hàng khi hết tồn kho (`RAISERROR`).
    4. Lỗi biên dịch build hoặc runtime của .NET 10.
  - Mỗi mục lỗi phải bao gồm: **Mô tả lỗi / Mã lỗi**, **Nguyên nhân cốt lõi**, và **Giải pháp khắc phục chi tiết**.
