using MySql.Data.MySqlClient;
using System.Windows;

namespace ManagerAppV3._5
{
    public class MySQLTableChecker
    {
        private readonly string connectionString;

        public MySQLTableChecker(string connectionString)
        {
            this.connectionString = connectionString;
        }
        public bool AreRequiredTablesPresent()
        {
            // Список обязательных таблиц
            List<string> requiredTables = new List<string> { "product price", "roles", "users", "warehouses" };
            List<string> existingTables = new List<string>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    // Получаем список всех таблиц в текущей базе данных
                    string query = "SHOW TABLES;";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            existingTables.Add(reader.GetString(0));
                        }
                    }
                }
                // Проверяем наличие каждой обязательной таблицы
                foreach (var table in requiredTables)
                {
                    if (!existingTables.Contains(table))
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при проверке таблиц: " + ex.Message);
                return false;
            }
        }
    }
}
