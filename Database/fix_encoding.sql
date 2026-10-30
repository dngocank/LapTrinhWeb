USE PhoneStoreDB;
GO

-- Cập nhật thông tin nhân viên kỹ thuật
UPDATE Employee 
SET employee_name = N'Nguyễn Văn Kỹ Thuật',
    position = N'Technician'
WHERE employee_id = 4 OR position = 'Technician';

-- Cập nhật tiếng Việt chuẩn cho phiếu 1
UPDATE Warranty_Ticket 
SET customer_name = N'Nguyễn Văn An',
    customer_issue = N'Màn hình chập chờn khi tăng độ sáng, có hiện tượng sọc xanh dọc mép phải',
    device_condition = N'Thân máy còn mới 98%, viền không cấn móp, màn hình kính nguyên bản không nứt',
    diagnosis = N'Hỏng cáp màn hình OLED Super Retina XDR do lỗi phần cứng nhà sản xuất',
    repair_action = N'Thay thế cụm màn hình OLED chính hãng Apple theo diện bảo hành tiêu chuẩn',
    notes = N'Đã test chuẩn 24h, hiển thị sắc nét, cảm ứng mượt mà. Sẵn sàng giao khách'
WHERE ticket_id = 1;

-- Cập nhật tiếng Việt chuẩn cho phiếu 2
UPDATE Warranty_Ticket 
SET customer_name = N'Trần Thị Bình',
    customer_issue = N'Pin tụt nhanh bất thường, máy nóng khi sạc và tự tắt nguồn ở mức 15%',
    device_condition = N'Máy trầy xước nhẹ nắp lưng, mặt kính đẹp, khay sim nguyên vẹn',
    diagnosis = N'Chai pin 68%, cell pin phồng nhẹ gây nghẽn nguồn điện',
    repair_action = N'Thay cell pin chuẩn dung lượng cao chính hãng, dán lại keo chống nước chuẩn IP68',
    notes = N'Đang chạy chu trình sạc xả thử nghiệm hiệu năng pin'
WHERE ticket_id = 2;

-- Cập nhật tiếng Việt chuẩn cho phiếu 3
UPDATE Warranty_Ticket 
SET customer_name = N'Lê Hoàng Long',
    customer_issue = N'Rơi nước, mất loa thoại và mic đàm thoại, camera sau bị mờ do đọng hơi nước',
    device_condition = N'Khung viền cấn nhẹ góc dưới, chân sạc có bụi ẩm',
    diagnosis = N'Nước vào buồng âm thanh loa thoại và ống kính camera, cần vệ sinh sấy khô và thay màng loa',
    repair_action = N'Vệ sinh sấy khô bo mạch bằng cồn chuyên dụng, thay màng loa thoại chống nước',
    notes = N'Khách đồng ý phí dịch vụ hỗ trợ ngoài hạn bảo hành'
WHERE ticket_id = 3;

-- Cập nhật tiếng Việt chuẩn cho phiếu 4
UPDATE Warranty_Ticket 
SET customer_name = N'Dương Ngọc Ánh',
    customer_issue = N'Không nhận sạc pin, lỏng chân cắm sạc',
    device_condition = N'Máy còn mới, có xước dăm quanh viền chân sạc',
    diagnosis = N'Bụi bẩn bám chân sạc Type-C, lỏng tiếp điểm sạc',
    repair_action = N'Vệ sinh cụm sạc, căn chỉnh lại tiếp điểm chân sạc Type-C',
    notes = N'Đã sửa xong, sạc nhanh 25W nhận tốt'
WHERE ticket_id = 4;
GO
