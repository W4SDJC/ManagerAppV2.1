using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ManagerAppV2._1
{
    public partial class ConnectionSettings : Window
    {
        private string filePath = "Config.json";
        private LoginWindow loginWindow;
        public ConnectionSettings(LoginWindow callerWindow)
        {
            InitializeComponent();
            Load();
            loginWindow = callerWindow; // сохранить ссылку на главное окно
            if (!string.IsNullOrEmpty(loginWindow.ConnectionErrorText))
            {
                ErrorTextBlock.Text = loginWindow.ConnectionErrorText;
            }
        }
        private void Load()
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                ConnectHelper CH = JsonConvert.DeserializeObject<ConnectHelper>(json);
                if (CH != null)
                {
                    ServerTextBox.Text = CH.Server;
                    PortTextBox.Text = CH.Port.ToString();
                    DatabaseTextBox.Text = CH.Database;
                    UserTextBox.Text = CH.User;
                    PasswordBox.Password = CH.Password;
                    CH.Load();
                }
            }
            else
            {
                MessageBox.Show("Файл конфигурации не найден. Будет создан новый файл с настройками по умолчанию.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);

                // Создание файла с настройками по умолчанию
                ConnectHelper defaultCH = new ConnectHelper
                {
                    Server = "localhost",
                    Port = 3306,
                    Database = "database",
                    User = "root",
                    Password = ""
                };
                if (Application.Current.MainWindow is LoginWindow LoginWindow)
                {
                    LoginWindow.UpdateLoginButtonState(true);
                }
                string json = JsonConvert.SerializeObject(defaultCH, Formatting.Indented);
                File.WriteAllText(filePath, json);

                // Заполнение полей формы значениями по умолчанию
                ServerTextBox.Text = defaultCH.Server;
                PortTextBox.Text = defaultCH.Port.ToString();
                DatabaseTextBox.Text = defaultCH.Database;
                UserTextBox.Text = defaultCH.User;
                PasswordBox.Password = defaultCH.Password;
            }
        }


        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(PortTextBox.Text, out int port))
            {
                ConnectHelper CH = new ConnectHelper
                {
                    Server = ServerTextBox.Text,
                    Port = port,
                    Database = DatabaseTextBox.Text,
                    User = UserTextBox.Text,
                    Password = PasswordBox.Password
                };

                string json = JsonConvert.SerializeObject(CH, Formatting.Indented);
                File.WriteAllText(filePath, json);
                MessageBox.Show("Данные сохранены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                bool isConnected = CheckMySQLConnection("Config.json");
                if (isConnected)
                {
                    ErrorTextBlock.Foreground = new SolidColorBrush(Colors.Green);
                    ErrorTextBlock.Text = "Подключение успешно";
                    TableCheck();
                    loginWindow.StopErrorAnimation(loginWindow.SettingButton);
                    loginWindow.LoginButton.IsEnabled = true;
                    this.Close();
                }
                else
                {
                    ErrorTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                    ErrorTextBlock.Text = "Подключение не удалось";
                }
            }
            else
            {
                MessageBox.Show("Порт должен быть числом!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void TableCheck()
        {
            ConnectHelper CH = new ConnectHelper();

            MySQLTableChecker checker = new MySQLTableChecker(CH.GetConnectionString());

            if (checker.AreRequiredTablesPresent())
            {
                MessageBox.Show("Все нужные таблицы найдены.");
            }
            else
            {
                var DialogResult = MessageBox.Show("Отсутствуют необходимые таблицы. \nСоздать таблицы?", "Отсутсвие таблиц", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (DialogResult == MessageBoxResult.Yes)
                {
                    string query1 = @"
                            CREATE DATABASE IF NOT EXISTS `database`
                            /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ 
                            /*!80016 DEFAULT ENCRYPTION='N' */;

                            USE `database`;

                            CREATE TABLE IF NOT EXISTS `admin` (
                                `id` INT NOT NULL AUTO_INCREMENT,
                                `ShipmentDate` DATE DEFAULT NULL,
                                `ShipmentWarehouse` VARCHAR(60) DEFAULT NULL,
                                `ClientCity` VARCHAR(45) DEFAULT NULL,
                                `ClientName` VARCHAR(45) DEFAULT NULL,
                                `ProductName` VARCHAR(45) DEFAULT NULL,
                                `ProductAmount` VARCHAR(45) DEFAULT NULL,
                                `UnitOfMeasurement` VARCHAR(45) DEFAULT NULL,
                                `Price` INT DEFAULT NULL,
                                `MinimumPrice` INT DEFAULT NULL,
                                `ShipmentValue` INT DEFAULT NULL,
                                `ShipmentValue(Minimum_price)` INT DEFAULT NULL,
                                `Reward` INT DEFAULT NULL,
                                `UPDNumber` INT DEFAULT NULL,
                                `ShipmentPrice` INT DEFAULT NULL,
                                PRIMARY KEY (`id`),
                                UNIQUE KEY `id_UNIQUE` (`id`),
                                UNIQUE KEY `UPDNumber_UNIQUE` (`UPDNumber`)
                            ) ENGINE=InnoDB AUTO_INCREMENT=2 
                              DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

                            CREATE TABLE IF NOT EXISTS `product price` (
                                `id` INT NOT NULL AUTO_INCREMENT,
                                `Product_name` VARCHAR(45) DEFAULT NULL,
                                `Product_price` INT DEFAULT NULL,
                                `Minimum_price` VARCHAR(45) DEFAULT NULL,
                                `Unit_of_measurement` VARCHAR(45) DEFAULT NULL,
                                `Role` VARCHAR(45) DEFAULT NULL,
                                PRIMARY KEY (`id`),
                                UNIQUE KEY `id_UNIQUE` (`id`),
                                UNIQUE KEY `Product name_UNIQUE` (`Product_name`)
                            ) ENGINE=InnoDB AUTO_INCREMENT=12 
                              DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci 
                              COMMENT='\t\t\t';

                            CREATE TABLE IF NOT EXISTS `roles` (
                                `id` INT NOT NULL AUTO_INCREMENT,
                                `role` VARCHAR(45) DEFAULT NULL,
                                `MonthPlan` VARCHAR(45) DEFAULT NULL,
                                PRIMARY KEY (`id`),
                                UNIQUE KEY `id_UNIQUE` (`id`),
                                UNIQUE KEY `role_UNIQUE` (`role`)
                            ) ENGINE=InnoDB AUTO_INCREMENT=8 
                              DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

                            CREATE TABLE IF NOT EXISTS `users` (
                                `id` INT NOT NULL AUTO_INCREMENT,
                                `name` VARCHAR(45) DEFAULT NULL,
                                `login` VARCHAR(45) NOT NULL,
                                `password` VARCHAR(128) NOT NULL,
                                `role` VARCHAR(45) DEFAULT NULL,
                                `databasename` VARCHAR(45) DEFAULT NULL,
                                `monthplan` LONGTEXT,
                                PRIMARY KEY (`id`),
                                UNIQUE KEY `id_UNIQUE` (`id`),
                                UNIQUE KEY `login_UNIQUE` (`login`),
                                UNIQUE KEY `databasename_UNIQUE` (`databasename`)
                            ) ENGINE=InnoDB AUTO_INCREMENT=50 
                              DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

                            CREATE TABLE IF NOT EXISTS `warehouses` (
                                `id` INT NOT NULL AUTO_INCREMENT,
                                `name` VARCHAR(45) DEFAULT NULL,
                                `address` VARCHAR(45) DEFAULT NULL,
                                PRIMARY KEY (`id`),
                                UNIQUE KEY `idwarehouse_UNIQUE` (`id`),
                                UNIQUE KEY `name_UNIQUE` (`name`),
                                UNIQUE KEY `address_UNIQUE` (`address`)
                            ) ENGINE=InnoDB AUTO_INCREMENT=4 
                              DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
                            ";
                    string query2 = $@"
                            INSERT INTO users(`name`, `login`, `password`, `role`, `databasename`) 
                            VALUES ('Admin', 'admin', '{PasswordHasher.HashPassword("admin")}', 'Admin', 'admin');
                            ";
                    try
                    {
                        using (MySqlConnection conn = new MySqlConnection(CH.GetConnectionString()))
                        {
                            conn.Open();
                            using (MySqlCommand command = new MySqlCommand(query1, conn))
                            {
                                int result = command.ExecuteNonQuery();

                                if (result > 0)
                                {
                                    MessageBox.Show("Таблицы успешно созданы!");
                                }
                            }
                            using (MySqlCommand command = new MySqlCommand(query2, conn))
                            {
                                int result = command.ExecuteNonQuery();

                                if (result > 0)
                                {
                                    MessageBox.Show("Admin пользователь создан!\nЛогин: admin Пароль: admin");
                                }
                            }
                        }
                    }
                    catch (Exception ex) { }
                }
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

                string connectionString = $"Server={config.Server};Port={config.Port};Database={config.Database};Uid={config.User};Pwd={config.Password};";

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open(); // Попытка открыть соединение
                    connection.Close();
                    return true; // Успех
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
    }
}
