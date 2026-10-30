using Microsoft.Data.SqlClient;

namespace BTL_PTTKYC.Data
{
    public static class DbInitializer
    {
        public static void Initialize(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString)) return;

            try
            {
                using var conn = new SqlConnection(connectionString);
                conn.Open();

                // 1. Ensure columns in Product and Invoice and Review table exist
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Product' AND COLUMN_NAME='image_url')
                            ALTER TABLE Product ADD image_url NVARCHAR(500) NULL;

                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Product' AND COLUMN_NAME='description')
                            ALTER TABLE Product ADD description NVARCHAR(MAX) NULL;

                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Invoice' AND COLUMN_NAME='shipping_address')
                            ALTER TABLE Invoice ADD shipping_address NVARCHAR(255) NULL;

                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Invoice' AND COLUMN_NAME='order_notes')
                            ALTER TABLE Invoice ADD order_notes NVARCHAR(500) NULL;

                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Invoice' AND COLUMN_NAME='payment_method_selected')
                            ALTER TABLE Invoice ADD payment_method_selected NVARCHAR(50) NULL;

                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Invoice' AND COLUMN_NAME='cancel_reason')
                            ALTER TABLE Invoice ADD cancel_reason NVARCHAR(500) NULL;

                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Invoice' AND COLUMN_NAME='cancel_requested_by')
                            ALTER TABLE Invoice ADD cancel_requested_by NVARCHAR(100) NULL;

                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Invoice' AND COLUMN_NAME='cancel_requested_at')
                            ALTER TABLE Invoice ADD cancel_requested_at DATETIME NULL;

                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Invoice' AND COLUMN_NAME='cancel_approved_by')
                            ALTER TABLE Invoice ADD cancel_approved_by NVARCHAR(100) NULL;

                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Invoice' AND COLUMN_NAME='cancel_approved_at')
                            ALTER TABLE Invoice ADD cancel_approved_at DATETIME NULL;

                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Customer' AND COLUMN_NAME='email')
                            ALTER TABLE Customer ADD email NVARCHAR(100) NULL;

                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Customer' AND COLUMN_NAME='password_hash')
                            ALTER TABLE Customer ADD password_hash VARCHAR(255) NULL;

                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Customer' AND COLUMN_NAME='is_email_verified')
                            ALTER TABLE Customer ADD is_email_verified BIT NOT NULL CONSTRAINT DF_Customer_EmailVer DEFAULT 0;

                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Customer' AND COLUMN_NAME='is_phone_verified')
                            ALTER TABLE Customer ADD is_phone_verified BIT NOT NULL CONSTRAINT DF_Customer_PhoneVer DEFAULT 0;

                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Customer' AND COLUMN_NAME='otp_code')
                            ALTER TABLE Customer ADD otp_code VARCHAR(10) NULL;

                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Customer' AND COLUMN_NAME='otp_expiry')
                            ALTER TABLE Customer ADD otp_expiry DATETIME NULL;

                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Review')
                        BEGIN
                            CREATE TABLE Review (
                                review_id     INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                                product_id    INT NOT NULL,
                                customer_name NVARCHAR(100) NOT NULL,
                                phone         VARCHAR(15) NULL,
                                rating        INT NOT NULL DEFAULT 5,
                                comment       NVARCHAR(1000) NOT NULL,
                                reply         NVARCHAR(1000) NULL,
                                created_at    DATETIME NOT NULL DEFAULT GETDATE(),
                                CONSTRAINT FK_Review_Product FOREIGN KEY (product_id) REFERENCES Product(product_id) ON DELETE CASCADE,
                                CONSTRAINT CK_Review_Rating CHECK (rating >= 1 AND rating <= 5)
                            );
                        END

                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Review' AND COLUMN_NAME='customer_id')
                            ALTER TABLE Review ADD customer_id INT NULL;

                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Review' AND COLUMN_NAME='invoice_id')
                            ALTER TABLE Review ADD invoice_id INT NULL;

                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Review' AND COLUMN_NAME='is_verified_purchase')
                            ALTER TABLE Review ADD is_verified_purchase BIT NOT NULL CONSTRAINT DF_Review_Verified DEFAULT 1;

                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Promotion' AND COLUMN_NAME='start_hour')
                            ALTER TABLE Promotion ADD start_hour INT NULL;

                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Promotion' AND COLUMN_NAME='end_hour')
                            ALTER TABLE Promotion ADD end_hour INT NULL;

                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Promotion' AND COLUMN_NAME='min_rank_id')
                            ALTER TABLE Promotion ADD min_rank_id INT NULL;
                    ";
                    cmd.ExecuteNonQuery();
                }

                // Cập nhật dữ liệu ràng buộc khuyến mãi
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        -- Xóa bỏ Autumn Sale / Khuyến Mãi Mùa Thu theo yêu cầu người dùng
                        DELETE FROM Promotion WHERE promotion_name LIKE N'%Autumn%' OR promotion_name LIKE N'%Mùa Thu%';

                        -- Cập nhật ràng buộc cho các chương trình khuyến mãi hiện có
                        IF EXISTS (SELECT 1 FROM Promotion WHERE promotion_name LIKE N'%Flash Sale%')
                        BEGIN
                            UPDATE Promotion 
                            SET start_date = '2026-10-18', end_date = '2026-10-22', start_hour = 0, end_hour = 2,
                                description = N'Khung giờ vàng 00:00 - 02:00 sáng từ 18/10 đến 22/10 (ngoài khung giờ này không áp dụng)'
                            WHERE promotion_name LIKE N'%Flash Sale%';
                        END
                        ELSE
                        BEGIN
                            INSERT INTO Promotion (promotion_name, discount_percent, start_date, end_date, start_hour, end_hour, min_rank_id, description)
                            VALUES (N'Flash Sale Giờ Vàng', 15, '2026-10-18', '2026-10-22', 0, 2, NULL, N'Khung giờ vàng 00:00 - 02:00 sáng từ 18/10 đến 22/10 (ngoài khung giờ này không áp dụng)');
                        END

                        IF EXISTS (SELECT 1 FROM Promotion WHERE promotion_name LIKE N'%Mừng Khai Trương%')
                        BEGIN
                            UPDATE Promotion 
                            SET start_date = '2026-10-18', end_date = '2026-10-20', start_hour = NULL, end_hour = NULL,
                                description = N'Chương trình mừng khai trương áp dụng duy nhất trong 3 ngày (18/10 - 20/10)'
                            WHERE promotion_name LIKE N'%Mừng Khai Trương%';
                        END
                        ELSE
                        BEGIN
                            INSERT INTO Promotion (promotion_name, discount_percent, start_date, end_date, start_hour, end_hour, min_rank_id, description)
                            VALUES (N'Mừng Khai Trương KittyPhone', 10, '2026-10-18', '2026-10-20', NULL, NULL, NULL, N'Chương trình mừng khai trương áp dụng duy nhất trong 3 ngày (18/10 - 20/10)');
                        END

                        IF EXISTS (SELECT 1 FROM Promotion WHERE promotion_name LIKE N'%Lễ Hội Công Nghệ%')
                        BEGIN
                            UPDATE Promotion 
                            SET start_date = '2026-10-20', end_date = '2026-10-20', start_hour = NULL, end_hour = NULL,
                                description = N'Duy nhất ngày 20/10 - Siêu hội công nghệ Tech Fest mở và kết thúc trọn vẹn trong ngày'
                            WHERE promotion_name LIKE N'%Lễ Hội Công Nghệ%';
                        END
                        ELSE
                        BEGIN
                            INSERT INTO Promotion (promotion_name, discount_percent, start_date, end_date, start_hour, end_hour, min_rank_id, description)
                            VALUES (N'Lễ Hội Công Nghệ Tech Fest', 8, '2026-10-20', '2026-10-20', NULL, NULL, NULL, N'Duy nhất ngày 20/10 - Siêu hội công nghệ Tech Fest mở và kết thúc trọn vẹn trong ngày');
                        END

