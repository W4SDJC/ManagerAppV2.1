using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Windows;
using System.IO;

namespace ManagerAppV4._0
{
    public class ConnectHelper
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public string Database { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string Version { get; set; }
        private static string filePath = "Config.json";


        public void Load()
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                var loadedConfig = JsonConvert.DeserializeObject<ConnectHelper>(json);
                if (loadedConfig != null)
                {
                    this.Server = loadedConfig.Server;
                    this.Port = loadedConfig.Port;
                    this.Database = loadedConfig.Database;
                    this.User = loadedConfig.User;
                    this.Password = loadedConfig.Password;
                    this.Version = "4.0 Pre-release";

                }
            }
            else
            {
                // Файл не найден — создаём с настройками по умолчанию
                MessageBox.Show("Файл конфигурации не найден. Будет создан новый файл с настройками по умолчанию.",
                    "Информация", MessageBoxButton.OK, MessageBoxImage.Information);

                // Настройки по умолчанию
                this.Server = "localhost";
                this.Port = 3306;
                this.Database = "default_db";
                this.User = "root";
                this.Password = "";
                this.Version = "4.0 Pre-release";

                // Сохраняем в файл
                string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(filePath, json);
            }
        }

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
        public string GetConnectionString(int mode = 1)
        {
            Load();

            string decryptedPassword = CryptoHelper.Decrypt(Password);

            switch (mode)
            {
                case 1: // С базой данных
                    return $"server={Server};port={Port};user={User};password={decryptedPassword};database={Database};";

                case 2: // Без базы данных
                    return $"server={Server};port={Port};user={User};password={decryptedPassword};";

                default:
                    throw new ArgumentException("Недопустимое значение mode. Допустимо: 1 или 2.");
            }
        }

        public bool CheckMySQLConnection(string configFilePath)
        {
            try
            {
                if (!File.Exists(configFilePath))
                {
                    MessageBox.Show("Файл конфигурации не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                string json = File.ReadAllText(configFilePath);
                ConnectHelper config = JsonConvert.DeserializeObject<ConnectHelper>(json);

                if (config == null)
                {
                    MessageBox.Show("Ошибка чтения конфигурации.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                string connectionString = $"Server={config.Server};Port={config.Port};Uid={config.User};Pwd={CryptoHelper.Decrypt(config.Password)};";

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    connection.Close();
                    return true;
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Ошибка подключения к MySQL: {ex.Message}", "Ошибка подключения", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Общая ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public string SystemQuery(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentException("Имя таблицы не может быть пустым", nameof(tableName));
            }

            // Приводим к единому формату (нижний регистр, удаляем пробелы)
            string normalizedTableName = tableName.Trim().ToLower().Replace(" ", "");

            switch (normalizedTableName)
            {
                case "productprice":
                    return """
                SELECT 
                    id,
                    `Product_name` as 'Название',
                    `Product_price` as 'Цена',
                    `Minimum_price` as 'Мин цена',
                    `Unit_of_measurement` as 'Ед измерения'
                FROM `product price`;
                """;

                case "roles":
                    return "SELECT id, `role` as 'Роль' FROM roles;";

                case "users":
                    return """
                SELECT 
                    id,
                    `name` as 'Имя',
                    `login` as 'Логин',
                    `password` as 'Пароль',
                    `role` as 'Роль',
                    `databasename` as 'Таблица',
                    `monthplan` as 'План',
                    `oklad` as 'Оклад'
                FROM users;
                """;

                case "warehouses":
                    return "SELECT id, `name` as 'Название', `address` as 'Адрес' FROM warehouses;";

                default:
                    throw new NotSupportedException($"Таблица '{tableName}' не поддерживается. Доступные таблицы: productprice, roles, users, warehouses");
            }
        }
        public string ManagerData(string DBName)
        {
            string Query = "SELECT " +
                "id, " +
                "DATE_FORMAT(ShipmentDate, '%d.%m.%Y') as 'Дата отправки', " +
                "ShipmentWarehouse as 'Склад отправки', " +
                "ClientCity as 'Город покупателя', " +
                "ClientName as 'Покупатель', " +
                "ProductName as 'Товар', " +
                "ProductAmount as 'Количество', " +
                "UnitOfMeasurement as 'Ед Изм', " +
                "Price as 'Цена менеджера', " +
                "MinimumPrice as 'Мин цена', " +
                "ShipmentValue as 'Итого менеджера', " +
                "`ShipmentValue(Minimum_price)` as 'Итого (Мин)', " +
                "UPDNumber as 'Номер УПД', " +
                "ShipmentPrice as 'Стоимость доставки' " +
                $"FROM `{DBName}`;";

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
        public string GetOklad( string table)
        {
            // Проверка входного параметра
            if (string.IsNullOrWhiteSpace(table))
            {
                throw new ArgumentException("Имя таблицы не может быть пустым", nameof(table));
            }

            string oklad = null;

            try
            {
                using (var con = new MySqlConnection(GetConnectionString()))
                {
                    // Используем параметризованный запрос для безопасности
                    string query = $"SELECT `oklad` FROM users WHERE `databasename` = @TableName;";

                    using (var cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@TableName", table);

                        con.Open();
                        var result = cmd.ExecuteScalar();
                        // Безопасное преобразование результата
                        oklad = result?.ToString();
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
            return oklad;
        }
    }
}