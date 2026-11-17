using System.Data;
using System.Data.SqlClient;

namespace RegionForecastSystem
{
    public static class DatabaseService
    {
        public static DataTable ExecuteQuery(string query, SqlParameter[] parameters = null)
        {
            using (var connection = DatabaseConnection.GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                if (parameters != null)
                    command.Parameters.AddRange(parameters);

                DataTable dt = new DataTable();
                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    adapter.Fill(dt);
                }
                return dt;
            }
        }

        public static void ExecuteNonQuery(string query, SqlParameter[] parameters = null)
        {
            using (var connection = DatabaseConnection.GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                if (parameters != null)
                    command.Parameters.AddRange(parameters);

                command.ExecuteNonQuery();
            }
        }

        public static object ExecuteScalar(string query, SqlParameter[] parameters = null)
        {
            using (var connection = DatabaseConnection.GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                if (parameters != null)
                    command.Parameters.AddRange(parameters);

                return command.ExecuteScalar();
            }
        }
    }
}