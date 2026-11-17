using System.Data.SqlClient;

namespace RegionForecastSystem
{
    public static class DatabaseConnection
    {
        private static readonly string ConnectionString =
            "Server=localhost;Database=RegionForecastDB;Trusted_Connection=True;TrustServerCertificate=True;";

        public static SqlConnection GetConnection()
        {
            SqlConnection connection = new SqlConnection(ConnectionString);
            connection.Open();
            return connection;
        }
    }
}