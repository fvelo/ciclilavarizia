using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Models.Models;

namespace DataAccessLayer
{
    public static class DbSecureServiceExtention
    {
        public static IServiceCollection AddDbSecure(this IServiceCollection services, string ccnString)
        {
            DbSecure dbSecure = new(ccnString);
            services.AddSingleton(dbSecure);
            return services;
        }
    }

    public class DbSecure
    {
        private readonly SqlConnection _connection = new();
        private readonly SqlCommand _command = new();
        public bool IsDbOnline = false;
        private string sqlCcnString = string.Empty;

        /// <summary>
        /// You need to pass the ConnectionString to the Db;
        /// </summary>
        /// <param name="cnnString"></param>
        public DbSecure(string cnnString)
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
            finally { _connection.Close(); }
        }

        public bool FindUserByCustomerId(long customerId)
        {
            bool result = false;
            Credentials credentials = new();
            try
            {
                using(SqlConnection connection = new(sqlCcnString))
                {
                    connection.Open();
                    using(SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                                                  SELECT *
                                                  FROM [dbo].[Credentials]
                                                  WHERE [CustomerId] = @CustomerId;
                                              ";

                        command.Parameters.AddWithValue("@CustomerId", customerId);

                        using(SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // this be necessaty in the real version of the login, so I already started with it.
                                //long credentialId = -1, customerIdRead = -1;
                                //string emailAddress = "vuoto", passwordHash = "vuoto", passwordSalt = "vuoto";
                                //if (reader["CredentialId"] is not DBNull) credentialId = Convert.ToInt64(reader["CredentialId"]);
                                //if (reader["CustomerId"] is not DBNull) customerIdRead = Convert.ToInt64(reader["CustomerId"]);
                                //if (reader["EmailAddress"] is not DBNull) emailAddress = reader["EmailAddress"].ToString()!;
                                //if (reader["PasswordHash"] is not DBNull) passwordHash = reader["PasswordHash"].ToString()!;
                                //if (reader["PasswordSalt"] is not DBNull) passwordSalt = reader["PasswordSalt"].ToString()!;

                                //credentials.CredentialId = credentialId;
                                //credentials.CustomerId = customerIdRead;
                                //credentials.EmailAddress = emailAddress;
                                //credentials.PasswordHash = passwordHash;
                                //credentials.PasswordSalt = passwordSalt;
                                result = true; // I mean if it enter here there is a result, am I wrong??
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

        public async Task<bool> FindUserByEmail(string email)
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
                                                  WHERE LOWER([EmailAddress]) LIKE @EmailAddress;
                                              ";

                        email = email.Trim().Replace(" ", "").ToLower();


                        command.Parameters.AddWithValue("@EmailAddress", email);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (reader.Read())
                            {
                                // this be necessaty in the real version of the login, so I already started with it.
                                //long credentialId = -1, customerIdRead = -1;
                                //string emailAddress = "vuoto", passwordHash = "vuoto", passwordSalt = "vuoto";
                                //if (reader["CredentialId"] is not DBNull) credentialId = Convert.ToInt64(reader["CredentialId"]);
                                //if (reader["CustomerId"] is not DBNull) customerIdRead = Convert.ToInt64(reader["CustomerId"]);
                                //if (reader["EmailAddress"] is not DBNull) emailAddress = reader["EmailAddress"].ToString()!;
                                //if (reader["PasswordHash"] is not DBNull) passwordHash = reader["PasswordHash"].ToString()!;
                                //if (reader["PasswordSalt"] is not DBNull) passwordSalt = reader["PasswordSalt"].ToString()!;

                                //credentials.CredentialId = credentialId;
                                //credentials.CustomerId = customerIdRead;
                                //credentials.EmailAddress = emailAddress;
                                //credentials.PasswordHash = passwordHash;
                                //credentials.PasswordSalt = passwordSalt;
                                result = true; // I mean if it enter here there is a result, am I wrong??
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
    }
}