                        IF EXISTS (SELECT 1 FROM Promotion WHERE promotion_name LIKE N'%Đặc Quyền Hội Viên%')
                        BEGIN
                            UPDATE Promotion 
                            SET min_rank_id = 2,
                                description = N'Đặc quyền chiết khấu 5% dành riêng cho Hội viên hạng Bạc (Silver) và Hạng Vàng (Gold) trở lên'
                            WHERE promotion_name LIKE N'%Đặc Quyền Hội Viên%';
                        END
                        ELSE
                        BEGIN
                            INSERT INTO Promotion (promotion_name, discount_percent, start_date, end_date, start_hour, end_hour, min_rank_id, description)
                            VALUES (N'Đặc Quyền Hội Viên KittyMember', 5, '2026-01-01', '2026-12-31', NULL, NULL, 2, N'Đặc quyền chiết khấu 5% dành riêng cho Hội viên hạng Bạc (Silver) và Hạng Vàng (Gold) trở lên');
                        END
                    ";
                    cmd.ExecuteNonQuery();
                }

                // 2. Ensure initial account passwords are valid BCrypt hashes
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT username, password_hash FROM Account";
                    using var reader = cmd.ExecuteReader();
                    var toUpdate = new List<(string username, string defaultPassword)>();
                    while (reader.Read())
                    {
                        var user = reader["username"]?.ToString() ?? "";
                        var hash = reader["password_hash"]?.ToString() ?? "";
                        string defaultPass = user switch
                        {
                            "admin" => "admin123",
                            "sales" => "sales123",
                            "kho" => "kho123",
                            "kythuat" => "kythuat123",
                            _ => "123456"
                        };

                        bool match = false;
                        try
                        {
                            match = BCrypt.Net.BCrypt.Verify(defaultPass, hash);
                        }
                        catch
                        {
                            match = false;
                        }

                        if (!match)
                        {
                            toUpdate.Add((user, defaultPass));
                        }
                    }
                    reader.Close();

                    foreach (var item in toUpdate)
                    {
                        using var updateCmd = conn.CreateCommand();
                        updateCmd.CommandText = "UPDATE Account SET password_hash = @hash WHERE username = @user";
                        updateCmd.Parameters.AddWithValue("@hash", BCrypt.Net.BCrypt.HashPassword(item.defaultPassword));
                        updateCmd.Parameters.AddWithValue("@user", item.username);
                        updateCmd.ExecuteNonQuery();
                    }
                }

                // 3. Ensure warranty schema and technician roles
                EnsureWarrantySchemaAndTechnician(conn);

                // 4. Ensure all products have 100% verified working images and rich 24hStore specs
                SyncProductCatalog(conn);
            }
            catch (Exception ex)
            {
                Console.WriteLine("DbInitializer Error: " + ex.Message);
            }
        }

        private static void SyncProductCatalog(SqlConnection conn)
        {
            var products = GetCatalogSpecs();

            foreach (var p in products)
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    IF EXISTS (SELECT 1 FROM Product WHERE product_name = @name)
                    BEGIN
                        UPDATE Product
                        SET image_url = @img,
                            description = @desc
                        WHERE product_name = @name;
                    END
                    ELSE
                    BEGIN
                        INSERT INTO Product (brand_id, category_id, product_name, import_price, selling_price, quantity, is_active, image_url, description)
                        VALUES (@brandId, @catId, @name, @importPrice, @sellingPrice, @qty, 1, @img, @desc);
                    END";

                cmd.Parameters.AddWithValue("@name", p.Name);
                cmd.Parameters.AddWithValue("@brandId", p.BrandId);
                cmd.Parameters.AddWithValue("@catId", p.CategoryId);
                cmd.Parameters.AddWithValue("@importPrice", p.ImportPrice);
                cmd.Parameters.AddWithValue("@sellingPrice", p.SellingPrice);
                cmd.Parameters.AddWithValue("@qty", p.Quantity);
                cmd.Parameters.AddWithValue("@img", p.ImageUrl);
                cmd.Parameters.AddWithValue("@desc", p.Description);
                cmd.ExecuteNonQuery();
            }
        }

        private static void EnsureWarrantySchemaAndTechnician(SqlConnection conn)
        {
            try
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    -- 1. Nới lỏng ràng buộc Position và Role cho Technician
                    ALTER TABLE Employee DROP CONSTRAINT IF EXISTS CK_Employee_Position;
                    ALTER TABLE Employee ADD CONSTRAINT CK_Employee_Position 
                        CHECK (position IN (N'Manager', N'Sales', N'Warehouse Staff', N'Technician', N'Kỹ thuật viên', N'Technical Staff'));

                    ALTER TABLE Account DROP CONSTRAINT IF EXISTS CK_Account_Role;
                    ALTER TABLE Account ADD CONSTRAINT CK_Account_Role 
                        CHECK (role IN ('Admin', 'Sales', 'Warehouse', 'Technician'));

                    -- 2. Đảm bảo bảng Warranty_Ticket tồn tại
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Warranty_Ticket')
                    BEGIN
                        CREATE TABLE Warranty_Ticket (
                            ticket_id            INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            ticket_code          VARCHAR(50)       NOT NULL UNIQUE,
                            warranty_id          INT               NULL REFERENCES Warranty(warranty_id),
                            serial_id            INT               NULL REFERENCES Product_Serial(serial_id),
                            customer_id          INT               NULL REFERENCES Customer(customer_id),
                            customer_name        NVARCHAR(100)     NOT NULL,
                            customer_phone       VARCHAR(15)       NOT NULL,
                            technician_id        INT               NULL REFERENCES Employee(employee_id),
                            receive_date         DATETIME          NOT NULL DEFAULT GETDATE(),
                            expected_return_date DATE              NULL,
                            completed_date       DATETIME          NULL,
                            customer_issue       NVARCHAR(1000)    NOT NULL,
                            device_condition     NVARCHAR(500)     NULL,
                            diagnosis            NVARCHAR(1000)    NULL,
                            repair_action        NVARCHAR(1000)    NULL,
                            repair_cost          DECIMAL(18,2)     NOT NULL DEFAULT 0 CHECK (repair_cost >= 0),
                            status               VARCHAR(30)       NOT NULL DEFAULT 'Received' CHECK (status IN ('Received', 'Diagnosing', 'Processing', 'WaitingParts', 'Completed', 'Delivered', 'Cancelled')),
                            notes                NVARCHAR(500)     NULL,
                            created_at           DATETIME          NOT NULL DEFAULT GETDATE(),
                            CONSTRAINT CK_WarrantyTicket_Dates CHECK (completed_date IS NULL OR completed_date >= receive_date)
                        );
                    END

                    -- 3. Đảm bảo bảng Warranty_Ticket_Component tồn tại
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Warranty_Ticket_Component')
                    BEGIN
                        CREATE TABLE Warranty_Ticket_Component (
                            component_id     INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            ticket_id        INT               NOT NULL REFERENCES Warranty_Ticket(ticket_id) ON DELETE CASCADE,
                            component_name   NVARCHAR(150)     NOT NULL,
                            quantity         INT               NOT NULL DEFAULT 1 CHECK (quantity > 0),
                            price            DECIMAL(18,2)     NOT NULL DEFAULT 0 CHECK (price >= 0),
                            note             NVARCHAR(255)     NULL
                        );
                    END

                    -- 4. Đảm bảo nhân viên Kỹ thuật và Tài khoản kythuat tồn tại
                    IF NOT EXISTS (SELECT 1 FROM Employee WHERE position = N'Technician' OR employee_name LIKE N'%Kỹ Thuật%')
                    BEGIN
                        INSERT INTO Employee (employee_name, phone, position)
                        VALUES (N'Nguyễn Văn Kỹ Thuật', '0987654321', N'Technician');
                    END

                    DECLARE @tId INT;
                    SELECT TOP 1 @tId = employee_id FROM Employee WHERE position = N'Technician' ORDER BY employee_id DESC;

                    IF @tId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Account WHERE username = 'kythuat')
                    BEGIN
                        INSERT INTO Account (employee_id, username, password_hash, role, is_active)
                        VALUES (@tId, 'kythuat', '$2a$11$N4k9RzBfH9F5tWk6RkQpEOb9xO2yZl9rP6p9Jz2U2Kx5R7Kz8Z1Xe', 'Technician', 1);
                    END

                    -- 5. Thêm các cột thanh toán và bảo hành tại nhà vào bảng Warranty_Ticket nếu chưa có
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Warranty_Ticket' AND COLUMN_NAME='payment_status')
                        ALTER TABLE Warranty_Ticket ADD payment_status VARCHAR(30) NOT NULL CONSTRAINT DF_WarrantyTicket_PaymentStatus DEFAULT 'Unpaid';

                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Warranty_Ticket' AND COLUMN_NAME='payment_method')
                        ALTER TABLE Warranty_Ticket ADD payment_method NVARCHAR(50) NULL;

                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Warranty_Ticket' AND COLUMN_NAME='paid_amount')
                        ALTER TABLE Warranty_Ticket ADD paid_amount DECIMAL(18,2) NOT NULL CONSTRAINT DF_WarrantyTicket_PaidAmount DEFAULT 0;

                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Warranty_Ticket' AND COLUMN_NAME='paid_at')
                        ALTER TABLE Warranty_Ticket ADD paid_at DATETIME NULL;

                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Warranty_Ticket' AND COLUMN_NAME='discount_amount')
                        ALTER TABLE Warranty_Ticket ADD discount_amount DECIMAL(18,2) NOT NULL CONSTRAINT DF_WarrantyTicket_Discount DEFAULT 0;

                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Warranty_Ticket' AND COLUMN_NAME='voucher_code')
                        ALTER TABLE Warranty_Ticket ADD voucher_code VARCHAR(50) NULL;

                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Warranty_Ticket' AND COLUMN_NAME='customer_email')
                        ALTER TABLE Warranty_Ticket ADD customer_email NVARCHAR(100) NULL;

                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Warranty_Ticket' AND COLUMN_NAME='is_home_service')
                        ALTER TABLE Warranty_Ticket ADD is_home_service BIT NOT NULL CONSTRAINT DF_WarrantyTicket_IsHome DEFAULT 0;

                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Warranty_Ticket' AND COLUMN_NAME='pickup_address')
                        ALTER TABLE Warranty_Ticket ADD pickup_address NVARCHAR(255) NULL;

                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Warranty_Ticket' AND COLUMN_NAME='pickup_time')
                        ALTER TABLE Warranty_Ticket ADD pickup_time DATETIME NULL;

                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Warranty_Ticket' AND COLUMN_NAME='service_type')
                        ALTER TABLE Warranty_Ticket ADD service_type NVARCHAR(50) NULL;

                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Warranty_Ticket' AND COLUMN_NAME='shipping_carrier')
                        ALTER TABLE Warranty_Ticket ADD shipping_carrier NVARCHAR(50) NULL;

                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Warranty_Ticket' AND COLUMN_NAME='shipping_fee')
                        ALTER TABLE Warranty_Ticket ADD shipping_fee DECIMAL(18,2) NOT NULL CONSTRAINT DF_WarrantyTicket_ShipFee DEFAULT 0;

                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Warranty_Ticket' AND COLUMN_NAME='shipping_distance_km')
                        ALTER TABLE Warranty_Ticket ADD shipping_distance_km DECIMAL(6,2) NULL;

                    -- 6. Đảm bảo bảng Warranty_Notification tồn tại để lưu vết Email & SMS
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Warranty_Notification')
                    BEGIN
                        CREATE TABLE Warranty_Notification (
                            notification_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            ticket_id       INT NOT NULL,
                            ticket_code     VARCHAR(50) NOT NULL,
                            customer_id     INT NULL,
                            channel         VARCHAR(20) NOT NULL,
                            recipient       NVARCHAR(100) NOT NULL,
                            title           NVARCHAR(255) NOT NULL,
                            message_body    NVARCHAR(MAX) NOT NULL,
                            status          VARCHAR(30) NOT NULL DEFAULT 'Sent',
                            sent_at         DATETIME NOT NULL DEFAULT GETDATE()
                        );
                    END

                    -- 7. Thêm các cột vận chuyển ship về tận nhà vào bảng Invoice nếu chưa có
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Invoice' AND COLUMN_NAME='shipping_carrier')
                        ALTER TABLE Invoice ADD shipping_carrier NVARCHAR(50) NULL;

                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Invoice' AND COLUMN_NAME='shipping_fee')
                        ALTER TABLE Invoice ADD shipping_fee DECIMAL(18,2) NOT NULL CONSTRAINT DF_Invoice_ShipFee DEFAULT 0;

                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Invoice' AND COLUMN_NAME='shipping_distance_km')
                        ALTER TABLE Invoice ADD shipping_distance_km DECIMAL(6,2) NULL;

                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Invoice' AND COLUMN_NAME='shipping_service_type')
                        ALTER TABLE Invoice ADD shipping_service_type NVARCHAR(50) NULL;

                    -- 8. Cập nhật VIEW vw_WarrantyTicketDetail bao gồm đầy đủ cột mới
                    IF OBJECT_ID('vw_WarrantyTicketDetail', 'V') IS NOT NULL
                        DROP VIEW vw_WarrantyTicketDetail;

                    EXEC('
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
                        t.customer_email,
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
                        t.payment_status,
                        t.payment_method,
                        t.paid_amount,
                        t.paid_at,
                        t.discount_amount,
                        t.voucher_code,
                        t.is_home_service,
                        t.pickup_address,
                        t.pickup_time,
                        t.service_type,
                        t.shipping_carrier,
                        t.shipping_fee,
                        t.shipping_distance_km,
                        ps.imei_number,
                        ps.serial_number,
                        p.product_id,
                        p.product_name,
                        b.brand_name,
                        w.warranty_start,
                        w.warranty_end,
                        w.warranty_status AS original_warranty_status,
                        CASE 
                            WHEN w.warranty_end IS NULL THEN N''Không có thẻ BH''
                            WHEN w.warranty_end < CAST(GETDATE() AS DATE) THEN N''Hết hạn BH''
                            ELSE N''Còn hạn BH''
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
                    ');
                ";
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine("EnsureWarrantySchema Error: " + ex.Message);
            }
        }

        private record CatalogItem(int BrandId, int CategoryId, string Name, decimal ImportPrice, decimal SellingPrice, int Quantity, string ImageUrl, string Description);

        private static List<CatalogItem> GetCatalogSpecs()
        {
            return new List<CatalogItem>
            {
                // iPhone Duo Series
                new(1, 1, "iPhone Duo 2TB | Chính hãng Việt Nam", 90000000, 103990000, 5,
                    "https://24hstore.vn/images/products/2026/09/10/large/iphone-duo-2tb-troi-dem_1789000472.jpg",
                    @"Siêu phẩm iPhone Duo màn hình kép gập tương lai độc quyền tại KittyPhone. Thiết kế bản lề titan không nếp gấp, dung lượng đỉnh cao 2TB, trang bị chip xử lý Apple Silicon thế hệ mới tối ưu hóa trí tuệ nhân tạo Apple Intelligence toàn diện.

--- THÔNG SỐ KỸ THUẬT ---
• Màn hình chính: Ultra Retina XDR OLED kép 7.8 inch, 120Hz ProMotion, 3000 nits, HDR10+
• Màn hình phụ: Super Retina OLED 6.3 inch, Ceramic Shield thế hệ mới chống trầy xước
• Vi xử lý (CPU): Apple M4 Pro / A20 Fusion thế hệ mới, Neural Engine 16 nhân
• RAM & Bộ nhớ: 16GB RAM Unified, Bộ nhớ trong 2TB NVMe siêu tốc độ
• Camera sau: Cụm 3 camera 48MP Fusion OIS + 48MP Siêu rộng + 48MP Tiềm vọng Zoom quang 10x
• Camera trước: Camera kép 12MP TrueDepth dưới màn hình
• Pin & Sạc: Thời lượng xem video lên đến 36 giờ, Sạc siêu tốc 45W, MagSafe 25W
• Thiết kế & Khung viền: Titan siêu nhẹ chuẩn hàng không vũ trụ, kháng nước bụi IP68
• Hệ điều hành: iOS 18 tích hợp trọn vẹn Apple Intelligence tiếng Việt"),

                new(1, 1, "iPhone Duo 1TB | Chính hãng Việt Nam", 75000000, 84490000, 6,
                    "https://24hstore.vn/images/products/2026/09/10/large/iphone-duo-1tb-trang_1789000266.jpg",
                    @"iPhone Duo 1TB màn hình mở rộng công nghệ cao, bản lề titan không nếp gấp, camera tiềm vọng thế hệ đột phá, hỗ trợ làm việc đa nhiệm 2 màn hình cùng lúc mượt mà.

--- THÔNG SỐ KỸ THUẬT ---
• Màn hình chính: Ultra Retina XDR OLED kép 7.8 inch, 120Hz ProMotion, 3000 nits
• Màn hình phụ: Super Retina OLED 6.3 inch Ceramic Shield
• Vi xử lý (CPU): Apple Silicon A20 Fusion thế hệ mới
• RAM & Bộ nhớ: 16GB RAM, 1TB ROM NVMe
• Camera sau: Cụm 3 camera 48MP Fusion + 48MP Góc siêu rộng + 48MP Tiềm vọng 10x
• Pin & Sạc: Xem video 35 giờ, sạc nhanh 45W Type-C
• Hệ điều hành: iOS 18 tích hợp Apple Intelligence"),

                new(1, 1, "iPhone Duo 512GB | Chính hãng Việt Nam", 63000000, 71490000, 8,
                    "https://24hstore.vn/images/products/2026/09/10/large/iphone-duo-512gb-troi-dem_1788999777.jpg",
                    @"iPhone Duo 512GB màu Trời Đêm bí ẩn, khung viền siêu cứng cáp, sạc nhanh MagSafe thế hệ mới, bảo hành 12 tháng chính hãng VN/A tại KittyPhone.

--- THÔNG SỐ KỸ THUẬT ---
• Màn hình: Màn hình gập kép 7.8 inch 120Hz ProMotion, màn hình ngoài 6.3 inch
• Vi xử lý (CPU): Apple A20 Fusion, GPU 6 nhân
• RAM & Bộ nhớ: 16GB RAM, 512GB ROM
• Camera: 48MP Fusion + 48MP Ultra Wide + 48MP Tele 10x
• Pin: Thời lượng dùng liên tục cả ngày, sạc nhanh 45W"),

                new(1, 1, "iPhone Duo 256GB | Chính hãng Việt Nam", 56000000, 64990000, 10,
                    "https://24hstore.vn/images/products/2026/09/10/large/iphone-duo-trang_1788999205.jpg",
                    @"iPhone Duo 256GB chính hãng Việt Nam. Thiết kế gập mở sang trọng màu Trắng Tinh Khôi, màn hình OLED ProMotion 120Hz mượt mà, cấu hình đỉnh cao.

--- THÔNG SỐ KỸ THUẬT ---
• Màn hình: Kép gập OLED 7.8 inch 120Hz, màn ngoài 6.3 inch
• Vi xử lý: Apple A20 Fusion
• RAM & Bộ nhớ: 16GB RAM, 256GB ROM
• Camera: 48MP + 48MP + 48MP
• Hệ điều hành: iOS 18"),

                // iPhone 18 Series
                new(1, 1, "iPhone 18 Pro Max 2TB | Chính hãng Việt Nam", 72000000, 80990000, 7,
                    "https://24hstore.vn/images/products/2026/09/10/large/iphone-18-pro-max-2tb-den_1788994688.jpg",
                    @"Đỉnh cao công nghệ Apple với iPhone 18 Pro Max 2TB. Vi xử lý A20 Pro tiến trình 2nm tối tân, camera zoom tiềm vọng 10x quang học, thời lượng pin lên đến 35 giờ liên tục.

--- THÔNG SỐ KỸ THUẬT ---
• Màn hình: Super Retina XDR OLED 6.9 inch, 2868 x 1320 pixels, ProMotion 120Hz, 3000 nits
• Vi xử lý (CPU): Apple A20 Pro 2nm siêu mạnh mẽ, GPU 6 nhân Ray Tracing
• RAM & Bộ nhớ: 12GB RAM, Bộ nhớ trong 2TB NVMe siêu tốc
• Camera sau: Cụm 3 camera 48MP Fusion OIS + 48MP Siêu rộng + 48MP Tiềm vọng 10x quang
• Camera trước: 12MP TrueDepth f/1.9, Autofocus, Cinematic Mode 4K
• Pin & Sạc: Xem video 35 giờ, sạc nhanh 50% trong 25 phút, MagSafe 25W
• Nút điều khiển: Nút Camera Control cảm biến lực + Nút Action Button đa chức năng
• Kết nối: 5G đa băng tần, Wi-Fi 7, Bluetooth 5.4, USB-C 10Gbps
• Hệ điều hành: iOS 18 tích hợp Apple Intelligence"),

                new(1, 1, "iPhone 18 Pro Max 1TB | Chính hãng Việt Nam", 54000000, 61490000, 8,
                    "https://24hstore.vn/images/products/2026/09/10/large/iphone-18-pro-max-1tb-bac_1788994673.jpg",
                    @"iPhone 18 Pro Max 1TB màu Bạc Ánh Kim, khung titan thế hệ mới nhất, màn hình siêu sáng 3000 nits hiển thị rõ nét dưới trời nắng gắt.

--- THÔNG SỐ KỸ THUẬT ---
• Màn hình: Super Retina XDR OLED 6.9 inch 120Hz ProMotion
• Vi xử lý: Apple A20 Pro 2nm
• RAM & Bộ nhớ: 12GB RAM, 1TB ROM
• Camera: 48MP + 48MP + 48MP Zoom 10x
• Pin: Xem video 35 giờ, sạc Type-C USB 3"),

                new(1, 1, "iPhone 18 Pro Max 512GB | Chính hãng Việt Nam", 42000000, 48490000, 12,
                    "https://24hstore.vn/images/products/2026/09/10/large/iphone-18-pro-max-512gb-bang_1788994658.jpg",
                    @"iPhone 18 Pro Max 512GB màu Băng Tuyết sang trọng. Trợ giá thu cũ đổi mới lên đến 2.5 triệu đồng, hỗ trợ trả góp 0% tại KittyPhone.

--- THÔNG SỐ KỸ THUẬT ---
• Màn hình: 6.9 inch OLED ProMotion 120Hz
• Vi xử lý: Apple A20 Pro 2nm
• RAM & Bộ nhớ: 12GB RAM, 512GB ROM
• Camera: 48MP Fusion OIS + 48MP Ultra Wide + 48MP Tele 10x
• Pin: 35 giờ sử dụng, sạc nhanh MagSafe 25W"),

                new(1, 1, "iPhone 18 Pro Max 256GB | Chính hãng Việt Nam", 36000000, 41990000, 15,
                    "https://24hstore.vn/images/products/2026/09/10/large/iphone-18-pro-max-do_1788994643.jpg",
                    @"iPhone 18 Pro Max 256GB màu Đỏ Ruby quyền lực, camera điện toán cao cấp, nút Camera Control cảm ứng trượt siêu nhạy.

--- THÔNG SỐ KỸ THUẬT ---
• Màn hình: 6.9 inch Super Retina XDR OLED 120Hz
• Vi xử lý: Apple A20 Pro 2nm
• RAM & Bộ nhớ: 12GB RAM, 256GB ROM
• Camera: 48MP + 48MP + 48MP Zoom 10x quang
• Hệ điều hành: iOS 18"),

                new(1, 1, "iPhone 18 Pro 256GB | Chính hãng Việt Nam", 33000000, 38490000, 14,
                    "https://24hstore.vn/images/products/2026/09/10/large/iphone-18-pro-256gb-do_1788994377.jpg",
                    @"iPhone 18 Pro 256GB nhỏ gọn vừa tay, sức mạnh Pro vô đối với chip A20 Pro, màn hình ProMotion 120Hz viền siêu mỏng.

--- THÔNG SỐ KỸ THUẬT ---
• Màn hình: Super Retina XDR OLED 6.3 inch, 120Hz ProMotion, 3000 nits
• Vi xử lý (CPU): Apple A20 Pro 2nm, GPU 6 nhân
• RAM & Bộ nhớ: 12GB RAM, 256GB ROM
• Camera sau: 48MP Fusion + 48MP Ultra Wide + 48MP Telephoto 5x
• Pin & Sạc: Xem video 29 giờ, sạc nhanh USB-C, sạc không dây MagSafe
• Hệ điều hành: iOS 18"),

                // iPhone 17 & Air
                new(1, 1, "iPhone 17 Pro Max 256GB | Chính hãng Việt Nam", 28000000, 33990000, 16,
                    "https://24hstore.vn/images/products/2025/09/10/large/iphone-17-pro-max-cam_1757461075.jpg",
                    @"iPhone 17 Pro Max 256GB phiên bản màu Cam Sa Mạc ấn tượng. Viền màn hình mỏng kỷ lục, chip A19 Pro tiết kiệm điện năng vượt trội.

--- THÔNG SỐ KỸ THUẬT ---
• Màn hình: Super Retina XDR OLED 6.9 inch, ProMotion 120Hz, Dynamic Island
• Vi xử lý (CPU): Apple A19 Pro 3nm
• RAM & Bộ nhớ: 8GB RAM, 256GB ROM
• Camera sau: 48MP chính + 48MP góc siêu rộng + 12MP Telephoto 5x
• Pin & Sạc: Xem video 33 giờ, sạc nhanh Type-C"),

                new(1, 1, "iPhone 17 256GB | Chính hãng Việt Nam", 20500000, 24990000, 18,
                    "https://24hstore.vn/images/products/2025/09/10/large/iphone-17-xanh-la_1757461999.jpg",
                    @"iPhone 17 256GB màu Xanh Lá Pastel thời thượng, trang bị màn hình 120Hz ProMotion lần đầu xuất hiện trên bản tiêu chuẩn.

--- THÔNG SỐ KỸ THUẬT ---
• Màn hình: Super Retina XDR OLED 6.1 inch, ProMotion 120Hz
• Vi xử lý (CPU): Apple A19 Bionic 6 nhân
• RAM & Bộ nhớ: 8GB RAM, 256GB ROM
• Camera sau: Kép 48MP Fusion + 48MP Ultra Wide
• Pin & Sạc: Xem video 26 giờ, sạc nhanh Type-C"),

                new(1, 1, "iPhone Air 1TB | Chính hãng Việt Nam", 26000000, 31990000, 8,
                    "https://24hstore.vn/images/products/2025/09/10/large/iphone-air-den_1757462555.jpg",
                    @"iPhone Air siêu phẩm mỏng nhẹ chỉ 5.5mm, chất liệu titan siêu cứng, định nghĩa lại trải nghiệm cầm nắm thiết bị di động.

--- THÔNG SỐ KỸ THUẬT ---
• Màn hình: Super Retina XDR OLED 6.6 inch viền siêu mỏng, 120Hz
• Vi xử lý (CPU): Apple A19 Bionic
• RAM & Bộ nhớ: 8GB RAM, 1TB ROM
• Độ mỏng: Chỉ 5.5mm, trọng lượng 165g
• Camera: 48MP Fusion chất lượng cao"),

                // iPhone 16 Series
                new(1, 1, "iPhone 16 Pro Max 256GB | Chính hãng VN/A", 26500000, 31490000, 22,
                    "https://cdn2.cellphones.com.vn/insecure/rs:fill:358:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/iphone-16-pro-max.png",
                    @"iPhone 16 Pro Max 256GB Titan Sa Mạc - Phiên bản bán chạy nhất thị trường. Màn hình lớn 6.9 inch, chip A18 Pro tối tân, phím điều khiển Camera Control cảm ứng.

--- THÔNG SỐ KỸ THUẬT ---
• Màn hình: Super Retina XDR OLED 6.9 inch, 2868 x 1320 pixels, ProMotion 120Hz, 2000 nits
• Vi xử lý (CPU): Apple A18 Pro 6 nhân, GPU 6 nhân, Neural Engine 16 nhân
• RAM & Bộ nhớ: 8GB RAM, 256GB ROM NVMe
• Camera sau: Chính 48MP Fusion (f/1.78, OIS) + Góc rộng 48MP + Tele 12MP zoom 5x quang học
• Camera trước: TrueDepth 12MP f/1.9 autofocus
• Nút điều khiển: Phím Camera Control cảm biến lực + Action Button
• Pin & Sạc: Xem video 33 giờ, sạc nhanh 50% trong 30 phút, MagSafe 25W, USB-C 3 10Gbps
• Chuẩn kháng nước: IP68 (ngâm nước 6m trong 30 phút)
• Hệ điều hành: iOS 18 hỗ trợ Apple Intelligence tiếng Việt"),

                new(1, 1, "iPhone 16 Pro 128GB | Chính hãng VN/A", 22500000, 26990000, 15,
                    "https://24hstore.vn/images/products/2024/09/10/large/iphone-16-pro-titan-den_1725928702.jpg",
                    @"iPhone 16 Pro 128GB Titan Đen nhỏ gọn đẳng cấp Pro, viền siêu mỏng, camera zoom quang 5x tetraprism, chip A18 Pro hiệu năng vô địch.

--- THÔNG SỐ KỸ THUẬT ---
• Màn hình: Super Retina XDR OLED 6.3 inch, 2622 x 1206 pixels, ProMotion 120Hz
• Vi xử lý (CPU): Apple A18 Pro 6 nhân
• RAM & Bộ nhớ: 8GB RAM, 128GB ROM
• Camera sau: Chính 48MP + Góc rộng 48MP + Tele 12MP 5x quang học
• Nút chức năng: Camera Control + Action Button
• Pin & Sạc: Xem video 27 giờ, sạc nhanh Type-C"),

                new(1, 1, "iPhone 16 Plus 128GB | Chính hãng VN/A", 20500000, 24490000, 16,
                    "https://cdn2.cellphones.com.vn/insecure/rs:fill:358:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/iphone-16-plus-1.png",
                    @"iPhone 16 Plus 128GB màn hình 6.7 inch siêu lớn, thời lượng pin bền bỉ nhất dòng iPhone 16 tiêu chuẩn, camera kép 48MP sắc nét.

--- THÔNG SỐ KỸ THUẬT ---
• Màn hình: Super Retina XDR OLED 6.7 inch, 2796 x 1290 pixels, độ sáng 2000 nits
• Vi xử lý (CPU): Apple A18 6 nhân, GPU 5 nhân, Neural Engine 16 nhân
• RAM & Bộ nhớ: 8GB RAM, 128GB ROM
• Camera sau: Kép 48MP Fusion + 12MP Ultra Wide
• Pin & Sạc: Xem video 27 giờ, sạc nhanh Type-C"),

                new(1, 1, "iPhone 16 128GB | Chính hãng VN/A", 17900000, 21490000, 25,
                    "https://cdn2.cellphones.com.vn/insecure/rs:fill:358:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/iphone-16-1.png",
                    @"iPhone 16 128GB sắc màu Lưu Ly độc đáo, nút Camera Control và nút Action tiện lợi, camera đặt dọc hỗ trợ quay video không gian Spatial Video.

--- THÔNG SỐ KỸ THUẬT ---
• Màn hình: Super Retina XDR OLED 6.1 inch, Dynamic Island, 2000 nits
• Vi xử lý (CPU): Apple A18 6 nhân
• RAM & Bộ nhớ: 8GB RAM, 128GB ROM
• Camera sau: 48MP Fusion + 12MP Ultra Wide
• Pin & Sạc: Xem video 22 giờ, sạc nhanh Type-C"),

                // iPhone 15 & Older
                new(1, 1, "iPhone 15 Pro Max 256GB | Chính hãng VN/A", 23500000, 27990000, 18,
                    "https://24hstore.vn/images/products/2024/08/27/large/iphone-15-pro-max-titan-trang_1724723754.jpg",
                    @"iPhone 15 Pro Max 256GB Titan Trắng chính hãng VN/A. Khung viền Titan hàng không vũ trụ siêu nhẹ, cổng USB-C 10Gbps, chip A17 Pro chơi game đồ họa đỉnh cao.

--- THÔNG SỐ KỸ THUẬT ---
• Màn hình: Super Retina XDR OLED 6.7 inch, 120Hz ProMotion, Always-On Display
• Vi xử lý (CPU): Apple A17 Pro 3nm, GPU 6 nhân hỗ trợ Ray Tracing
• RAM & Bộ nhớ: 8GB RAM, 256GB ROM
• Camera sau: 48MP chính OIS + 12MP siêu rộng + 12MP Tele 5x quang học
• Cổng sạc: USB-C chuẩn USB 3 (10Gbps)
• Pin: Xem video 29 giờ"),

                new(1, 1, "iPhone 15 128GB | Chính hãng VN/A", 15000000, 17990000, 24,
                    "https://cdn2.cellphones.com.vn/insecure/rs:fill:358:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/iphone-15-plus_1_.png",
                    @"iPhone 15 128GB Dynamic Island tiện ích, camera 48MP nâng cấp vượt trội, mặt lưng kính pha màu nhám mịn, cổng sạc Type-C thế hệ mới.

--- THÔNG SỐ KỸ THUẬT ---
• Màn hình: Super Retina XDR OLED 6.1 inch, Dynamic Island, 2000 nits
• Vi xử lý (CPU): Apple A16 Bionic 6 nhân
• RAM & Bộ nhớ: 6GB RAM, 128GB ROM
• Camera sau: Chính 48MP f/1.6 + Góc siêu rộng 12MP
• Cổng sạc: USB-C tiện lợi đa năng"),

                new(1, 1, "iPhone 14 Pro Max 128GB | Cũ Like New", 15500000, 18490000, 12,
                    "https://cdn2.cellphones.com.vn/insecure/rs:fill:358:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/iphone-14-pro-max.png",
                    @"iPhone 14 Pro Max 128GB Tím Deep Purple đẹp 99%, nguyên zin 100%, bảo hành 1 đổi 1 trong 30 ngày tại hệ thống KittyPhone.

--- THÔNG SỐ KỸ THUẬT ---
• Màn hình: Super Retina XDR OLED 6.7 inch, ProMotion 120Hz, Dynamic Island
• Vi xử lý (CPU): Apple A16 Bionic 6 nhân
• RAM & Bộ nhớ: 6GB RAM, 128GB ROM
• Camera sau: 48MP chính + 12MP góc rộng + 12MP Tele 3x
• Tình trạng: Máy đẹp 99% nguyên bản chính hãng VN/A"),

                new(1, 1, "iPhone 13 128GB | Chính hãng VN/A", 10000000, 12490000, 20,
                    "https://cdn2.cellphones.com.vn/insecure/rs:fill:358:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/iphone-13.png",
                    @"iPhone 13 128GB giá tốt nhất phân khúc, pin trâu, hiệu năng chip A15 Bionic vẫn cực kỳ mạnh mẽ cho mọi tác vụ và chơi game ổn định.

--- THÔNG SỐ KỸ THUẬT ---
• Màn hình: Super Retina XDR OLED 6.1 inch, độ sáng 1200 nits
• Vi xử lý (CPU): Apple A15 Bionic 6 nhân
• RAM & Bộ nhớ: 4GB RAM, 128GB ROM
• Camera sau: Cụm camera chéo 12MP kép, chống rung cảm biến Sensor-Shift
• Pin: Xem video 19 giờ, sạc nhanh 20W"),

                new(1, 1, "iPhone 11 64GB | Chính hãng VN/A", 6800000, 8490000, 15,
                    "https://cdn2.cellphones.com.vn/insecure/rs:fill:358:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/iphone-11.png",
                    @"iPhone 11 64GB - smartphone quốc dân giá rẻ dưới 10 triệu, màn hình 6.1 inch, camera góc siêu rộng, độ bền cao và pin khỏe cả ngày dài.

--- THÔNG SỐ KỸ THUẬT ---
• Màn hình: Liquid Retina IPS LCD 6.1 inch
• Vi xử lý (CPU): Apple A13 Bionic 6 nhân
• RAM & Bộ nhớ: 4GB RAM, 64GB ROM
• Camera sau: Kép 12MP góc rộng và góc siêu rộng, Night Mode
• Kháng nước: IP68"),

                // Android Smartphones
                new(2, 1, "Samsung Galaxy S25 Ultra 5G 256GB", 27000000, 31990000, 15,
                    "https://cdn2.cellphones.com.vn/insecure/rs:fill:358:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/s/a/samsung-galaxy-s24-ultra_1__1.png",
                    @"Flagship đỉnh cao Samsung Galaxy S25 Ultra trang bị chip Snapdragon 8 Elite, camera 200MP zoom 100x và bút S-Pen quyền năng.

--- THÔNG SỐ KỸ THUẬT ---
• Màn hình: Dynamic AMOLED 2X 6.9 inch, QHD+ (3120 x 1440), 120Hz thích ứng, 2600 nits
• Kính bảo vệ: Corning Gorilla Armor chống chói quang học 75%
• Vi xử lý (CPU): Snapdragon 8 Elite for Galaxy 3nm
• RAM & Bộ nhớ: 12GB RAM LPDDR5X, 256GB UFS 4.0
• Camera sau: 200MP OIS + 50MP Siêu rộng + 50MP Tiềm vọng 5x + 10MP Tele 3x (Zoom 100x)
• Bút cảm ứng: Bút S-Pen tích hợp trong thân máy độ trễ 2.8ms
• Pin & Sạc: 5000mAh, sạc nhanh 45W, sạc không dây 15W
• Trợ lý AI: Galaxy AI dịch trực tiếp cuộc gọi, khoanh vùng tìm kiếm"),

                new(2, 1, "Samsung Galaxy Z Fold 6 5G 256GB", 36000000, 41990000, 8,
                    "https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/s/a/samsung-galaxy-z-fold-6_2.jpg",
                    @"Smartphone màn hình gập cao cấp nhất, mỏng nhẹ vượt trội, bản lề Flex không nếp gấp, tính năng Galaxy AI thông minh.

--- THÔNG SỐ KỸ THUẬT ---
• Màn hình chính: Dynamic AMOLED 2X 7.6 inch, 120Hz, 2600 nits
• Màn hình phụ: Dynamic AMOLED 2X 6.3 inch, 120Hz
• Vi xử lý (CPU): Snapdragon 8 Gen 3 for Galaxy
• RAM & Bộ nhớ: 12GB RAM, 256GB ROM
• Chuẩn kháng nước: IP48
• Pin: 4400mAh, sạc nhanh 25W"),

                new(2, 1, "Samsung Galaxy Z Flip 6 5G 256GB", 23000000, 26990000, 12,
                    "https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/f/r/frame_166_3.png",
                    @"Thiết kế gập vỏ sò sành điệu, màn hình phụ FlexWindow đa năng, kháng nước IP48, camera 50MP sắc nét và tản nhiệt buồng hơi.

--- THÔNG SỐ KỸ THUẬT ---
• Màn hình chính: Dynamic AMOLED 2X 6.7 inch 120Hz FHD+
• Màn hình phụ: Super AMOLED 3.4 inch FlexWindow
• Vi xử lý: Snapdragon 8 Gen 3 for Galaxy
• RAM & Bộ nhớ: 12GB RAM, 256GB ROM
• Camera: 50MP chính OIS + 12MP góc rộng
• Pin: 4000mAh"),

                new(2, 1, "Samsung Galaxy A55 5G 8GB/128GB", 7800000, 9490000, 25,
                    "https://cdn2.cellphones.com.vn/insecure/rs:fill:358:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/s/a/samsung-galaxy-a55_1.png",
                    @"Thiết kế kim loại cao cấp, màn hình Super AMOLED 120Hz, bảo mật Knox Vault, chống nước bụi IP67.

--- THÔNG SỐ KỸ THUẬT ---
• Màn hình: Super AMOLED 6.6 inch, FHD+, 120Hz, 1000 nits
• Vi xử lý (CPU): Exynos 1480 8 nhân kèm đồ họa AMD Xclipse 530
• RAM & Bộ nhớ: 8GB RAM, 128GB ROM
• Camera: 50MP OIS + 12MP siêu rộng + 5MP macro
• Pin: 5000mAh, sạc 25W, kháng nước IP67"),

                new(2, 1, "Samsung Galaxy A17 5G 8GB/128GB Cũ", 3200000, 4290000, 20,
                    "https://cdn2.cellphones.com.vn/insecure/rs:fill:358:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/s/a/samsung-galaxy-a55_1.png",
                    @"Điện thoại 5G giá siêu tốt phân khúc phổ thông, màn hình lớn 90Hz, pin trâu 5000mAh, bảo hành 1 đổi 1 trong 30 ngày tại KittyPhone.

--- THÔNG SỐ KỸ THUẬT ---
• Màn hình: Super AMOLED 6.5 inch, 90Hz FHD+
• Vi xử lý (CPU): MediaTek Dimensity 6100+ 5G
• RAM & Bộ nhớ: 8GB RAM, 128GB ROM
• Camera: 50MP chính + 5MP góc rộng
• Pin: 5000mAh, sạc nhanh 25W"),

                new(3, 1, "Xiaomi 14 Ultra 5G 16GB/512GB", 25500000, 29990000, 6,
                    "https://cdn2.cellphones.com.vn/insecure/rs:fill:358:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/x/i/xiaomi-14-ultra_1.png",
                    @"Hệ thống 4 camera Leica đỉnh cao cảm biến 1 inch Sony LYT-900, ống kính Summilux, chip Snapdragon 8 Gen 3 mạnh mẽ hàng đầu.

--- THÔNG SỐ KỸ THUẬT ---
• Màn hình: AMOLED 6.73 inch 2K (3200 x 1440), 120Hz LTPO, 3000 nits
• Vi xử lý (CPU): Qualcomm Snapdragon 8 Gen 3
• RAM & Bộ nhớ: 16GB RAM LPDDR5X, 512GB UFS 4.0
• Camera Leica: 4 camera 50MP cảm biến 1 inch, khẩu độ biến thiên f/1.63 - f/4.0
• Pin & Sạc: 5000mAh, sạc 90W có dây, 80W không dây"),

                new(3, 1, "Xiaomi 14T Pro 5G 12GB/512GB", 14200000, 16990000, 14,
                    "https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/x/i/xiaomi_14t_pro_1_.png",
                    @"Hiệu năng đột phá chip Dimensity 9300+, camera Leica chuyên nghiệp, sạc siêu tốc 120W HyperCharge đầy pin trong 19 phút.

--- THÔNG SỐ KỸ THUẬT ---
• Màn hình: AMOLED 6.67 inch 1.5K, 144Hz siêu mượt, 4000 nits
• Vi xử lý (CPU): MediaTek Dimensity 9300+ 4nm
• Camera Leica: Light Fusion 900 50MP + Tele 50MP + Góc rộng 12MP
• Pin & Sạc: 5000mAh, sạc nhanh HyperCharge 120W"),

                new(3, 1, "Xiaomi Redmi Note 13 Pro+ 5G", 7700000, 9290000, 18,
                    "https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/x/i/xiaomi-redmi-note-13-pro-4g_13__1.png",
                    @"Màn hình cong AMOLED 1.5K 120Hz, camera 200MP chống rung OIS, chuẩn kháng nước IP68 cao cấp.

--- THÔNG SỐ KỸ THUẬT ---
• Màn hình: Cong AMOLED 6.67 inch 1.5K, 120Hz
• Vi xử lý: MediaTek Dimensity 7200-Ultra
• Camera: 200MP cảm biến Samsung HP3 OIS
• Pin: 5000mAh, sạc nhanh 120W, kháng nước IP68"),

                new(4, 1, "OPPO Find X7 Ultra 5G 16GB/256GB", 21000000, 24990000, 5,
                    "https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/e/d/eda006276802c.jpg",
                    @"Hệ thống camera kép tiềm vọng đầu tiên trên thế giới kết hợp Hasselblad, màn hình 2K LTPO 120Hz siêu nét.

--- THÔNG SỐ KỸ THUẬT ---
• Màn hình: LTPO AMOLED 6.82 inch 2K, 120Hz, 4500 nits
• Vi xử lý (CPU): Snapdragon 8 Gen 3
• Camera: 4 camera 50MP kép tiềm vọng zoom 3x và 6x
• Pin: 5000mAh, sạc 100W SUPERVOOC"),

                new(4, 1, "OPPO Reno12 Pro 5G 12GB/512GB", 14800000, 17490000, 12,
                    "https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/d/i/dien-thoai-oppo-reno12-5g-_en-1_1_1.png",
                    @"Thiết kế thời thượng mỏng nhẹ, tính năng chụp ảnh chân dung AI chuyên sâu, kết nối AI LinkBoost siêu mạnh.

--- THÔNG SỐ KỸ THUẬT ---
• Màn hình: Cong vô cực AMOLED 6.7 inch 120Hz
• Vi xử lý: Dimensity 7300-Energy
• Camera: 50MP Sony LYT-600 OIS + 50MP Tele chân dung
• Pin: 5000mAh, sạc nhanh 80W"),

                new(5, 1, "Vivo X100 Pro 5G 16GB/512GB", 20000000, 23990000, 6,
                    "https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/v/i/vivo-x100.jpg",
                    @"Cụm camera ống kính ZEISS APO danh tiếng, cảm biến 1 inch, chip xử lý hình ảnh Vivo V3 độc quyền.

--- THÔNG SỐ KỸ THUẬT ---
• Màn hình: LTPO AMOLED 6.78 inch, 1.5K, 120Hz
• Vi xử lý (CPU): MediaTek Dimensity 9300
• Camera ZEISS: 50MP 1 inch + 50MP Telephoto ZEISS APO
• Pin: 5400mAh, sạc nhanh 100W"),

                new(5, 1, "Vivo V30 5G 12GB/512GB", 10200000, 12490000, 15,
                    "https://cdn2.cellphones.com.vn/358x358,webp,q100/media/catalog/product/d/i/dien-thoai-vivo-v30-5g_1_.png",
                    @"Vòng sáng Aura 2.0 chụp đêm thần sầu, thân máy siêu mỏng chỉ 7.45mm, màn hình 1.5K 120Hz.

--- THÔNG SỐ KỸ THUẬT ---
• Màn hình: AMOLED cong 6.78 inch, 1.5K, 120Hz
• Vi xử lý: Snapdragon 7 Gen 3
• Camera: Kép 50MP True Color + đèn vòng Aura Light 3.0
• Pin: 5000mAh, sạc nhanh 80W"),

                // Tablets
                new(1, 2, "iPad Pro M4 11 inch Wifi 256GB | Chính hãng Apple VN/A", 23800000, 27990000, 10,
                    "https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/f/r/frame_100_1_2__2_2.png",
                    @"Đỉnh cao công nghệ máy tính bảng với chip Apple M4 siêu tốc độ, màn hình OLED 2 lớp Ultra Retina XDR, thiết kế siêu mỏng chỉ 5.3mm.

--- THÔNG SỐ KỸ THUẬT ---
• Màn hình: Ultra Retina XDR OLED 2 lớp Tandem OLED 11 inch, 2420 x 1668, 120Hz ProMotion
• Vi xử lý (CPU): Apple M4 9 nhân, GPU 10 nhân
• RAM & Bộ nhớ: 8GB RAM, 256GB ROM
• Độ mỏng: Chỉ 5.3mm - mỏng nhất lịch sử Apple
• Hỗ trợ: Apple Pencil Pro, Magic Keyboard thế hệ mới"),

                new(1, 2, "iPad Air M2 11 inch Wifi 128GB | Chính hãng Apple VN/A", 13900000, 16490000, 12,
                    "https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/ipad-air-5.png",
                    @"Hiệu năng chuẩn Pro nhờ vi xử lý Apple M2, hỗ trợ Apple Pencil Pro với tính năng xoay thân bút và phản hồi rung.

--- THÔNG SỐ KỸ THUẬT ---
• Màn hình: Liquid Retina IPS LCD 11 inch, 2360 x 1640
• Vi xử lý (CPU): Apple M2 8 nhân, GPU 9 nhân
• RAM & Bộ nhớ: 8GB RAM, 128GB ROM
• Hỗ trợ: Apple Pencil Pro, Magic Keyboard"),

                new(1, 2, "iPad mini 7 Wifi 128GB | Chính hãng Apple", 11800000, 13990000, 14,
                    "https://cdn2.cellphones.com.vn/insecure/rs:fill:358:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/ipad-mini-6_2.png",
                    @"Kích thước nhỏ gọn 8.3 inch cầm vừa tay, chip A17 Pro mạnh mẽ hỗ trợ Apple Intelligence, kết nối Wifi 6E tốc độ cao.

--- THÔNG SỐ KỸ THUẬT ---
• Màn hình: Liquid Retina 8.3 inch, True Tone, dải màu P3
• Vi xử lý (CPU): Apple A17 Pro
• Kết nối: Wi-Fi 6E, Bluetooth 5.3, USB-C 10Gbps
• Hỗ trợ: Apple Pencil Pro"),

                new(1, 2, "iPad A16 11 inch 2025 Wifi 128GB | Chính hãng Apple", 9900000, 11890000, 15,
                    "https://cdn2.cellphones.com.vn/insecure/rs:fill:358:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/ipad-10-9-inch-2022_1.png",
                    @"iPad thế hệ 2025 trang bị chip Apple A16 Bionic mạnh mẽ, màn hình Liquid Retina 11 inch sắc nét, hỗ trợ Apple Pencil và Magic Keyboard.

--- THÔNG SỐ KỸ THUẬT ---
• Màn hình: Liquid Retina 11 inch viền mỏng đều 4 cạnh
• Vi xử lý (CPU): Apple A16 Bionic 6 nhân
• RAM & Bộ nhớ: 6GB RAM, 128GB ROM
• Hỗ trợ: Apple Pencil, Magic Keyboard Folio"),

                new(2, 2, "Samsung Galaxy Tab S10 Ultra 5G 256GB", 28500000, 33990000, 6,
                    "https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/s/s/ss-tab-s9-ultra_1_.png",
                    @"Màn hình khổng lồ 14.6 inch Dynamic AMOLED 2X chống chói Anti-Reflection, kháng nước IP68, kèm sẵn bút S-Pen cao cấp.

--- THÔNG SỐ KỸ THUẬT ---
• Màn hình: Dynamic AMOLED 2X 14.6 inch, 120Hz, kính Anti-Reflection
• Vi xử lý: MediaTek Dimensity 9300+ 4nm
• Bút cảm ứng: S-Pen kèm sẵn trong hộp, kháng nước IP68
• Pin & Sạc: 11200mAh, sạc nhanh 45W"),

                new(2, 2, "Samsung Galaxy Tab A11 WIFI 4GB/64GB Chính hãng", 2400000, 3090000, 25,
                    "https://cdn2.cellphones.com.vn/insecure/rs:fill:358:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/s/a/samsung-galaxy-tab-a9_1.png",
                    @"Máy tính bảng giải trí và học tập online giá tốt nhất, màn hình 8.7 inch, loa kép Dolby Atmos sống động, pin 5100mAh bền bỉ.

--- THÔNG SỐ KỸ THUẬT ---
• Màn hình: LCD 8.7 inch sắc nét
• Vi xử lý: 8 nhân Helio G99
• Âm thanh: Hệ thống loa kép Dolby Atmos
• Pin: 5100mAh"),

                new(3, 2, "Xiaomi Pad 6 6GB/128GB Chính Hãng", 5600000, 6990000, 18,
                    "https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/m/i/mi-pad-6-cps-doc-quyen-xanh.jpg",
                    @"Màn hình 2.8K 144Hz siêu mượt mà, chip Snapdragon 870 hiệu năng mạnh mẽ, pin 8840mAh sạc nhanh 33W.

--- THÔNG SỐ KỸ THUẬT ---
• Màn hình: IPS LCD 11 inch 2.8K (2880 x 1800), 144Hz
• Vi xử lý (CPU): Snapdragon 870 5G
• Âm thanh: 4 loa vòm Dolby Atmos
• Pin & Sạc: 8840mAh, sạc 33W"),

                new(3, 2, "Xiaomi Redmi Pad 2 Wifi 9.7 inch 4GB/64GB Chính Hãng", 2900000, 3690000, 20,
                    "https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/x/i/xiaomi-redmi-pad-se_1_3.png",
                    @"Màn hình độ phân giải FHD+ tần số quét 90Hz, 4 loa âm thanh vòm Dolby Atmos, vỏ kim loại nguyên khối sang trọng.

--- THÔNG SỐ KỸ THUẬT ---
• Màn hình: IPS 9.7 inch FHD+, 90Hz
• Vi xử lý: Snapdragon 680 8 nhân
• Vỏ máy: Kim loại nguyên khối
• Pin: 8000mAh"),

                // Smartwatches
                new(1, 4, "Apple Watch Series 10 42mm GPS Viền Nhôm", 8800000, 10490000, 15,
                    "https://cdn2.cellphones.com.vn/x/media/wysiwyg/Watch/Apple/Series-10/apple-watch-series-10-8.jpg",
                    @"Màn hình OLED góc rộng sáng hơn 40%, thân máy mỏng nhất lịch sử Apple Watch, đo độ sâu dưới nước và nhiệt độ nước.

--- THÔNG SỐ KỸ THUẬT ---
• Màn hình: Retina LTPO3 OLED góc rộng Always-On Display
• Độ mỏng: Chỉ 9.7mm
• Cảm biến: ECG điện tâm đồ, SpO2 oxy máu, đo nhiệt độ cổ tay, phát hiện ngưng thở khi ngủ
• Kháng nước: 50m, đo độ sâu 6m và nhiệt độ nước
• Pin: 18 giờ, sạc 80% trong 30 phút"),

                new(1, 4, "Apple Watch Ultra 2 Titanium 49mm Dây Alpine", 18200000, 21490000, 8,
                    "https://cdn2.cellphones.com.vn/x/media/wysiwyg/Watch/Apple/Ultra/apple-watch-ultra-2-6_1.jpg",
                    @"Đồng hồ thể thao chuyên nghiệp vỏ titan chuẩn quân đội, GPS băng tần kép chuẩn xác, thời lượng pin 72 giờ ở chế độ tiết kiệm.

--- THÔNG SỐ KỸ THUẬT ---
• Màn hình: Sapphire Crystal phẳng 49mm, 3000 nits
• Chất liệu: Titanium chuẩn hàng không vũ trụ
• GPS: Băng tần kép L1 và L5
• Kháng nước: 100m, chuẩn lặn EN13319 lặn sâu 40m
• Pin: 36 - 72 giờ"),

                new(1, 4, "Apple Watch SE 3 44mm GPS - Viền nhôm, Dây Đeo Thể Thao", 5300000, 6590000, 20,
                    "https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/a/p/apple-watch-se-2023-40mm.png",
                    @"Smartwatch bán chạy nhất của Apple với đầy đủ tính năng đo nhịp tim, phát hiện té ngã va chạm, theo dõi giấc ngủ, kháng nước 50m.

--- THÔNG SỐ KỸ THUẬT ---
• Màn hình: Retina OLED Always-On
• Vi xử lý: Apple S8 SiP 64-bit 2 nhân
• Tính năng an toàn: Phát hiện va chạm xe, té ngã, SOS
• Kháng nước: 50m"),

                new(2, 4, "Samsung Galaxy Watch 7 40mm BT Chính hãng", 4800000, 5990000, 18,
                    "https://cdn2.cellphones.com.vn/x/media/catalog/product/s/a/samsung-galaxy-watch-7-44mm_1_2_1.png",
                    @"Cảm biến BioActive thế hệ mới theo dõi chỉ số sức khỏe và giấc ngủ chính xác, chip Exynos W1000 3nm siêu mượt mà.

--- THÔNG SỐ KỸ THUẬT ---
• Màn hình: Super AMOLED 1.3 inch Sapphire Crystal
• Vi xử lý: Exynos W1000 3nm
• Cảm biến: BioActive thế hệ mới theo dõi chỉ số AGEs, phân tích giấc ngủ
• Trợ lý AI: Phân tích thể chất và chỉ số Energy Score"),

                // Accessories
                new(1, 3, "Tai nghe Bluetooth Apple AirPods 4 Chống Ồn Chủ Động (ANC)", 3900000, 4790000, 25,
                    "https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/a/p/apple-airpods-4-chong-on-chu-dong-thumb.png",
                    @"Thế hệ tai nghe AirPods 4 mới nhất tích hợp chip H2, khử tiếng ồn chủ động ANC, âm thanh không gian cá nhân hóa, sạc không dây và cổng USB-C.

--- THÔNG SỐ KỸ THUẬT ---
• Chip âm thanh: Apple H2
• Công nghệ: Chống ồn chủ động ANC, Spatial Audio
• Kháng nước/bụi: IP54 cho cả tai nghe và hộp sạc
• Cổng sạc: USB-C và sạc không dây Qi
• Thời lượng pin: Lên đến 30 giờ kèm hộp sạc"),

                new(1, 3, "Tai nghe Bluetooth Apple AirPods Pro 2 MagSafe USB-C", 4500000, 5490000, 30,
                    "https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/a/p/apple-airpods-pro-2-usb-c_1_.png",
                    @"Chống ồn chủ động vượt trội gấp 2 lần, tính năng nhận biết cuộc trò chuyện thông minh, hỗ trợ kiểm tra thính lực và hộp sạc có loa định vị.

--- THÔNG SỐ KỸ THUẬT ---
• Chip âm thanh: Apple H2 + Chip định vị U1
• Công nghệ: Chống ồn ANC vượt trội gấp 2x, Adaptive Audio
• Kháng nước/bụi: IP54
• Cổng sạc: USB-C, MagSafe, sạc qua đế sạc Apple Watch
• Pin: 6 giờ nghe liên tục (30 giờ kèm hộp sạc)"),

                new(1, 3, "Tai nghe chụp tai Apple AirPods Max USB-C", 11800000, 13990000, 8,
                    "https://cdn2.cellphones.com.vn/x/media/catalog/product/a/p/apple-airpods-max-1_1.png",
                    @"Tai nghe Over-ear cao cấp với âm thanh trung thực tuyệt đỉnh, đệm tai thoáng khí, khung thép không gỉ sang trọng.

--- THÔNG SỐ KỸ THUẬT ---
• Màng loa: Dynamic 40mm do Apple tự thiết kế
• Chống ồn: ANC Pro với 8 microphone
• Cổng kết nối: USB-C hỗ trợ âm thanh Lossless
• Pin: 20 giờ nghe nhạc liên tục khi bật ANC"),

                new(6, 3, "Tai nghe Bluetooth Baseus Bowie M2+ ANC Chống Ồn 42dB", 580000, 850000, 40,
                    "/images/products/baseus-bowie-m2.svg",
                    @"Khử ồn chủ động Hybrid ANC sâu đến -42dB, độ trễ siêu thấp 0.038s chơi game, Bluetooth 5.2 kết nối ổn định 2 thiết bị cùng lúc.

--- THÔNG SỐ KỸ THUẬT ---
• Chống ồn: Hybrid ANC đến -42dB
• Độ trễ: 0.038s chế độ Game Mode
• Kết nối: Bluetooth 5.2 kết nối 2 thiết bị
• Pin: 30 giờ kèm hộp sạc"),

                new(6, 3, "Tai nghe không dây Baseus Bowie WM02 Siêu Nhỏ Gọn", 220000, 350000, 50,
                    "https://cdn2.cellphones.com.vn/x/media/catalog/product/t/a/tai-nghe-bluetooth-baseus-bowie-wm02-01.png",
                    @"Thiết kế dạng viên thuốc con nhộng trong suốt cực xinh, pin trâu đến 25 giờ, âm bass chắc khỏe, Bluetooth 5.3.

--- THÔNG SỐ KỸ THUẬT ---
• Thiết kế: Dạng viên nhộng trong suốt mini
• Kết nối: Bluetooth 5.3 ổn định
• Pin: 25 giờ sử dụng liên tục kèm case"),

                new(7, 3, "Tai nghe không dây Bluetooth Havit TW967", 180000, 290000, 60,
                    "https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/f/r/frame_4_2.png",
                    @"Tai nghe True Wireless quốc dân giá siêu rẻ, kết nối Bluetooth 5.3 mượt mà, mic đàm thoại rõ ràng, cổng sạc Type-C.

--- THÔNG SỐ KỸ THUẬT ---
• Kết nối: Bluetooth 5.3
• Màng loa: Dynamic 10mm bass mạnh mẽ
• Cổng sạc: Type-C sạc nhanh"),

                new(1, 3, "Bút cảm ứng Apple Pencil Pro Chính Hãng VN/A", 2800000, 3490000, 20,
                    "https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/a/p/apple-pencil-pro-36.png",
                    @"Thế hệ Apple Pencil Pro với cử chỉ bóp Barrel Roll, phản hồi xúc giác Haptic, tính năng lướt Apple Pencil Hover và tìm kiếm Find My.

--- THÔNG SỐ KỸ THUẬT ---
• Cử chỉ: Bóp thân bút (Squeeze), Xoay thân bút (Barrel Roll)
• Phản hồi: Haptic feedback rung phản hồi
• Tính năng: Apple Pencil Hover, hỗ trợ tìm qua Find My
• Tương thích: iPad Pro M4, iPad Air M2"),

                new(1, 3, "Bàn phím Apple Magic Keyboard cho iPad Pro 11 inch", 6600000, 7990000, 10,
                    "/images/products/apple-magic-keyboard.svg",
                    @"Thiết kế lơ lửng cantilever thanh lịch, dãy phím chức năng 14 phím, bàn di chuột xúc giác bằng kính cỡ lớn phủ nhôm cao cấp.

--- THÔNG SỐ KỸ THUẬT ---
• Thiết kế: Lơ lửng cantilever góc nghiêng tùy chỉnh
• Bàn phím: 14 phím chức năng, phím cắt kéo hành trình 1mm
• Trackpad: Bàn di chuột kính lớn phủ nhôm
• Cổng: USB-C sạc chuyển tiếp tiện lợi"),

                new(1, 3, "Củ sạc nhanh Apple 20W USB-C Power Adapter Chính Hãng", 380000, 520000, 50,
                    "/images/products/apple-20w-adapter.svg",
                    @"Củ sạc chuẩn Apple Power Delivery 20W sạc 50% pin cho iPhone trong 30 phút, bảo vệ an toàn chống quá nhiệt đoản mạch.

--- THÔNG SỐ KỸ THUẬT ---
• Công suất: 20W Power Delivery (PD 3.0)
• Cổng ra: USB-C
• Tương thích: Toàn bộ iPhone, iPad, Apple Watch, AirPods"),

                new(1, 3, "Cáp sạc Apple Type-C to Type-C 60W Dây Dù 1m", 380000, 520000, 50,
                    "/images/products/apple-typec-cable.svg",
                    @"Cáp bện dù siêu bền chính hãng Apple, sạc nhanh công suất đến 60W và đồng bộ dữ liệu tốc độ cao.

--- THÔNG SỐ KỸ THUẬT ---
• Chiều dài: 1 mét
• Chất liệu: Dây bện dù chống gãy rối
• Công suất sạc: Hỗ trợ lên đến 60W"),

                new(1, 3, "Ốp lưng iPhone 16 Pro Max MagSafe Trong Suốt Chính Hãng", 1100000, 1420000, 30,
                    "/images/products/iphone-magsafe-case.svg",
                    @"Chất liệu polycarbonate trong suốt chống ố vàng, nam châm tích hợp hít sạc MagSafe chuẩn xác, bề mặt chống trầy xước.

--- THÔNG SỐ KỸ THUẬT ---
• Chất liệu: Polycarbonate quang học chống ố vàng
• Tính năng: Vòng nam châm MagSafe hít sạc chuẩn xác
• Tương thích: iPhone 16 Pro Max")
            };
        }
    }
}
