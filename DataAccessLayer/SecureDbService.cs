using CommonCiclilavarizia;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

namespace DataAccessLayer
{
    public static class DbSecureServiceExtention
    {
        public static IServiceCollection AddDbSecure(this IServiceCollection services, string ccnString)
        {
            SecureDbService secureDb = new(ccnString);
            services.AddSingleton(secureDb);
            return services;
        }
    }

    public class SecureDbService
    {
        private readonly SqlConnection _connection = new();
        private readonly SqlCommand _command = new();
        public bool IsDbOnline = false;
        private string sqlCcnString = string.Empty;

        /// <summary>
        /// You need to pass the ConnectionString to the Db;
        /// </summary>
        /// <param name="cnnString"></param>
        public SecureDbService(string cnnString)
        {
            try
            {
                _connection = new SqlConnection(cnnString);
                _connection.Open();
                IsDbOnline = true;
                sqlCcnString = cnnString;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
            finally
            {
                _connection.Close();
                IsDbOnline = false;
            }
        }

        public bool FindUserByCustomerId(long customerId)
        {
            bool result = false;
            Credentials credentials = new();
            try
            {
                using (SqlConnection connection = new(sqlCcnString))
                {
                    connection.Open();
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                                                  SELECT *
                                                  FROM [dbo].[Credentials]
                                                  WHERE [CustomerId] = @CustomerId;
                                              ";
                        command.Parameters.AddWithValue("@CustomerId", customerId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                result = true;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return result;
            }
            return result;
        }

        /// <summary>
        /// Checks if a credential record already exists for the specified email address.
        /// Performs a case-insensitive check and handles string sanitization.
        /// </summary>
        /// <param name="email">The email address to verify.</param>
        /// <returns>True if the email exists in the Credentials table; otherwise, false.</returns>
        public async Task<bool> DoesCredentialExistsByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;

            email = email.ToLower().Replace(" ", "");

            using (SqlConnection connection = new(sqlCcnString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                                                  IF EXISTS (SELECT 1 FROM [dbo].[Credentials]
                                                             WHERE LOWER([EmailAddress]) = @EmailAddress) 
                                                  SELECT 1 ELSE SELECT 0
                                              ";

                    command.Parameters.AddWithValue("@EmailAddress", email);
                    var result = await command.ExecuteScalarAsync();

                    return (int)(result ?? 0) == 1;
                }
            }
        }

        /// <summary>
        /// Retrieves the internal CustomerId associated with a specific email address from the Secure database.
        /// </summary>
        /// <param name="email">The email address to search for.</param>
        /// <returns>The CustomerId if found; otherwise, null.</returns>
        public async Task<int?> GetCustomerIdByEmailAddressAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;

            email = email.ToLower().Replace(" ", "");

            using (SqlConnection connection = new(sqlCcnString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"SELECT [CustomerId] FROM [dbo].[Credentials] WHERE LOWER([EmailAddress]) = @EmailAddress";
                    command.Parameters.AddWithValue("@EmailAddress", email);

                    var result = await command.ExecuteScalarAsync();

                    return result != DBNull.Value && result != null
                        ? Convert.ToInt32(result)
                        : null;
                }
            }
        }

        /// <summary>
        /// Search in SecureDb for the email by customerId.
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns>Returns EmailAddress if exists, otherwise empty.</returns>
        public async Task<string> GetEmailAddressByCustomerIdAsync(int customerId)
        {
            string emailAddress = string.Empty;
            using (SqlConnection connection = new(sqlCcnString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                                                  SELECT [EmailAddress]
                                                  FROM [dbo].[Credentials]
                                                  WHERE [CustomerId] = @CustomerId;
                                              ";

                    command.Parameters.AddWithValue("@CustomerId", customerId);

                    object result = await command.ExecuteScalarAsync();

                    if (result != null && result != DBNull.Value)
                    {
                        emailAddress = result.ToString();
                    }
                    return emailAddress;
                }
            }

        }

        public async Task<int> CreateCredentialAsync(Credentials incomingCredentials)
        {
            if (await DoesCredentialExistsByEmailAsync(incomingCredentials.EmailAddress))
            {
                return -1;
            }

            using (SqlConnection connection = new(sqlCcnString))
            {
                await connection.OpenAsync();

                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                                                INSERT INTO [dbo].[Credentials]
                                                           ([CustomerId]
                                                           ,[EmailAddress]
                                                           ,[PasswordHash]
                                                           ,[PasswordSalt]
                                                           ,[Role])
                                                     VALUES
                                                           (@CustomerId
                                                           ,@EmailAddress
                                                           ,@PasswordHash
                                                           ,@PasswordSalt
                                                           ,@Role);
                                                SELECT SCOPE_IDENTITY();
                                            ";

                    command.Parameters.AddWithValue("@CustomerId", incomingCredentials.CustomerId);
                    command.Parameters.AddWithValue("@EmailAddress", incomingCredentials.EmailAddress);
                    command.Parameters.AddWithValue("@PasswordHash", incomingCredentials.PasswordHash);
                    command.Parameters.AddWithValue("@PasswordSalt", incomingCredentials.PasswordSalt);
                    command.Parameters.AddWithValue("@Role", incomingCredentials.Role);

                    var result = await command.ExecuteScalarAsync();

                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToInt32(result);
                    }
                    return -1;
                }
            }
        }

