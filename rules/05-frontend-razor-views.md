# 5. Quy Chuẩn Frontend & Razor Views (UI/UX Standards - 24hStore.vn Style)

## 5.1. Thiết Kế Thẩm Mỹ & Hiện Đại (E-Commerce & Retail Management)
- **Phong cách giao diện**:
  - Giao diện Storefront khách hàng và giao diện Quản trị / Bán hàng / Kho được thiết kế chuyên nghiệp theo phong cách chuỗi bán lẻ công nghệ hàng đầu (như **24hStore.vn**).
  - Kiểu chữ tiêu chuẩn: Font `Roboto` hoặc font hệ thống hiện đại, dễ đọc, khoảng cách dòng thoáng.
  - Bố cục thẻ sản phẩm: Hình ảnh sắc nét, tên sản phẩm nổi bật, giá khuyến mãi & giá niêm yết rõ ràng, thông số kỹ thuật cốt lõi trực quan (RAM, bộ nhớ trong, chip, pin).
  - Bộ lọc sản phẩm đa năng: Lọc theo Hãng (Brand), Dải giá (Price range), Danh mục (Category), Sắp xếp theo giá tăng/giảm hoặc hàng mới.

---

## 5.2. Hệ Thống Badge Trạng Thái Trực Quan

Nhất quán màu sắc và nhãn trạng thái trên toàn bộ các trang:

### 1. Tồn Kho Sản Phẩm (Stock Status)
- `In stock`: Badge xanh lá (`badge-success` / `bg-success`) - Hàng còn dồi dào.
- `Low stock`: Badge vàng cam (`badge-warning` / `bg-warning`) - Tồn kho $\le 5$, cần nhập thêm.
- `Out of stock`: Badge đỏ (`badge-danger` / `bg-danger`) - Hết hàng, khóa nút đặt nhanh.

### 2. Trạng Thái Đơn Hàng (Invoice / Order Status)
- `Pending`: Badge vàng cam - Chờ xử lý / Chờ xác nhận.
- `Confirmed`: Badge xanh dương - Đã xác nhận đơn hàng / Đang chuẩn bị hàng.
- `Paid` / `Delivered`: Badge xanh lá - Đã thanh toán / Giao hàng thành công.
- `Cancelled`: Badge đỏ - Đã hủy đơn (kèm logic hoàn trả kho).

### 3. Trạng Thái Serial / IMEI Thiết Bị
- `InStock`: Badge xanh lá - Thiết bị sẵn sàng trong kho để xuất bán.
- `Sold`: Badge xám / xanh dương nhạt - Đã xuất bán cho hóa đơn cụ thể.
- `Warranty`: Badge tím hoặc vàng - Đang trong quy trình tiếp nhận hoặc gửi bảo hành.

---

## 5.3. Quy Tắc Tách Biệt Script Và Style (Tập Trung Hóa Client Assets)

> [!IMPORTANT]
> Đây là quy định bắt buộc đối với tất cả lập trình viên và AI Agent khi tạo hoặc sửa đổi các file Razor Views (`.cshtml`).

1. **Mọi mã JavaScript tương tác BẮT BUỘC nằm trong [wwwroot/js/site.js](file:///d:/Downloads/BTL_PTTKYC/wwwroot/js/site.js)**:
   - Các tương tác AJAX giỏ hàng, cập nhật số lượng (+ / -), popup modal xác nhận xóa đơn, copy nhanh mã IMEI, lọc sản phẩm bằng AJAX,... phải được viết tập trung tại file này.
2. **TUYỆT ĐỐI KHÔNG viết mã CSS nội dòng (`<style>`) hay Script nội dòng (`<script>`) trực tiếp trong các file Razor Views (`.cshtml`)**:
   - Không được chèn thẻ `<style> ... </style>` trong view.
   - Không được chèn thẻ `<script> ... </script>` trực tiếp trong nội dung view.
3. **Toàn bộ định kiểu CSS đặt trong [wwwroot/css/site.css](file:///d:/Downloads/BTL_PTTKYC/wwwroot/css/site.css)**:
   - Tất cả các class tiện ích, responsive layout, modal style, thẻ card 24hStore style phải được định nghĩa trong `site.css`.
4. **Ngoại lệ duy nhất**: Chỉ các script bundle validation đặc thù của ASP.NET mới được gọi ở cuối file Razor View thông qua cấu trúc:
   ```html
   @section Scripts {
       @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
   }
   ```
