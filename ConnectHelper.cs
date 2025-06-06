using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Windows;

namespace ManagerAppV2
{
    public class ConnectHelper
    {

        // Конструктор класса, который инициализирует подключение к базе данных
        public string GetDbName(string login)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(GetConnectionString()))
                {
                    connection.Open();
                    string query = $"SELECT databasename FROM users where login = '{login}';";
                    MySqlCommand command = new MySqlCommand(query, connection);

                    object dbname = command.ExecuteScalar();
                    return dbname.ToString();

                }
            }catch (MySqlException ex)
            {
                MessageBox.Show("Произошла ошибка: " + ex);
                return "";
            }

        }
        // Метод для получения строки подключения
        public string GetConnectionString()
        {

            return "server=localhost;port=3306;user=root;password=password;database=database;";
        }
        
        public string ManagerData(string DBName)
        {
            string Query = "SELECT " +
                            "id," +
                            "DATE_FORMAT(ShipmentDate, '%d.%m.%Y') as \"Дата отправки\"," +
                            "ShipmentWarehouse as \"Склад отправки\"," +
                            "ClientCity as \"Город покупателя\"," +
                            "ClientName as \"Покупатель\"," +
                            "ProductName as \"Товар\"," +
                            "ProductAmount as \"Количество\"," +
                            "`UnitOfMeasurement` as `Ед Изм`," +
                            "Price as \"Цена менеджера\"," +
                            "MinimumPrice as \"Мин цена\"," +
                            "ShipmentValue as \"Итого менеджера\"," +
                            "`ShipmentValue(Minimum_price)` as \"Итого (Мин)\"," +
                            "UPDNumber as \"Номер УПД\"," +
                            "ShipmentPrice as \"Стоимость доставки\"," +
                            $"Reward as Премия FROM `{DBName}`;";
            return Query;
        }
        public string GetRole(string tableName)
        {
            // Проверка входного параметра
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentException("Имя таблицы не может быть пустым", nameof(tableName));
            }

            string role = null;

            try
            {
                using (var con = new MySqlConnection(GetConnectionString()))
                {
                    // Используем параметризованный запрос для безопасности
                    string query = "SELECT role FROM users WHERE databasename = @TableName LIMIT 1";

                    using (var cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@TableName", tableName);

                        con.Open();
                        var result = cmd.ExecuteScalar();

                        // Безопасное преобразование результата
                        role = result?.ToString();
                    }
                }
            }
            catch (MySqlException ex)
            {
                // Логирование ошибки
                Debug.WriteLine($"Ошибка при получении роли: {ex.Message}");
                throw new ApplicationException("Ошибка доступа к базе данных", ex);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Неожиданная ошибка: {ex.Message}");
                throw;
            }

            // Возвращаем null если роль не найдена (вместо исключения)
            return role;
        }
    }
}