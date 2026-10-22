using System.ComponentModel.DataAnnotations;

namespace BTL_PTTKYC.Models
{
    public class ImportDetailItemInput
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal ImportPrice { get; set; }
    }

    public class CreateImportViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn nhà cung cấp")]
        public int SupplierId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn nhân viên kho")]
        public int EmployeeId { get; set; }

        public List<SupplierOptionModel> Suppliers { get; set; } = new();
        public List<EmployeeOptionModel> WarehouseEmployees { get; set; } = new();
        public List<ProductViewModel> AvailableProducts { get; set; } = new();

        public List<ImportDetailItemInput> Items { get; set; } = new();
    }

    public class SupplierOptionModel
    {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Address { get; set; }
    }

    public class ImportReceiptModel
    {
        public int ImportId { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string? SupplierPhone { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime ImportDate { get; set; }
        public decimal TotalAmount { get; set; }
        public List<ImportDetailModel> Details { get; set; } = new();
    }

    public class ImportDetailModel
    {
        public int ImportDetailId { get; set; }
        public int ImportId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal ImportPrice { get; set; }
        public decimal LineTotal { get; set; }
    }

    public class StockMovementModel
    {
        public int MovementId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string MovementType { get; set; } = string.Empty;
        public int QuantityChange { get; set; }
        public int? ReferenceId { get; set; }
        public DateTime MovementDate { get; set; }
        public string? Note { get; set; }
    }

    public class ProductSerialModel
    {
        public int SerialId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ImeiNumber { get; set; }
        public string? SerialNumber { get; set; }
        public string Status { get; set; } = "In Stock";
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
    }
}
