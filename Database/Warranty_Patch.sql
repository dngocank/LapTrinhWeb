-- =====================================================================
-- BỔ SUNG TOÀN DIỆN NGHIỆP VỤ BẢO HÀNH & KỸ THUẬT VIÊN (TECHNICIAN)
-- PhoneStoreDB - Dự án BTL_PTTKYC
-- =====================================================================

USE PhoneStoreDB;
GO

-- 1. Nới lỏng/Cập nhật ràng buộc cho Employee.position và Account.role
ALTER TABLE Employee DROP CONSTRAINT IF EXISTS CK_Employee_Position;
ALTER TABLE Employee ADD CONSTRAINT CK_Employee_Position 
    CHECK (position IN (N'Manager', N'Sales', N'Warehouse Staff', N'Technician', N'Kỹ thuật viên', N'Technical Staff'));
GO

ALTER TABLE Account DROP CONSTRAINT IF EXISTS CK_Account_Role;
ALTER TABLE Account ADD CONSTRAINT CK_Account_Role 
    CHECK (role IN ('Admin', 'Sales', 'Warehouse', 'Technician'));
GO

-- 2. Tạo bảng Phiếu Tiếp Nhận & Xử Lý Bảo Hành (Warranty_Ticket)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Warranty_Ticket')
BEGIN
    CREATE TABLE Warranty_Ticket (
        ticket_id            INT IDENTITY(1,1) NOT NULL,
        ticket_code          VARCHAR(50)       NOT NULL,
        warranty_id          INT               NULL,
        serial_id            INT               NULL,
        customer_id          INT               NULL,
        customer_name        NVARCHAR(100)     NOT NULL,
        customer_phone       VARCHAR(15)       NOT NULL,
        technician_id        INT               NULL,
        receive_date         DATETIME          NOT NULL CONSTRAINT DF_WarrantyTicket_ReceiveDate DEFAULT GETDATE(),
        expected_return_date DATE              NULL,
        completed_date       DATETIME          NULL,
        customer_issue       NVARCHAR(1000)    NOT NULL,
        device_condition     NVARCHAR(500)     NULL,
        diagnosis            NVARCHAR(1000)    NULL,
        repair_action        NVARCHAR(1000)    NULL,
        repair_cost          DECIMAL(18,2)     NOT NULL CONSTRAINT DF_WarrantyTicket_Cost DEFAULT 0,
        status               VARCHAR(30)       NOT NULL CONSTRAINT DF_WarrantyTicket_Status DEFAULT 'Received',
        notes                NVARCHAR(500)     NULL,
        created_at           DATETIME          NOT NULL CONSTRAINT DF_WarrantyTicket_CreatedAt DEFAULT GETDATE(),
        CONSTRAINT PK_Warranty_Ticket PRIMARY KEY (ticket_id),
        CONSTRAINT UQ_Warranty_Ticket_Code UNIQUE (ticket_code),
        CONSTRAINT FK_WarrantyTicket_Warranty   FOREIGN KEY (warranty_id)   REFERENCES Warranty(warranty_id),
        CONSTRAINT FK_WarrantyTicket_Serial     FOREIGN KEY (serial_id)     REFERENCES Product_Serial(serial_id),
        CONSTRAINT FK_WarrantyTicket_Customer   FOREIGN KEY (customer_id)   REFERENCES Customer(customer_id),
        CONSTRAINT FK_WarrantyTicket_Technician FOREIGN KEY (technician_id) REFERENCES Employee(employee_id),
        CONSTRAINT CK_WarrantyTicket_Status     CHECK (status IN ('Received', 'Diagnosing', 'Processing', 'WaitingParts', 'Completed', 'Delivered', 'Cancelled')),
        CONSTRAINT CK_WarrantyTicket_Cost       CHECK (repair_cost >= 0),
        CONSTRAINT CK_WarrantyTicket_Dates      CHECK (completed_date IS NULL OR completed_date >= receive_date)
    );

    CREATE INDEX IX_WarrantyTicket_Code       ON Warranty_Ticket(ticket_code);
    CREATE INDEX IX_WarrantyTicket_Status     ON Warranty_Ticket(status);
    CREATE INDEX IX_WarrantyTicket_Technician ON Warranty_Ticket(technician_id);
    CREATE INDEX IX_WarrantyTicket_Customer   ON Warranty_Ticket(customer_id);
    CREATE INDEX IX_WarrantyTicket_Serial     ON Warranty_Ticket(serial_id);
END
GO

