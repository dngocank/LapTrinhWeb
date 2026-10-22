using Microsoft.Data.SqlClient;
using BTL_PTTKYC.Models;

namespace BTL_PTTKYC.Data
{
    public class ReviewRepository
    {
        private readonly string _connectionString;
        public ReviewRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public List<ReviewModel> GetReviewsByProductId(int productId)
        {
            var list = new List<ReviewModel>();
            using var conn = new SqlConnection(_connectionString);
            var sql = @"
                SELECT r.review_id, r.product_id, p.product_name, r.customer_name, r.phone, r.rating, r.comment, r.reply, r.created_at,
                       r.customer_id, r.invoice_id, ISNULL(r.is_verified_purchase, 1) AS is_verified_purchase
                FROM Review r
                JOIN Product p ON p.product_id = r.product_id
                WHERE r.product_id = @productId
                ORDER BY r.created_at DESC";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@productId", productId);
            conn.Open();
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new ReviewModel
                {
                    ReviewId = Convert.ToInt32(r["review_id"]),
                    ProductId = Convert.ToInt32(r["product_id"]),
                    ProductName = r["product_name"].ToString()!,
                    CustomerName = r["customer_name"].ToString()!,
                    Phone = r["phone"] != DBNull.Value ? r["phone"].ToString() : null,
                    Rating = Convert.ToInt32(r["rating"]),
                    Comment = r["comment"].ToString()!,
                    Reply = r["reply"] != DBNull.Value ? r["reply"].ToString() : null,
                    CustomerId = r["customer_id"] != DBNull.Value ? Convert.ToInt32(r["customer_id"]) : null,
                    InvoiceId = r["invoice_id"] != DBNull.Value ? Convert.ToInt32(r["invoice_id"]) : null,
                    IsVerifiedPurchase = Convert.ToBoolean(r["is_verified_purchase"]),
                    CreatedAt = Convert.ToDateTime(r["created_at"])
                });
            }
            return list;
        }

        public List<ReviewModel> GetAllReviews(int? productId = null, int? rating = null)
        {
            var list = new List<ReviewModel>();
            using var conn = new SqlConnection(_connectionString);
            var sql = @"
                SELECT r.review_id, r.product_id, p.product_name, r.customer_name, r.phone, r.rating, r.comment, r.reply, r.created_at,
                       r.customer_id, r.invoice_id, ISNULL(r.is_verified_purchase, 1) AS is_verified_purchase
                FROM Review r
                JOIN Product p ON p.product_id = r.product_id
                WHERE (@productId IS NULL OR r.product_id = @productId)
                  AND (@rating IS NULL OR r.rating = @rating)
                ORDER BY r.created_at DESC";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@productId", (object?)productId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@rating", (object?)rating ?? DBNull.Value);
            conn.Open();
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new ReviewModel
                {
                    ReviewId = Convert.ToInt32(r["review_id"]),
                    ProductId = Convert.ToInt32(r["product_id"]),
                    ProductName = r["product_name"].ToString()!,
                    CustomerName = r["customer_name"].ToString()!,
                    Phone = r["phone"] != DBNull.Value ? r["phone"].ToString() : null,
                    Rating = Convert.ToInt32(r["rating"]),
                    Comment = r["comment"].ToString()!,
                    Reply = r["reply"] != DBNull.Value ? r["reply"].ToString() : null,
                    CustomerId = r["customer_id"] != DBNull.Value ? Convert.ToInt32(r["customer_id"]) : null,
                    InvoiceId = r["invoice_id"] != DBNull.Value ? Convert.ToInt32(r["invoice_id"]) : null,
                    IsVerifiedPurchase = Convert.ToBoolean(r["is_verified_purchase"]),
                    CreatedAt = Convert.ToDateTime(r["created_at"])
                });
            }
            return list;
        }

        public bool CheckCustomerPurchasedProduct(int customerId, int productId, out int matchedInvoiceId)
        {
            matchedInvoiceId = 0;
            using var conn = new SqlConnection(_connectionString);
            var sql = @"
                SELECT TOP 1 i.invoice_id
                FROM Invoice i
                JOIN Invoice_Detail id ON i.invoice_id = id.invoice_id
                WHERE i.customer_id = @custId 
                  AND id.product_id = @prodId
                  AND ISNULL(i.status, '') NOT IN ('CANCELLED', 'RETURNED')
                ORDER BY i.invoice_id DESC";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@custId", customerId);
            cmd.Parameters.AddWithValue("@prodId", productId);
            conn.Open();

            var obj = cmd.ExecuteScalar();
            if (obj != null && obj != DBNull.Value)
            {
                matchedInvoiceId = Convert.ToInt32(obj);
                return true;
            }
            return false;
        }

        public bool AddVerifiedReview(ReviewModel model, int customerId, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (!CheckCustomerPurchasedProduct(customerId, model.ProductId, out int invoiceId))
            {
                errorMessage = "Bạn chưa từng mua sản phẩm này tại KittyPhone. Để chống spam, chỉ khách hàng đã mua sản phẩm mới có quyền đánh giá.";
                return false;
            }

            using var conn = new SqlConnection(_connectionString);
            var sql = @"
                INSERT INTO Review (product_id, customer_name, phone, rating, comment, customer_id, invoice_id, is_verified_purchase, created_at)
                VALUES (@productId, @customerName, @phone, @rating, @comment, @custId, @invId, 1, GETDATE())";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@productId", model.ProductId);
            cmd.Parameters.AddWithValue("@customerName", model.CustomerName.Trim());
            cmd.Parameters.AddWithValue("@phone", (object?)model.Phone?.Trim() ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@rating", model.Rating);
            cmd.Parameters.AddWithValue("@comment", model.Comment.Trim());
            cmd.Parameters.AddWithValue("@custId", customerId);
            cmd.Parameters.AddWithValue("@invId", invoiceId);

            conn.Open();
            cmd.ExecuteNonQuery();
            return true;
        }

        public void AddReview(ReviewModel model)
        {
            using var conn = new SqlConnection(_connectionString);
            var sql = @"
                INSERT INTO Review (product_id, customer_name, phone, rating, comment, customer_id, invoice_id, is_verified_purchase, created_at)
                VALUES (@productId, @customerName, @phone, @rating, @comment, @custId, @invId, @isVerified, GETDATE())";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@productId", model.ProductId);
            cmd.Parameters.AddWithValue("@customerName", model.CustomerName);
            cmd.Parameters.AddWithValue("@phone", (object?)model.Phone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@rating", model.Rating);
            cmd.Parameters.AddWithValue("@comment", model.Comment);
            cmd.Parameters.AddWithValue("@custId", (object?)model.CustomerId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@invId", (object?)model.InvoiceId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@isVerified", model.IsVerifiedPurchase ? 1 : 0);

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public void ReplyReview(int reviewId, string reply)
        {
            using var conn = new SqlConnection(_connectionString);
            var sql = "UPDATE Review SET reply = @reply WHERE review_id = @reviewId";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@reviewId", reviewId);
            cmd.Parameters.AddWithValue("@reply", reply);
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public void DeleteReview(int reviewId)
        {
            using var conn = new SqlConnection(_connectionString);
            var sql = "DELETE FROM Review WHERE review_id = @reviewId";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@reviewId", reviewId);
            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }
}
