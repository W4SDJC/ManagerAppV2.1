using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;

namespace Kursovaya2
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
        public ObservableCollection<string> GetColumnData(string tableName, string columnName)
        {
            ObservableCollection<string> data = new ObservableCollection<string>();

            try
            {
                using (MySqlConnection connection = new MySqlConnection("server=localhost;port=3306;user=root;password=password;database=database;"))
                {
                    connection.Open();

                    string query = $"SELECT `{columnName}` FROM `{tableName}`"; // Обязательно используйте обратные кавычки для имен таблиц и столбцов
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Преобразуйте значение в строку и добавьте его в коллекцию
                                data.Add(reader.GetString(0));
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                // Обработка ошибок подключения или запроса к базе данных
                System.Windows.MessageBox.Show($"Ошибка подключения к базе данных: {ex.Message}");
                // Запись в лог:  лучше использовать более продвинутые методы логирования (например, NLog или Serilog)
            }

            return data;
        }
        // Метод для открытия подключения
        //public void OpenConnection()
        //{
        //    if (connection.State == ConnectionState.Closed)
        //    {
        //        connection.Open();
        //    }
        //}

        //// Метод для закрытия подключения
        //public void CloseConnection()
        //{
        //    if (connection.State == ConnectionState.Open)
        //    {
        //        connection.Close();
        //    }
        //}

        // Метод для выполнения запросов, которые не возвращают данные (например, INSERT, UPDATE, DELETE)
        //public void ExecuteQuery(string query)
        //{
        //    try
        //    {
        //        OpenConnection();
        //        MySqlCommand command = new MySqlCommand(query, connection);
        //        command.ExecuteNonQuery();
        //    }
        //    catch (MySqlException ex)
        //    {
        //        // Обработка ошибок
        //        Console.WriteLine("Ошибка при выполнении запроса: " + ex.Message);
        //    }
        //    finally
        //    {
        //        CloseConnection();
        //    }
        //}

        //// Метод для выполнения запросов, которые возвращают данные (например, SELECT)
        //public DataTable GetDataTable(string query)
        //{
        //    DataTable dataTable = new DataTable();
        //    try
        //    {
        //        OpenConnection();
        //        MySqlCommand command = new MySqlCommand(query, connection);
        //        MySqlDataAdapter adapter = new MySqlDataAdapter(command);
        //        adapter.Fill(dataTable);
        //    }
        //    catch (MySqlException ex)
        //    {
        //        // Обработка ошибок
        //        Console.WriteLine("Ошибка при выполнении запроса: " + ex.Message);
        //    }
        //    finally
        //    {
        //        CloseConnection();
        //    }
        //    return dataTable;
        //}

        //public DataTable GetSchemaTable(string tableName)
        //{
        //    DataTable schemaTable = new DataTable();
        //    string query = $"SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, EXTRA FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}'";
        //    try
        //    {
        //        OpenConnection();
        //        MySqlCommand command = new MySqlCommand(query, connection);
        //        MySqlDataAdapter adapter = new MySqlDataAdapter(command);
        //        adapter.Fill(schemaTable);
        //    }
        //    catch (MySqlException ex)
        //    {
        //        // Обработка ошибок
        //        Console.WriteLine("Ошибка при получении схемы таблицы: " + ex.Message);
        //    }
        //    finally
        //    {
        //        CloseConnection();
        //    }
        //    return schemaTable;
        //}
    }
}