-- 3. Tạo bảng Linh Kiện / Phụ Kiện Sửa Chữa (Warranty_Ticket_Component)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Warranty_Ticket_Component')
BEGIN
    CREATE TABLE Warranty_Ticket_Component (
        component_id     INT IDENTITY(1,1) NOT NULL,
        ticket_id        INT               NOT NULL,
        component_name   NVARCHAR(150)     NOT NULL,
        quantity         INT               NOT NULL CONSTRAINT DF_WTC_Qty DEFAULT 1,
        price            DECIMAL(18,2)     NOT NULL CONSTRAINT DF_WTC_Price DEFAULT 0,
        note             NVARCHAR(255)     NULL,
        CONSTRAINT PK_Warranty_Ticket_Component PRIMARY KEY (component_id),
        CONSTRAINT FK_WTC_Ticket FOREIGN KEY (ticket_id) REFERENCES Warranty_Ticket(ticket_id) ON DELETE CASCADE,
        CONSTRAINT CK_WTC_Qty   CHECK (quantity > 0),
        CONSTRAINT CK_WTC_Price CHECK (price >= 0)
    );
END
GO

-- 4. VIEW: Tra cứu chi tiết phiếu bảo hành tổng hợp (vw_WarrantyTicketDetail)
IF OBJECT_ID('vw_WarrantyTicketDetail', 'V') IS NOT NULL
    DROP VIEW vw_WarrantyTicketDetail;
GO

CREATE VIEW vw_WarrantyTicketDetail
AS
SELECT
    t.ticket_id,
    t.ticket_code,
    t.warranty_id,
    t.serial_id,
    t.customer_id,
    t.customer_name,
    t.customer_phone,
    t.technician_id,
    e.employee_name AS technician_name,
    e.phone AS technician_phone,
    t.receive_date,
    t.expected_return_date,
    t.completed_date,
    t.customer_issue,
    t.device_condition,
    t.diagnosis,
    t.repair_action,
    t.repair_cost,
    t.status,
    t.notes,
    t.created_at,
    -- Thông tin thiết bị & Serial
    ps.imei_number,
    ps.serial_number,
    p.product_id,
    p.product_name,
    b.brand_name,
    -- Thông tin thẻ bảo hành gốc
    w.warranty_start,
    w.warranty_end,
    w.warranty_status AS original_warranty_status,
    CASE 
        WHEN w.warranty_end IS NULL THEN N'Không có thẻ BH'
        WHEN w.warranty_end < CAST(GETDATE() AS DATE) THEN N'Hết hạn BH'
        ELSE N'Còn hạn BH'
    END AS warranty_coverage_text,
    CASE 
        WHEN w.warranty_end >= CAST(GETDATE() AS DATE) THEN DATEDIFF(DAY, CAST(GETDATE() AS DATE), w.warranty_end)
        ELSE 0
    END AS warranty_days_remaining
FROM Warranty_Ticket t
LEFT JOIN Employee e        ON t.technician_id = e.employee_id
LEFT JOIN Product_Serial ps ON t.serial_id = ps.serial_id
LEFT JOIN Product p         ON ps.product_id = p.product_id
LEFT JOIN Brand b           ON p.brand_id = b.brand_id
LEFT JOIN Warranty w        ON t.warranty_id = w.warranty_id;
GO

-- 5. TRIGGERS Bảo Hành
-- Trigger 1: Cập nhật trạng thái Serial và Thẻ bảo hành khi lập phiếu
IF OBJECT_ID('trg_Warranty_Ticket_AfterInsert', 'TR') IS NOT NULL
    DROP TRIGGER trg_Warranty_Ticket_AfterInsert;
GO

CREATE TRIGGER trg_Warranty_Ticket_AfterInsert
ON Warranty_Ticket
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    -- Đánh dấu Serial là Defective nếu đang bảo hành
    UPDATE ps
    SET ps.status = 'Defective'
    FROM Product_Serial ps
    JOIN inserted i ON i.serial_id = ps.serial_id
    WHERE i.serial_id IS NOT NULL;

    -- Đánh dấu thẻ bảo hành là Claimed nếu có liên kết
    UPDATE w
    SET w.warranty_status = 'Claimed'
    FROM Warranty w
    JOIN inserted i ON i.warranty_id = w.warranty_id
    WHERE i.warranty_id IS NOT NULL;
END;
GO

-- Trigger 2: Tự động ghi nhận ngày hoàn tất khi bàn giao hoặc sửa xong
IF OBJECT_ID('trg_Warranty_Ticket_AfterUpdate', 'TR') IS NOT NULL
    DROP TRIGGER trg_Warranty_Ticket_AfterUpdate;
