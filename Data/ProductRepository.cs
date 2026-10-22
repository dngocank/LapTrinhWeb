using Microsoft.Data.SqlClient;
using BTL_PTTKYC.Models;
using System.Data;

namespace BTL_PTTKYC.Data
{
    public class ProductRepository
    {
        private readonly string _connectionString;
        public ProductRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public List<ProductViewModel> GetAllProducts(string? keyword = null, int? brandId = null, int? categoryId = null, bool includeInactive = false, string? series = null, string? priceRange = null, string? sort = null)
        {
            var list = new List<ProductViewModel>();

            using (var conn = new SqlConnection(_connectionString))
            {
                var sql = @"
                    SELECT p.product_id, p.product_name, p.brand_id, b.brand_name, p.category_id, c.category_name, 
                           p.import_price, p.selling_price, p.quantity, p.is_active, p.image_url, p.description,
                           CASE 
                               WHEN p.quantity = 0 THEN 'Out of stock'
                               WHEN p.quantity <= 5 THEN 'Low stock'
                               ELSE 'In stock'
                           END AS stock_status,
                           ISNULL((SELECT AVG(CAST(r.rating AS FLOAT)) FROM Review r WHERE r.product_id = p.product_id), 5.0) AS avg_rating,
                           ISNULL((SELECT COUNT(*) FROM Review r WHERE r.product_id = p.product_id), 0) AS review_count
                    FROM Product p
                    JOIN Brand b ON p.brand_id = b.brand_id
                    JOIN Category c ON p.category_id = c.category_id
                    WHERE (@includeInactive = 1 OR p.is_active = 1)
                      AND (@keyword IS NULL OR p.product_name LIKE '%' + @keyword + '%')
                      AND (@brandId IS NULL OR p.brand_id = @brandId)
                      AND (@categoryId IS NULL OR p.category_id = @categoryId)
                      AND (@series IS NULL OR p.product_name LIKE '%' + @series + '%')
                      AND (
                           @priceRange IS NULL 
                           OR (@priceRange = 'lt10' AND p.selling_price < 10000000)
                           OR (@priceRange = '10to20' AND p.selling_price >= 10000000 AND p.selling_price <= 20000000)
                           OR (@priceRange = 'gt20' AND p.selling_price > 20000000)
                      )";

                switch (sort?.ToLower())
                {
                    case "asc":
                        sql += " ORDER BY p.selling_price ASC";
                        break;
                    case "desc":
                        sql += " ORDER BY p.selling_price DESC";
                        break;
                    case "discount":
                        sql += " ORDER BY p.selling_price DESC";
                        break;
                    case "installment":
                        sql += " ORDER BY p.selling_price ASC";
                        break;
                    case "hot":
                    default:
                        sql += " ORDER BY p.product_id DESC";
                        break;
                }

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@includeInactive", includeInactive ? 1 : 0);
                    cmd.Parameters.AddWithValue("@keyword", (object?)keyword ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@brandId", (object?)brandId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@categoryId", (object?)categoryId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@series", (object?)series ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@priceRange", (object?)priceRange ?? DBNull.Value);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new ProductViewModel
                            {
                                ProductId = Convert.ToInt32(reader["product_id"]),
                                BrandId = Convert.ToInt32(reader["brand_id"]),
                                CategoryId = Convert.ToInt32(reader["category_id"]),
                                ProductName = reader["product_name"].ToString()!,
                                BrandName = reader["brand_name"].ToString()!,
                                CategoryName = reader["category_name"].ToString()!,
                                ImportPrice = Convert.ToDecimal(reader["import_price"]),
                                SellingPrice = Convert.ToDecimal(reader["selling_price"]),
                                Quantity = Convert.ToInt32(reader["quantity"]),
                                IsActive = Convert.ToBoolean(reader["is_active"]),
                                StockStatus = reader["stock_status"].ToString()!,
                                ImageUrl = reader["image_url"] != DBNull.Value ? reader["image_url"].ToString() : null,
                                Description = reader["description"] != DBNull.Value ? reader["description"].ToString() : null,
                                AverageRating = Convert.ToDouble(reader["avg_rating"]),
                                ReviewCount = Convert.ToInt32(reader["review_count"])
                            });
                        }
                    }
                }
            }
            return list;
        }

        public ProductViewModel? GetProductById(int productId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var sql = @"
                    SELECT p.product_id, p.product_name, p.brand_id, b.brand_name, p.category_id, c.category_name, 
                           p.import_price, p.selling_price, p.quantity, p.is_active, p.image_url, p.description,
                           CASE 
                               WHEN p.quantity = 0 THEN 'Out of stock'
                               WHEN p.quantity <= 5 THEN 'Low stock'
                               ELSE 'In stock'
                           END AS stock_status,
                           ISNULL((SELECT AVG(CAST(r.rating AS FLOAT)) FROM Review r WHERE r.product_id = p.product_id), 5.0) AS avg_rating,
                           ISNULL((SELECT COUNT(*) FROM Review r WHERE r.product_id = p.product_id), 0) AS review_count
                    FROM Product p
                    JOIN Brand b ON p.brand_id = b.brand_id
                    JOIN Category c ON p.category_id = c.category_id
                    WHERE p.product_id = @productId";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@productId", productId);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new ProductViewModel
                            {
                                ProductId = Convert.ToInt32(reader["product_id"]),
                                BrandId = Convert.ToInt32(reader["brand_id"]),
                                CategoryId = Convert.ToInt32(reader["category_id"]),
                                ProductName = reader["product_name"].ToString()!,
                                BrandName = reader["brand_name"].ToString()!,
                                CategoryName = reader["category_name"].ToString()!,
                                ImportPrice = Convert.ToDecimal(reader["import_price"]),
                                SellingPrice = Convert.ToDecimal(reader["selling_price"]),
                                Quantity = Convert.ToInt32(reader["quantity"]),
                                IsActive = Convert.ToBoolean(reader["is_active"]),
                                StockStatus = reader["stock_status"].ToString()!,
                                ImageUrl = reader["image_url"] != DBNull.Value ? reader["image_url"].ToString() : null,
                                Description = reader["description"] != DBNull.Value ? reader["description"].ToString() : null,
                                AverageRating = Convert.ToDouble(reader["avg_rating"]),
                                ReviewCount = Convert.ToInt32(reader["review_count"])
                            };
                        }
                    }
                }
            }
            return null;
        }

        public List<(int id, string name)> GetBrands()
        {
            var list = new List<(int, string)>();
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("SELECT brand_id, brand_name FROM Brand ORDER BY brand_name", conn);
            conn.Open();
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add((Convert.ToInt32(r["brand_id"]), r["brand_name"].ToString()!));
            }
            return list;
        }

        public List<(int id, string name)> GetCategories()
        {
            var list = new List<(int, string)>();
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("SELECT category_id, category_name FROM Category ORDER BY category_name", conn);
            conn.Open();
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add((Convert.ToInt32(r["category_id"]), r["category_name"].ToString()!));
            }
            return list;
        }

        public int CreateProduct(ProductViewModel model)
        {
            using var conn = new SqlConnection(_connectionString);
            var sql = @"
                INSERT INTO Product (brand_id, category_id, product_name, import_price, selling_price, is_active, image_url, description)
                VALUES (@brandId, @categoryId, @productName, @importPrice, @sellingPrice, @isActive, @imageUrl, @description);
                SELECT SCOPE_IDENTITY();";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@brandId", model.BrandId);
            cmd.Parameters.AddWithValue("@categoryId", model.CategoryId);
            cmd.Parameters.AddWithValue("@productName", model.ProductName);
            cmd.Parameters.AddWithValue("@importPrice", model.ImportPrice);
            cmd.Parameters.AddWithValue("@sellingPrice", model.SellingPrice);
            cmd.Parameters.AddWithValue("@isActive", model.IsActive ? 1 : 0);
            cmd.Parameters.AddWithValue("@imageUrl", (object?)model.ImageUrl ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@description", (object?)model.Description ?? DBNull.Value);

            conn.Open();
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public void UpdateProduct(ProductViewModel model)
        {
            using var conn = new SqlConnection(_connectionString);
            var sql = @"
                UPDATE Product
                SET brand_id = @brandId,
                    category_id = @categoryId,
                    product_name = @productName,
                    import_price = @importPrice,
                    selling_price = @sellingPrice,
                    is_active = @isActive,
                    image_url = @imageUrl,
                    description = @description
                WHERE product_id = @productId";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@productId", model.ProductId);
            cmd.Parameters.AddWithValue("@brandId", model.BrandId);
            cmd.Parameters.AddWithValue("@categoryId", model.CategoryId);
            cmd.Parameters.AddWithValue("@productName", model.ProductName);
            cmd.Parameters.AddWithValue("@importPrice", model.ImportPrice);
            cmd.Parameters.AddWithValue("@sellingPrice", model.SellingPrice);
            cmd.Parameters.AddWithValue("@isActive", model.IsActive ? 1 : 0);
            cmd.Parameters.AddWithValue("@imageUrl", (object?)model.ImageUrl ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@description", (object?)model.Description ?? DBNull.Value);

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public void DeleteOrDeactivateProduct(int productId)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("usp_Product_Delete", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@product_id", productId);
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public List<ProductViewModel> GetLowStockProducts()
        {
            var list = new List<ProductViewModel>();
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("SELECT product_id, product_name, brand_name, quantity, import_price FROM vw_LowStockProducts", conn);
            conn.Open();
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new ProductViewModel
                {
                    ProductId = Convert.ToInt32(r["product_id"]),
                    ProductName = r["product_name"].ToString()!,
                    BrandName = r["brand_name"].ToString()!,
                    Quantity = Convert.ToInt32(r["quantity"]),
                    ImportPrice = Convert.ToDecimal(r["import_price"]),
                    StockStatus = "Low stock"
                });
            }
            return list;
        }
    }
}