        /// <summary>
        /// Retrieves the full credentials (including Hash and Salt) for a given email address.
        /// This is used during the login process to verify the password.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <returns>A Credentials object if found; otherwise, null.</returns>
        public async Task<Credentials?> GetCredentialsByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;

            // Sanitize input
            email = email.Trim().ToLower();

            using (SqlConnection connection = new(sqlCcnString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                                               SELECT [CustomerId], [EmailAddress], [PasswordHash], [PasswordSalt], [Role]
                                               FROM [dbo].[Credentials]
                                               WHERE LOWER([EmailAddress]) = @EmailAddress;
                                          ";
                    command.Parameters.AddWithValue("@EmailAddress", email);
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Credentials
                            {
                                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                                EmailAddress = reader.GetString(reader.GetOrdinal("EmailAddress")),
                                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                                PasswordSalt = reader.GetString(reader.GetOrdinal("PasswordSalt")),
                                Role = reader.GetString(reader.GetOrdinal("Role"))
                            };
                        }
                    }
                }
            }
            return null; // No user found
        }

        /// <summary>
        /// Permanently removes a credential record from the Secure DB.
        /// </summary>
        /// <param name="customerId">The ID of the customer to remove.</param>
        /// <returns>True if the operation completed successfully; otherwise, false.</returns>
        public async Task<bool> DeleteCredentialByCustomerIdAsync(int customerId)
        {
            try
            {
                using (SqlConnection connection = new(sqlCcnString))
                {
                    // Simple DELETE statement
                    string query = "";

                    using (SqlCommand command = new(query, connection))
                    {


                        // If no exception occurred, we assume success. 
                        // Even if 0 rows were affected (user didn't exist in secure DB), 
                        // it is considered "cleaned".
                    }

                    await connection.OpenAsync();
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                                               DELETE FROM [dbo].[Credentials] WHERE [CustomerId] = @CustomerId
                                            ";

                        command.Parameters.AddWithValue("@CustomerId", customerId);

                        await command.ExecuteNonQueryAsync();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Secure DB Delete Error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Updates the email address for a specific customer.
        /// Does not perform validation; expects a valid email string.
        /// </summary>
        public async Task<bool> UpdateEmailAsync(int customerId, string newEmail)
        {
            using (SqlConnection connection = new(sqlCcnString))
            {
                const string query = @"
                                        UPDATE [dbo].[Credentials]
                                        SET [EmailAddress] = @Email
                                        WHERE [CustomerId] = @CustomerId
                                     ";

                using (SqlCommand command = new(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", newEmail);
                    command.Parameters.AddWithValue("@CustomerId", customerId);

                    await connection.OpenAsync();
                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    return rowsAffected > 0;
                }
            }
        }

        /// <summary>
        /// Updates the password hash and salt for a specific customer. 
        /// Does not perform encryption; expects already hashed values.
        /// </summary>
        public async Task<bool> UpdatePasswordAsync(int customerId, string passwordHash, string passwordSalt)
        {
            using (SqlConnection connection = new(sqlCcnString))
            {
                const string query = @"
                                        UPDATE [dbo].[Credentials]
                                        SET [PasswordHash] = @Hash, 
                                            [PasswordSalt] = @Salt
                                        WHERE [CustomerId] = @CustomerId
                                    ";

                using (SqlCommand command = new(query, connection))
                {
                    command.Parameters.AddWithValue("@Hash", passwordHash);
                    command.Parameters.AddWithValue("@Salt", passwordSalt);
                    command.Parameters.AddWithValue("@CustomerId", customerId);

                    await connection.OpenAsync();
                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    return rowsAffected > 0;
                }
            }
        }
    }
}