GO

CREATE TRIGGER trg_Warranty_Ticket_AfterUpdate
ON Warranty_Ticket
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Nếu trạng thái chuyển sang Completed hoặc Delivered mà completed_date đang NULL
    UPDATE t
    SET t.completed_date = GETDATE()
    FROM Warranty_Ticket t
    JOIN inserted i ON i.ticket_id = t.ticket_id
    WHERE i.status IN ('Completed', 'Delivered') AND t.completed_date IS NULL;

    -- Nếu trả máy thành công cho khách hàng ('Delivered'), chuyển trạng thái Serial về 'In Stock' hoặc 'Sold'
    UPDATE ps
    SET ps.status = 'Sold'
    FROM Product_Serial ps
    JOIN inserted i ON i.serial_id = ps.serial_id
    WHERE i.status = 'Delivered' AND i.serial_id IS NOT NULL;
END;
GO

-- 6. FUNCTIONS Bảo Hành
-- Function 1: Tra cứu bảo hành qua IMEI
IF OBJECT_ID('fn_CheckWarrantyByImei', 'IF') IS NOT NULL
    DROP FUNCTION fn_CheckWarrantyByImei;
GO

CREATE FUNCTION fn_CheckWarrantyByImei (@imei VARCHAR(50))
RETURNS TABLE
AS
RETURN (
    SELECT 
        w.warranty_id,
        p.product_id,
        p.product_name,
        b.brand_name,
        ps.serial_id,
        ps.imei_number,
        ps.serial_number,
        c.customer_id,
        c.customer_name,
        c.phone AS customer_phone,
        i.invoice_id,
        i.invoice_date,
        w.warranty_start,
        w.warranty_end,
        w.warranty_status,
        CASE 
            WHEN w.warranty_end < CAST(GETDATE() AS DATE) THEN N'Hết hạn bảo hành'
            ELSE N'Còn trong hạn bảo hành'
        END AS warranty_eligibility,
        CASE 
            WHEN w.warranty_end >= CAST(GETDATE() AS DATE) 
            THEN DATEDIFF(DAY, CAST(GETDATE() AS DATE), w.warranty_end)
            ELSE 0
        END AS days_left
    FROM Product_Serial ps
    JOIN Product p          ON ps.product_id = p.product_id
    JOIN Brand b            ON p.brand_id = b.brand_id
    LEFT JOIN Warranty w    ON w.serial_id = ps.serial_id
    LEFT JOIN Invoice_Detail id ON w.invoice_detail_id = id.invoice_detail_id
    LEFT JOIN Invoice i     ON id.invoice_id = i.invoice_id
    LEFT JOIN Customer c    ON i.customer_id = c.customer_id
    WHERE ps.imei_number = @imei OR ps.serial_number = @imei
);
GO

-- Function 2: Thống kê hiệu suất làm việc của Kỹ thuật viên
IF OBJECT_ID('fn_GetTechnicianPerformance', 'IF') IS NOT NULL
    DROP FUNCTION fn_GetTechnicianPerformance;
GO

CREATE FUNCTION fn_GetTechnicianPerformance (@employee_id INT)
RETURNS TABLE
AS
RETURN (
    SELECT 
        e.employee_id,
        e.employee_name,
        COUNT(t.ticket_id) AS total_received,
        SUM(CASE WHEN t.status IN ('Received', 'Diagnosing', 'Processing', 'WaitingParts') THEN 1 ELSE 0 END) AS in_progress_count,
        SUM(CASE WHEN t.status IN ('Completed', 'Delivered') THEN 1 ELSE 0 END) AS completed_count,
        SUM(CASE WHEN t.status = 'Cancelled' THEN 1 ELSE 0 END) AS cancelled_count,
        ISNULL(SUM(CASE WHEN t.status = 'Delivered' THEN t.repair_cost ELSE 0 END), 0) AS total_repair_revenue
    FROM Employee e
    LEFT JOIN Warranty_Ticket t ON e.employee_id = t.technician_id
    WHERE e.employee_id = @employee_id
    GROUP BY e.employee_id, e.employee_name
);
GO

-- 7. STORED PROCEDURES Bảo Hành
-- Procedure 1: Tiếp nhận máy bảo hành mới
IF OBJECT_ID('usp_CreateWarrantyTicket', 'P') IS NOT NULL
    DROP PROCEDURE usp_CreateWarrantyTicket;
GO

