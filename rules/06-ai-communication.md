# 6. Cách Trình Bày Phản Hồi Của AI (Assistant Communication)

Nhằm đảm bảo tính minh bạch, dễ theo dõi và đồng bộ trong suốt quá trình phát triển dự án, Trợ lý AI (Antigravity) cần tuân thủ các chuẩn giao tiếp sau:

## 6.1. Định Dạng File Rõ Ràng (Clickable File Links)
- Trước mỗi khối mã nguồn hoặc khi đề cập đến một tệp tin, luôn ghi rõ đường dẫn đầy đủ dạng GitHub-style markdown link với tiền tố `file:///`:
  - Ví dụ chuẩn: `[ProductRepository.cs](file:///d:/Downloads/BTL_PTTKYC/Data/ProductRepository.cs)` hoặc `[site.css](file:///d:/Downloads/BTL_PTTKYC/wwwroot/css/site.css)`.
  - Giúp người dùng có thể click trực tiếp để mở tệp ngay trên trình soạn thảo.

## 6.2. Giải Thích Súc Tích Về Tư Duy Nghiệp Vụ
- Giải thích ngắn gọn cách hoạt động, bản chất logic trước khi đưa ra mã nguồn:
  - Ví dụ: Trình bày rõ luồng quản lý cá thể hóa IMEI/Serial khi nhập kho và gán vào hóa đơn khi xuất bán.
  - Phân tích cơ chế tính toán chiết khấu tự động theo hạng thành viên (Standard, Silver, Gold, Platinum).
  - Cách xử lý hoàn kho thông qua Trigger `trg_Invoice_AfterUpdateStatus` khi người dùng hủy đơn hàng.

## 6.3. Chỉ Rõ Các Bước Tiếp Theo Trong Roadmap
- Sau mỗi lần hoàn thành một nhiệm vụ hoặc chức năng:
  - Tóm tắt các nội dung và tệp tin đã thực hiện.
  - Đề xuất các bước hành động tiếp theo dựa trên tiến độ thực tế trong [docs/roadmap.md](file:///d:/Downloads/BTL_PTTKYC/docs/roadmap.md).
