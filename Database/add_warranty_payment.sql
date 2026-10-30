USE PhoneStoreDB;
GO

-- 1. Thêm các cột thanh toán vào bảng Warranty_Ticket nếu chưa có
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Warranty_Ticket' AND COLUMN_NAME='payment_status')
BEGIN
    ALTER TABLE Warranty_Ticket ADD payment_status VARCHAR(30) NOT NULL CONSTRAINT DF_WarrantyTicket_PaymentStatus DEFAULT 'Unpaid';
END
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Warranty_Ticket' AND COLUMN_NAME='payment_method')
BEGIN
    ALTER TABLE Warranty_Ticket ADD payment_method NVARCHAR(50) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Warranty_Ticket' AND COLUMN_NAME='paid_amount')
BEGIN
    ALTER TABLE Warranty_Ticket ADD paid_amount DECIMAL(18,2) NOT NULL CONSTRAINT DF_WarrantyTicket_PaidAmount DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Warranty_Ticket' AND COLUMN_NAME='paid_at')
BEGIN
    ALTER TABLE Warranty_Ticket ADD paid_at DATETIME NULL;
END
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Warranty_Ticket' AND COLUMN_NAME='discount_amount')
BEGIN
    ALTER TABLE Warranty_Ticket ADD discount_amount DECIMAL(18,2) NOT NULL CONSTRAINT DF_WarrantyTicket_Discount DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Warranty_Ticket' AND COLUMN_NAME='voucher_code')
BEGIN
    ALTER TABLE Warranty_Ticket ADD voucher_code VARCHAR(50) NULL;
END
GO

-- Cập nhật dữ liệu mặc định:
-- Nếu repair_cost = 0 => payment_status = 'Free'
UPDATE Warranty_Ticket 
SET payment_status = 'Free', paid_amount = 0
WHERE repair_cost = 0;

-- Nếu phiếu số 4 đã Delivered và có chi phí => đánh dấu đã thanh toán tiền mặt demo
UPDATE Warranty_Ticket 
SET payment_status = 'Paid',
    payment_method = N'Tiền mặt',
    paid_amount = repair_cost,
    paid_at = GETDATE()
WHERE ticket_id = 4 AND repair_cost > 0;
GO

-- 2. Cập nhật VIEW vw_WarrantyTicketDetail để bao gồm các cột thanh toán
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
    -- Các cột thanh toán mới
    t.payment_status,
    t.payment_method,
    t.paid_amount,
    t.paid_at,
    t.discount_amount,
    t.voucher_code,
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