CREATE PROCEDURE usp_CreateWarrantyTicket
    @serial_number_or_imei VARCHAR(50) = NULL,
    @customer_name         NVARCHAR(100),
    @customer_phone        VARCHAR(15),
    @technician_id         INT = NULL,
    @customer_issue        NVARCHAR(1000),
    @device_condition      NVARCHAR(500) = NULL,
    @expected_days         INT = 3,
    @notes                 NVARCHAR(500) = NULL,
    @new_ticket_code       VARCHAR(50) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Tìm serial_id và warranty_id nếu có IMEI / Serial
        DECLARE @found_serial_id INT = NULL;
        DECLARE @found_warranty_id INT = NULL;
        DECLARE @found_customer_id INT = NULL;

        IF @serial_number_or_imei IS NOT NULL AND LEN(TRIM(@serial_number_or_imei)) > 0
        BEGIN
            SELECT TOP 1 
                @found_serial_id = ps.serial_id,
                @found_warranty_id = w.warranty_id,
                @found_customer_id = i.customer_id
            FROM Product_Serial ps
            LEFT JOIN Warranty w ON w.serial_id = ps.serial_id
            LEFT JOIN Invoice_Detail id ON w.invoice_detail_id = id.invoice_detail_id
            LEFT JOIN Invoice i ON id.invoice_id = i.invoice_id
            WHERE ps.imei_number = @serial_number_or_imei OR ps.serial_number = @serial_number_or_imei;
        END

        -- Nếu chưa tìm thấy customer_id, tìm theo SĐT
        IF @found_customer_id IS NULL AND @customer_phone IS NOT NULL
        BEGIN
            SELECT TOP 1 @found_customer_id = customer_id FROM Customer WHERE phone = @customer_phone;
        END

        -- Sinh mã phiếu tự động: BH-YYYYMMDD-XXXX
        DECLARE @today_str VARCHAR(8) = CONVERT(VARCHAR(8), GETDATE(), 112);
        DECLARE @next_num INT;

        SELECT @next_num = ISNULL(MAX(CAST(RIGHT(ticket_code, 4) AS INT)), 0) + 1
        FROM Warranty_Ticket
        WHERE ticket_code LIKE 'BH-' + @today_str + '-%';

        SET @new_ticket_code = 'BH-' + @today_str + '-' + RIGHT('0000' + CAST(@next_num AS VARCHAR(4)), 4);

        -- Thêm vào bảng Warranty_Ticket
        INSERT INTO Warranty_Ticket (
            ticket_code, warranty_id, serial_id, customer_id, customer_name, customer_phone,
            technician_id, receive_date, expected_return_date, customer_issue,
            device_condition, status, notes
        )
        VALUES (
            @new_ticket_code, @found_warranty_id, @found_serial_id, @found_customer_id, @customer_name, @customer_phone,
            @technician_id, GETDATE(), DATEADD(DAY, @expected_days, CAST(GETDATE() AS DATE)), @customer_issue,
            @device_condition, 'Received', @notes
        );

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- Procedure 2: Kỹ thuật viên cập nhật tiến độ, chẩn đoán, biện pháp sửa chữa
IF OBJECT_ID('usp_UpdateWarrantyTicket', 'P') IS NOT NULL
    DROP PROCEDURE usp_UpdateWarrantyTicket;
GO

CREATE PROCEDURE usp_UpdateWarrantyTicket
    @ticket_id        INT,
    @technician_id    INT = NULL,
    @diagnosis        NVARCHAR(1000) = NULL,
    @repair_action    NVARCHAR(1000) = NULL,
    @repair_cost      DECIMAL(18,2) = 0,
    @status           VARCHAR(30),
    @notes            NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Warranty_Ticket
    SET 
        technician_id = COALESCE(@technician_id, technician_id),
        diagnosis = COALESCE(@diagnosis, diagnosis),
        repair_action = COALESCE(@repair_action, repair_action),
        repair_cost = @repair_cost,
        status = @status,
        notes = COALESCE(@notes, notes)
    WHERE ticket_id = @ticket_id;
END;
GO

-- Procedure 3: Bàn giao máy cho khách hàng
IF OBJECT_ID('usp_DeliverWarrantyTicket', 'P') IS NOT NULL
    DROP PROCEDURE usp_DeliverWarrantyTicket;
GO

CREATE PROCEDURE usp_DeliverWarrantyTicket
    @ticket_id INT,
    @notes     NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Warranty_Ticket
    SET 
        status = 'Delivered',
        completed_date = GETDATE(),
        notes = CASE WHEN @notes IS NOT NULL THEN ISNULL(notes + ' | ', '') + @notes ELSE notes END
    WHERE ticket_id = @ticket_id;
