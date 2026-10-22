using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using BTL_PTTKYC.Models;

namespace BTL_PTTKYC.Data
{
    public class PromotionRepository
    {
        private readonly string _connectionString;

        public PromotionRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        public List<PromotionModel> GetActivePromotions()
        {
            var list = new List<PromotionModel>();
            using var conn = new SqlConnection(_connectionString);
            var sql = @"
                SELECT promotion_id, promotion_name, discount_percent, start_date, end_date, 
                       start_hour, end_hour, min_rank_id, description
                FROM Promotion
                WHERE end_date >= CAST(GETDATE() AS DATE)
                  AND promotion_name NOT LIKE '%Autumn%' 
                  AND promotion_name NOT LIKE N'%Mùa Thu%'
                ORDER BY start_date ASC, discount_percent DESC";

            using var cmd = new SqlCommand(sql, conn);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new PromotionModel
                {
                    PromotionId = Convert.ToInt32(reader["promotion_id"]),
                    PromotionName = reader["promotion_name"].ToString() ?? string.Empty,
                    DiscountPercent = Convert.ToDecimal(reader["discount_percent"]),
                    StartDate = Convert.ToDateTime(reader["start_date"]),
                    EndDate = Convert.ToDateTime(reader["end_date"]),
                    StartHour = reader["start_hour"] != DBNull.Value ? Convert.ToInt32(reader["start_hour"]) : null,
                    EndHour = reader["end_hour"] != DBNull.Value ? Convert.ToInt32(reader["end_hour"]) : null,
                    MinRankId = reader["min_rank_id"] != DBNull.Value ? Convert.ToInt32(reader["min_rank_id"]) : null,
                    Description = reader["description"] != DBNull.Value ? reader["description"].ToString() : null
                });
            }
            return list;
        }

        public List<PromotionModel> GetAllPromotions()
        {
            var list = new List<PromotionModel>();
            using var conn = new SqlConnection(_connectionString);
            var sql = @"
                SELECT promotion_id, promotion_name, discount_percent, start_date, end_date,
                       start_hour, end_hour, min_rank_id, description
                FROM Promotion
                ORDER BY end_date DESC, promotion_id DESC";

            using var cmd = new SqlCommand(sql, conn);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new PromotionModel
                {
                    PromotionId = Convert.ToInt32(reader["promotion_id"]),
                    PromotionName = reader["promotion_name"].ToString() ?? string.Empty,
                    DiscountPercent = Convert.ToDecimal(reader["discount_percent"]),
                    StartDate = Convert.ToDateTime(reader["start_date"]),
                    EndDate = Convert.ToDateTime(reader["end_date"]),
                    StartHour = reader["start_hour"] != DBNull.Value ? Convert.ToInt32(reader["start_hour"]) : null,
                    EndHour = reader["end_hour"] != DBNull.Value ? Convert.ToInt32(reader["end_hour"]) : null,
                    MinRankId = reader["min_rank_id"] != DBNull.Value ? Convert.ToInt32(reader["min_rank_id"]) : null,
                    Description = reader["description"] != DBNull.Value ? reader["description"].ToString() : null
                });
            }
            return list;
        }

        public PromotionModel? GetPromotionById(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            var sql = @"
                SELECT promotion_id, promotion_name, discount_percent, start_date, end_date,
                       start_hour, end_hour, min_rank_id, description
                FROM Promotion
                WHERE promotion_id = @id";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new PromotionModel
                {
                    PromotionId = Convert.ToInt32(reader["promotion_id"]),
                    PromotionName = reader["promotion_name"].ToString() ?? string.Empty,
                    DiscountPercent = Convert.ToDecimal(reader["discount_percent"]),
                    StartDate = Convert.ToDateTime(reader["start_date"]),
                    EndDate = Convert.ToDateTime(reader["end_date"]),
                    StartHour = reader["start_hour"] != DBNull.Value ? Convert.ToInt32(reader["start_hour"]) : null,
                    EndHour = reader["end_hour"] != DBNull.Value ? Convert.ToInt32(reader["end_hour"]) : null,
                    MinRankId = reader["min_rank_id"] != DBNull.Value ? Convert.ToInt32(reader["min_rank_id"]) : null,
                    Description = reader["description"] != DBNull.Value ? reader["description"].ToString() : null
                };
            }
            return null;
        }

        public bool CreatePromotion(PromotionModel model, out string errorMessage)
        {
            errorMessage = string.Empty;
            try
            {
                using var conn = new SqlConnection(_connectionString);
                var sql = @"
                    INSERT INTO Promotion (promotion_name, discount_percent, start_date, end_date, start_hour, end_hour, min_rank_id, description)
                    VALUES (@name, @percent, @start, @end, @startHour, @endHour, @minRank, @desc);
                    SELECT SCOPE_IDENTITY();";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@name", model.PromotionName.Trim());
                cmd.Parameters.AddWithValue("@percent", model.DiscountPercent);
                cmd.Parameters.AddWithValue("@start", model.StartDate.Date);
                cmd.Parameters.AddWithValue("@end", model.EndDate.Date);
                cmd.Parameters.AddWithValue("@startHour", (object?)model.StartHour ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@endHour", (object?)model.EndHour ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@minRank", (object?)model.MinRankId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@desc", (object?)model.Description ?? DBNull.Value);

                conn.Open();
                int id = Convert.ToInt32(cmd.ExecuteScalar());
                model.PromotionId = id;
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
