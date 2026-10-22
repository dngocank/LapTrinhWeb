using Microsoft.Data.SqlClient;
using BTL_PTTKYC.Models;
using System.Data;

namespace BTL_PTTKYC.Data
{
    public class WarehouseRepository
    {
        private readonly string _connectionString;
        public WarehouseRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public List<ImportReceiptModel> GetAllImports()
        {
            var list = new List<ImportReceiptModel>();
            using var conn = new SqlConnection(_connectionString);
            var sql = @"
                SELECT r.import_id, r.supplier_id, s.supplier_name, s.phone AS supplier_phone,
                       r.employee_id, e.employee_name, r.import_date, r.total_amount
                FROM Import_Receipt r
                JOIN Supplier s ON s.supplier_id = r.supplier_id
                JOIN Employee e ON e.employee_id = r.employee_id
                ORDER BY r.import_id DESC";

            using var cmd = new SqlCommand(sql, conn);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new ImportReceiptModel
                {
                    ImportId = Convert.ToInt32(reader["import_id"]),
                    SupplierId = Convert.ToInt32(reader["supplier_id"]),
                    SupplierName = reader["supplier_name"].ToString()!,
                    SupplierPhone = reader["supplier_phone"] != DBNull.Value ? reader["supplier_phone"].ToString() : null,
                    EmployeeId = Convert.ToInt32(reader["employee_id"]),
                    EmployeeName = reader["employee_name"].ToString()!,
                    ImportDate = Convert.ToDateTime(reader["import_date"]),
                    TotalAmount = Convert.ToDecimal(reader["total_amount"])
                });
            }
            return list;
        }

        public ImportReceiptModel? GetImportById(int importId)
        {
            ImportReceiptModel? receipt = null;
            using var conn = new SqlConnection(_connectionString);
            var sql = @"
                SELECT r.import_id, r.supplier_id, s.supplier_name, s.phone AS supplier_phone,
                       r.employee_id, e.employee_name, r.import_date, r.total_amount
                FROM Import_Receipt r
                JOIN Supplier s ON s.supplier_id = r.supplier_id
                JOIN Employee e ON e.employee_id = r.employee_id
                WHERE r.import_id = @id";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", importId);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                receipt = new ImportReceiptModel
                {
                    ImportId = Convert.ToInt32(reader["import_id"]),
                    SupplierId = Convert.ToInt32(reader["supplier_id"]),
                    SupplierName = reader["supplier_name"].ToString()!,
                    SupplierPhone = reader["supplier_phone"] != DBNull.Value ? reader["supplier_phone"].ToString() : null,
                    EmployeeId = Convert.ToInt32(reader["employee_id"]),
                    EmployeeName = reader["employee_name"].ToString()!,
                    ImportDate = Convert.ToDateTime(reader["import_date"]),
                    TotalAmount = Convert.ToDecimal(reader["total_amount"])
                };
            }
            reader.Close();

            if (receipt != null)
            {
                var detailSql = @"
                    SELECT d.import_detail_id, d.import_id, d.product_id, p.product_name, d.quantity, d.import_price, d.line_total
                    FROM Import_Detail d
                    JOIN Product p ON p.product_id = d.product_id
                    WHERE d.import_id = @id";

                using var detailCmd = new SqlCommand(detailSql, conn);
                detailCmd.Parameters.AddWithValue("@id", importId);
                using var detailReader = detailCmd.ExecuteReader();
                while (detailReader.Read())
                {
                    receipt.Details.Add(new ImportDetailModel
                    {
                        ImportDetailId = Convert.ToInt32(detailReader["import_detail_id"]),
                        ImportId = Convert.ToInt32(detailReader["import_id"]),
                        ProductId = Convert.ToInt32(detailReader["product_id"]),
                        ProductName = detailReader["product_name"].ToString()!,
                        Quantity = Convert.ToInt32(detailReader["quantity"]),
                        ImportPrice = Convert.ToDecimal(detailReader["import_price"]),
                        LineTotal = Convert.ToDecimal(detailReader["line_total"])
                    });
                }
            }

            return receipt;
        }

        public int CreateImportReceipt(int supplierId, int employeeId, List<ImportDetailItemInput> items, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (items == null || items.Count == 0)
            {
                errorMessage = "Phiếu nhập phải có ít nhất 1 sản phẩm.";
                return 0;
            }

            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var trans = conn.BeginTransaction();

            try
            {
                int importId = 0;
                var insertHeader = @"
                    INSERT INTO Import_Receipt (supplier_id, employee_id, import_date, total_amount)
                    VALUES (@supplierId, @empId, CAST(GETDATE() AS DATE), 0);
                    SELECT SCOPE_IDENTITY();";

                using (var cmd = new SqlCommand(insertHeader, conn, trans))
                {
                    cmd.Parameters.AddWithValue("@supplierId", supplierId);
                    cmd.Parameters.AddWithValue("@empId", employeeId);
                    importId = Convert.ToInt32(cmd.ExecuteScalar());
                }

                decimal calculatedTotal = 0;
                foreach (var item in items)
                {
                    if (item.Quantity <= 0)
                    {
                        trans.Rollback();
                        errorMessage = "Số lượng nhập của sản phẩm phải lớn hơn 0.";
                        return 0;
                    }

                    if (item.ImportPrice <= 0)
                    {
                        trans.Rollback();
                        errorMessage = "Đơn giá nhập của sản phẩm phải lớn hơn 0đ (không cho phép nhập 0đ).";
                        return 0;
                    }

                    calculatedTotal += item.Quantity * item.ImportPrice;

                    // Trigger trg_ImportDetail_AfterInsert will automatically add quantity to Product and recalculate total_amount
                    var insertDetail = @"
                        INSERT INTO Import_Detail (import_id, product_id, quantity, import_price)
                        VALUES (@importId, @productId, @qty, @price);";

                    using (var cmd = new SqlCommand(insertDetail, conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@importId", importId);
                        cmd.Parameters.AddWithValue("@productId", item.ProductId);
                        cmd.Parameters.AddWithValue("@qty", item.Quantity);
                        cmd.Parameters.AddWithValue("@price", item.ImportPrice);
                        cmd.ExecuteNonQuery();
                    }
                }

                if (calculatedTotal <= 0)
                {
                    trans.Rollback();
                    errorMessage = "Tổng giá trị phiếu nhập không thể bằng 0đ.";
                    return 0;
                }

                trans.Commit();
                return importId;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                errorMessage = ex.Message;
                return 0;
            }
        }

        public List<SupplierOptionModel> GetAllSuppliers()
        {
            var list = new List<SupplierOptionModel>();
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("SELECT supplier_id, supplier_name, phone, address FROM Supplier ORDER BY supplier_name", conn);
            conn.Open();
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new SupplierOptionModel
                {
                    SupplierId = Convert.ToInt32(r["supplier_id"]),
                    SupplierName = r["supplier_name"].ToString()!,
                    Phone = r["phone"] != DBNull.Value ? r["phone"].ToString() : null,
                    Address = r["address"] != DBNull.Value ? r["address"].ToString() : null
                });
            }
            return list;
        }

        public List<EmployeeOptionModel> GetWarehouseEmployees()
        {
            var list = new List<EmployeeOptionModel>();
            using var conn = new SqlConnection(_connectionString);
            var sql = "SELECT employee_id, employee_name, position, phone FROM Employee WHERE position IN (N'Warehouse Staff', N'Manager') ORDER BY employee_name";
            using var cmd = new SqlCommand(sql, conn);
            conn.Open();
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new EmployeeOptionModel
                {
                    EmployeeId = Convert.ToInt32(r["employee_id"]),
                    EmployeeName = r["employee_name"].ToString()!,
                    Position = r["position"] != DBNull.Value ? r["position"].ToString() : null,
                    Phone = r["phone"] != DBNull.Value ? r["phone"].ToString() : null
                });
            }
            return list;
        }

        public List<StockMovementModel> GetStockMovements()
        {
            var list = new List<StockMovementModel>();
            using var conn = new SqlConnection(_connectionString);
            var sql = @"
                SELECT TOP 100 m.movement_id, m.product_id, p.product_name, m.movement_type, m.quantity_change, m.reference_id, m.movement_date, m.note
                FROM Stock_Movement m
                JOIN Product p ON p.product_id = m.product_id
                ORDER BY m.movement_id DESC";

            using var cmd = new SqlCommand(sql, conn);
            conn.Open();
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new StockMovementModel
                {
                    MovementId = Convert.ToInt32(r["movement_id"]),
                    ProductId = Convert.ToInt32(r["product_id"]),
                    ProductName = r["product_name"].ToString()!,
                    MovementType = r["movement_type"].ToString()!,
                    QuantityChange = Convert.ToInt32(r["quantity_change"]),
                    ReferenceId = r["reference_id"] != DBNull.Value ? Convert.ToInt32(r["reference_id"]) : null,
                    MovementDate = Convert.ToDateTime(r["movement_date"]),
                    Note = r["note"] != DBNull.Value ? r["note"].ToString() : null
                });
            }
            return list;
        }

        public List<ProductSerialModel> GetSerials()
        {
            var list = new List<ProductSerialModel>();
            using var conn = new SqlConnection(_connectionString);
            var sql = @"
                SELECT s.serial_id, s.product_id, p.product_name, s.imei_number, s.serial_number, s.status, s.warehouse_id, w.warehouse_name
                FROM Product_Serial s
                JOIN Product p ON p.product_id = s.product_id
                JOIN Warehouse w ON w.warehouse_id = s.warehouse_id
                ORDER BY s.serial_id DESC";

            using var cmd = new SqlCommand(sql, conn);
            conn.Open();
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new ProductSerialModel
                {
                    SerialId = Convert.ToInt32(r["serial_id"]),
                    ProductId = Convert.ToInt32(r["product_id"]),
                    ProductName = r["product_name"].ToString()!,
                    ImeiNumber = r["imei_number"] != DBNull.Value ? r["imei_number"].ToString() : null,
                    SerialNumber = r["serial_number"] != DBNull.Value ? r["serial_number"].ToString() : null,
                    Status = r["status"].ToString()!,
                    WarehouseId = Convert.ToInt32(r["warehouse_id"]),
                    WarehouseName = r["warehouse_name"].ToString()!
                });
            }
            return list;
        }

        public bool AddSerial(int productId, string imei, string serial, int warehouseId, out string errorMessage)
        {
            errorMessage = string.Empty;
            try
            {
                using var conn = new SqlConnection(_connectionString);
                var sql = @"
                    INSERT INTO Product_Serial (product_id, imei_number, serial_number, status, warehouse_id)
                    VALUES (@pid, @imei, @serial, 'In Stock', @wid)";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@pid", productId);
                cmd.Parameters.AddWithValue("@imei", imei.Trim());
                cmd.Parameters.AddWithValue("@serial", serial.Trim());
                cmd.Parameters.AddWithValue("@wid", warehouseId);
                conn.Open();
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }
    }
}