END;
GO

-- 8. DỮ LIỆU MẪU NHÂN SỰ KỸ THUẬT & PHIẾU BẢO HÀNH
-- Thêm nhân viên Kỹ thuật nếu chưa có
IF NOT EXISTS (SELECT 1 FROM Employee WHERE position = N'Technician' OR employee_name LIKE N'%Kỹ Thuật%')
BEGIN
    INSERT INTO Employee (employee_name, phone, position)
    VALUES (N'Nguyễn Văn Kỹ Thuật', '0987654321', N'Technician');
END
GO

-- Thêm tài khoản đăng nhập cho nhân viên kỹ thuật: username = 'kythuat', pass = 'kythuat123'
DECLARE @tech_emp_id INT;
SELECT TOP 1 @tech_emp_id = employee_id FROM Employee WHERE position = N'Technician' ORDER BY employee_id DESC;

IF @tech_emp_id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Account WHERE username = 'kythuat')
BEGIN
    -- Mật khẩu mặc định: kythuat123
    INSERT INTO Account (employee_id, username, password_hash, role, is_active)
    VALUES (@tech_emp_id, 'kythuat', '$2a$11$N4k9RzBfH9F5tWk6RkQpEOb9xO2yZl9rP6p9Jz2U2Kx5R7Kz8Z1Xe', 'Technician', 1);
END
GO

-- Cập nhật mật khẩu chính xác bằng BCrypt hash nếu cần qua DbInitializer
-- Thêm một số phiếu bảo hành mẫu để trải nghiệm giao diện
IF NOT EXISTS (SELECT 1 FROM Warranty_Ticket WHERE ticket_code LIKE 'BH-%')
BEGIN
    DECLARE @techId INT;
    SELECT TOP 1 @techId = employee_id FROM Employee WHERE position = N'Technician';

    INSERT INTO Warranty_Ticket (
        ticket_code, warranty_id, serial_id, customer_id, customer_name, customer_phone,
        technician_id, receive_date, expected_return_date, customer_issue,
        device_condition, diagnosis, repair_action, repair_cost, status, notes
    )
    VALUES 
    (
        'BH-20261022-0001', 1, 1, 1, N'Nguyễn Văn An', '0901234567',
        @techId, DATEADD(DAY, -4, GETDATE()), DATEADD(DAY, -1, GETDATE()),
        N'Màn hình chập chờn khi tăng độ sáng, có hiện tượng sọc xanh dọc mép phải',
        N'Thân máy còn mới 98%, viền không cấn móp, màn hình kính nguyên bản không nứt',
        N'Hỏng cáp màn hình OLED Super Retina XDR do lỗi phần cứng nhà sản xuất',
        N'Thay thế cụm màn hình OLED chính hãng Apple theo diện bảo hành tiêu chuẩn',
        0, 'Completed', N'Đã test chuẩn 24h, hiển thị sắc nét, cảm ứng mượt mà. Sẵn sàng giao khách'
    ),
    (
        'BH-20261024-0002', 2, 2, 2, N'Trần Thị Bình', '0912345678',
        @techId, DATEADD(DAY, -2, GETDATE()), DATEADD(DAY, 1, GETDATE()),
        N'Pin tụt nhanh bất thường, máy nóng khi sạc và tự tắt nguồn ở mức 15%',
        N'Máy trầy xước nhẹ nắp lưng, mặt kính đẹp, khay sim nguyên vẹn',
        N'Chai pin 68%, cell pin phồng nhẹ gây nghẽn nguồn điện',
        N'Thay cell pin chuẩn dung lượng cao chính hãng, dán lại keo chống nước chuẩn IP68',
        350000, 'Processing', N'Đang chạy chu trình sạc xả thử nghiệm hiệu năng pin'
    ),
    (
        'BH-20261026-0003', NULL, NULL, NULL, N'Lê Hoàng Long', '0988776655',
        @techId, GETDATE(), DATEADD(DAY, 3, GETDATE()),
        N'Rơi nước, mất loa thoại và mic đàm thoại, camera sau bị mờ do đọng hơi nước',
        N'Khung viền cấn nhẹ góc dưới, chân sạc có bụi ẩm',
        N'Nước vào buồng âm thanh loa thoại và ống kính camera, cần vệ sinh sấy khô và thay màng loa',
        N'Vệ sinh sấy khô bo mạch bằng cồn chuyên dụng, thay màng loa thoại chống nước',
        450000, 'Received', N'Khách đồng ý phí dịch vụ hỗ trợ ngoài hạn bảo hành'
    );
END
GO
