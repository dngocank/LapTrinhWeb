using Microsoft.Data.SqlClient;
using BTL_PTTKYC.Models;

namespace BTL_PTTKYC.Data
{
    public class AccountRepository
    {
        private readonly string _connectionString;
        public AccountRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public AccountModel? Authenticate(string username, string password)
        {
            using var conn = new SqlConnection(_connectionString);
            var sql = @"
                SELECT a.account_id, a.employee_id, a.username, a.password_hash, a.role, a.is_active,
                       e.employee_name, e.position, e.phone
                FROM Account a
                JOIN Employee e ON e.employee_id = a.employee_id
                WHERE a.username = @username";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@username", username.Trim());

            conn.Open();
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var isActive = Convert.ToBoolean(reader["is_active"]);
                if (!isActive) return null; // Inactive account

                var hash = reader["password_hash"].ToString()!;
                bool verified = false;

                try
                {
                    verified = BCrypt.Net.BCrypt.Verify(password, hash);
                }
                catch
                {
                    // Fallback in case of raw password match
                    verified = (password == hash);
                }

                if (verified)
                {
                    return new AccountModel
                    {
                        AccountId = Convert.ToInt32(reader["account_id"]),
                        EmployeeId = Convert.ToInt32(reader["employee_id"]),
                        Username = reader["username"].ToString()!,
                        Role = reader["role"].ToString()!,
                        IsActive = isActive,
                        EmployeeName = reader["employee_name"].ToString()!,
                        Position = reader["position"] != DBNull.Value ? reader["position"].ToString() : null,
                        Phone = reader["phone"] != DBNull.Value ? reader["phone"].ToString() : null
                    };
                }
            }

            return null;
        }

        public List<AccountModel> GetAllAccounts()
        {
            var list = new List<AccountModel>();
            using var conn = new SqlConnection(_connectionString);
            var sql = @"
                SELECT a.account_id, a.employee_id, a.username, a.role, a.is_active,
                       e.employee_name, e.position, e.phone
                FROM Account a
                JOIN Employee e ON e.employee_id = a.employee_id
                ORDER BY a.account_id ASC";

            using var cmd = new SqlCommand(sql, conn);
            conn.Open();
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new AccountModel
                {
                    AccountId = Convert.ToInt32(r["account_id"]),
                    EmployeeId = Convert.ToInt32(r["employee_id"]),
                    Username = r["username"].ToString()!,
                    Role = r["role"].ToString()!,
                    IsActive = Convert.ToBoolean(r["is_active"]),
                    EmployeeName = r["employee_name"].ToString()!,
                    Position = r["position"] != DBNull.Value ? r["position"].ToString() : null,
                    Phone = r["phone"] != DBNull.Value ? r["phone"].ToString() : null
                });
            }
            return list;
        }

        public AccountModel? GetAccountById(int accountId)
        {
            using var conn = new SqlConnection(_connectionString);
            var sql = @"
                SELECT a.account_id, a.employee_id, a.username, a.role, a.is_active,
                       e.employee_name, e.position, e.phone
                FROM Account a
                JOIN Employee e ON e.employee_id = a.employee_id
                WHERE a.account_id = @id";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", accountId);
            conn.Open();
            using var r = cmd.ExecuteReader();
            if (r.Read())
            {
                return new AccountModel
                {
                    AccountId = Convert.ToInt32(r["account_id"]),
                    EmployeeId = Convert.ToInt32(r["employee_id"]),
                    Username = r["username"].ToString()!,
                    Role = r["role"].ToString()!,
                    IsActive = Convert.ToBoolean(r["is_active"]),
                    EmployeeName = r["employee_name"].ToString()!,
                    Position = r["position"] != DBNull.Value ? r["position"].ToString() : null,
                    Phone = r["phone"] != DBNull.Value ? r["phone"].ToString() : null
                };
            }
            return null;
        }

        public bool CreateAccount(CreateAccountViewModel model, out string errorMessage)
        {
            errorMessage = string.Empty;
            try
            {
                using var conn = new SqlConnection(_connectionString);
                conn.Open();

                // Check username exists
                using (var checkCmd = new SqlCommand("SELECT COUNT(*) FROM Account WHERE username = @user", conn))
                {
                    checkCmd.Parameters.AddWithValue("@user", model.Username.Trim());
                    if (Convert.ToInt32(checkCmd.ExecuteScalar()) > 0)
                    {
                        errorMessage = "Tên đăng nhập đã tồn tại trong hệ thống.";
                        return false;
                    }
                }

                // Check employee already has an account
                using (var checkEmp = new SqlCommand("SELECT COUNT(*) FROM Account WHERE employee_id = @empId", conn))
                {
                    checkEmp.Parameters.AddWithValue("@empId", model.EmployeeId);
                    if (Convert.ToInt32(checkEmp.ExecuteScalar()) > 0)
                    {
                        errorMessage = "Nhân viên này đã có tài khoản trong hệ thống.";
                        return false;
                    }
                }

                var hash = BCrypt.Net.BCrypt.HashPassword(model.Password);
                var sql = @"
                    INSERT INTO Account (employee_id, username, password_hash, role, is_active)
                    VALUES (@empId, @user, @hash, @role, @isActive)";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@empId", model.EmployeeId);
                cmd.Parameters.AddWithValue("@user", model.Username.Trim());
                cmd.Parameters.AddWithValue("@hash", hash);
                cmd.Parameters.AddWithValue("@role", model.Role);
                cmd.Parameters.AddWithValue("@isActive", model.IsActive ? 1 : 0);

                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        public bool UpdateAccount(EditAccountViewModel model, out string errorMessage)
        {
            errorMessage = string.Empty;
            try
            {
                using var conn = new SqlConnection(_connectionString);
                conn.Open();

                string sql;
                if (!string.IsNullOrWhiteSpace(model.NewPassword))
                {
                    sql = @"
                        UPDATE Account
                        SET role = @role,
                            is_active = @isActive,
                            password_hash = @hash
                        WHERE account_id = @id";
                }
                else
                {
                    sql = @"
                        UPDATE Account
                        SET role = @role,
                            is_active = @isActive
                        WHERE account_id = @id";
                }

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", model.AccountId);
                cmd.Parameters.AddWithValue("@role", model.Role);
                cmd.Parameters.AddWithValue("@isActive", model.IsActive ? 1 : 0);

                if (!string.IsNullOrWhiteSpace(model.NewPassword))
                {
                    cmd.Parameters.AddWithValue("@hash", BCrypt.Net.BCrypt.HashPassword(model.NewPassword));
                }

                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        public List<EmployeeOptionModel> GetAllEmployees()
        {
            var list = new List<EmployeeOptionModel>();
            using var conn = new SqlConnection(_connectionString);
            var sql = @"
                SELECT e.employee_id, e.employee_name, e.position, e.phone,
                       CASE WHEN a.account_id IS NOT NULL THEN 1 ELSE 0 END AS has_account
                FROM Employee e
                LEFT JOIN Account a ON a.employee_id = e.employee_id
                ORDER BY e.employee_id ASC";

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
                    Phone = r["phone"] != DBNull.Value ? r["phone"].ToString() : null,
                    HasAccount = Convert.ToInt32(r["has_account"]) == 1
                });
            }
            return list;
        }
    }
}
