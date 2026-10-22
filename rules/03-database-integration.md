# 3. Nguyên Tắc Tương Tác Cơ Sở Dữ Liệu (Database Integration)

Tận dụng tối đa thành phần CSDL `PhoneStoreDB` đã được thiết kế hoàn chỉnh trong [PhoneStoreDB_Full.sql](file:///d:/Downloads/BTL_PTTKYC/Database/PhoneStoreDB_Full.sql).

---

## 3.1. Cấu Trúc 19 Bảng Dữ Liệu (Tables)
1. `Brand`: Thương hiệu điện thoại / phụ kiện (Apple, Samsung, Xiaomi,...).
2. `Category`: Phân loại sản phẩm (Điện thoại, Tablet, Tai nghe, Sạc cáp,...).
3. `MembershipRank`: Hạng thành viên (Standard, Silver, Gold, Platinum).
4. `Product`: Thông tin sản phẩm cốt lõi (Tên, cấu hình, giá, số lượng tồn).
5. `Supplier`: Nhà cung cấp thiết bị và linh kiện.
6. `Customer`: Khách hàng, thông tin liên lạc và tổng chi tiêu (`total_spent`).
7. `Employee`: Nhân viên cửa hàng (Sales, Warehouse, Manager).
8. `Account`: Tài khoản hệ thống (Username, mật khẩu băm, Role, trạng thái).
9. `Warehouse`: Thông tin kho hàng vật lý.
10. `Product_Serial`: Quản lý từng máy theo mã Serial / IMEI cá thể hóa.
11. `Import_Receipt`: Phiếu nhập hàng từ nhà cung cấp.
12. `Import_Detail`: Chi tiết số lượng, đơn giá nhập theo từng sản phẩm.
13. `Promotion`: Mã giảm giá, chương trình khuyến mãi và thời hạn hiệu lực.
14. `Invoice`: Hóa đơn bán hàng / Đơn đặt hàng.
15. `Invoice_Detail`: Chi tiết các mặt hàng bán ra trong hóa đơn.
16. `Payment`: Giao dịch thanh toán (Tiền mặt, Chuyển khoản, Thẻ).
17. `Warranty`: Thông tin bảo hành của sản phẩm / Serial/IMEI đã bán.
18. `Stock_Movement`: Lịch sử biến động xuất / nhập / điều chuyển tồn kho.
19. `Review`: Đánh giá, xếp hạng và phản hồi từ khách hàng.

---

## 3.2. 8 Views (Dùng cho Màn Hình Danh Sách, Tra Cứu, Báo Cáo & An Toàn)
Bắt buộc truy vấn qua View cho các màn hình tổng hợp để tối ưu hiệu năng:
- `vw_ProductCatalog`: Danh mục sản phẩm kinh doanh kèm hãng, danh mục, trạng thái tồn kho (`In stock`, `Low stock`, `Out of stock`).
- `vw_InvoiceFull`: Chi tiết hóa đơn đầy đủ kèm thông tin khách hàng, nhân viên bán hàng, chiết khấu và sản phẩm.
- `vw_MonthlyRevenue`: Thống kê doanh thu theo tháng / năm.
- `vw_BestSellingProducts`: Danh sách sản phẩm bán chạy nhất theo số lượng và doanh thu.
- `vw_LowStockProducts`: Cảnh báo các sản phẩm sắp hết hàng (tồn kho $\le 5$).
- `vw_CustomerSummary`: Tổng quan khách hàng, hạng thẻ thành viên, số đơn đã mua và tổng chi tiêu.
- `vw_WarrantyStatus`: Theo dõi trạng thái bảo hành của từng máy theo Serial / IMEI.
- `vw_AccountSecurity`: An toàn tài khoản, che giấu mã băm mật khẩu khi truy vấn hiển thị.

---

## 3.3. 13 Stored Procedures (Nghiệp Vụ Chính Kèm Giao Dịch)
Bắt buộc dùng Stored Procedure cho các nghiệp vụ chính có transaction:
- **Sản phẩm**:
  - `usp_Product_Search`: Tìm kiếm sản phẩm theo từ khóa, hãng, tầm giá.
  - `usp_Product_Insert`: Thêm mới sản phẩm kèm xác thực ban đầu.
  - `usp_Product_Update`: Cập nhật thông số và giá bán sản phẩm.
  - `usp_Product_Delete`: Xóa hoặc vô hiệu hóa sản phẩm.
- **Khách hàng**:
  - `usp_Customer_Insert`: Thêm khách hàng mới.
  - `usp_Customer_Update`: Cập nhật thông tin khách hàng.
- **Đơn hàng & Bán lẻ**:
  - `usp_CreateInvoice`: Tạo hóa đơn mới kèm danh sách chi tiết mặt hàng và gán IMEI.
  - `usp_CancelInvoice`: Hủy hóa đơn và kích hoạt hoàn tồn kho.
  - `usp_AddPayment`: Thêm thanh toán cho hóa đơn.
  - `usp_GetInvoiceDetail`: Lấy toàn bộ chi tiết hóa đơn.
- **Kho & Nhập hàng**:
  - `usp_CreateImportReceipt`: Tạo phiếu nhập kho và nạp danh sách Serial/IMEI tương ứng.
- **Báo cáo & Bảo hành**:
  - `usp_GetRevenueReport`: Trích xuất báo cáo doanh thu theo kỳ.
  - `usp_RegisterWarranty`: Đăng ký kích hoạt bảo hành điện tử.

---

## 3.4. 6 Functions (Tính Toán & Tra Cứu Phụ Trợ)
Gọi trực tiếp trong câu lệnh SQL hoặc Store khi cần tính toán phụ trợ:
- `fn_GetInvoiceSubTotal`: Tính tiền tạm tính trước thuế/giảm giá của hóa đơn.
- `fn_GetProductStock`: Lấy số lượng tồn kho khả dụng tức thời của một sản phẩm.
- `fn_GetPromotionPercent`: Lấy phần trăm giảm giá khuyến mãi còn hiệu lực.
- `fn_GetCustomerRank`: Lấy hạng thành viên dựa trên tổng mức chi tiêu.
- `fn_GetRevenueByDateRange`: Tính tổng doanh thu trong khoảng ngày bất kỳ.
- `fn_GetInvoicesByCustomer`: Bảng danh sách toàn bộ hóa đơn của một khách hàng cụ thể.

---

## 3.5. 7 Triggers (Xử Lý Tự Động & Ràng Buộc Nghiệp Vụ)
Lập trình viên cần hiểu rõ Trigger để chủ động `try-catch` và thông báo lỗi:
- `trg_ImportDetail_AfterInsert`: Tự động cộng tồn kho `Product.quantity` và cập nhật tổng tiền phiếu nhập.
- `trg_InvoiceDetail_CheckBeforeInsert`: Kiểm tra tồn kho trước khi bán, chặn bán khi hết hàng (`RAISERROR`).
- `trg_InvoiceDetail_AfterInsert`: Tự động trừ tồn kho `Product.quantity` khi xuất hóa đơn.
- `trg_Invoice_UpdateCustomerRank`: Tự động tích lũy `Customer.total_spent` và cập nhật hạng thành viên (`MembershipRank`).
- `trg_Invoice_AfterUpdateStatus`: Xử lý hoàn kho khi đơn hàng bị hủy hoặc xuất kho khi xác nhận đơn.
- `trg_Payment_AfterInsert`: Cập nhật trạng thái thanh toán của hóa đơn khi đã thanh toán đủ tiền.
- `trg_Warranty_SetStatus`: Tự động kích hoạt thời hạn bảo hành cho sản phẩm/serial bán ra.

---

## 3.6. Phòng Chống Tấn Công SQL Injection
- **Cấm kỵ**: Tuyệt đối KHÔNG cộng chuỗi SQL (ví dụ: `string.Format`, chuỗi nội suy `$"SELECT ... WHERE id = '{id}'"`).
- **Quy tắc**: Luôn luôn sử dụng `SqlParameter` cho mọi câu truy vấn qua ADO.NET:
  ```csharp
  cmd.Parameters.Add(new SqlParameter("@productId", SqlDbType.Int) { Value = productId });
  ```
