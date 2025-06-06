using ManagerAppV2;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace ManagerAppV2._1
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        ConnectHelper CH = new ConnectHelper();

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text.Trim();
            string password = Passwordbox.Password;


            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль");
                return;
            }
            string storedHash = GetPasswordHashFromDatabase(login);
            if (storedHash != null && PasswordHasher.VerifyPassword(password, storedHash))
            {
                using (MySqlConnection connection = new MySqlConnection(CH.GetConnectionString()))
                {
                    connection.Open();

                    string getrole = $"SELECT role FROM users WHERE login = '{login}'";
                    MySqlCommand RoleGetter = new MySqlCommand(getrole, connection);
                    object role = RoleGetter.ExecuteScalar();

                    string getname = $"SELECT name FROM users where login = '{login}';";
                    MySqlCommand NameGetter = new MySqlCommand(getname, connection);
                    string name = NameGetter.ExecuteScalar().ToString();

                    string getdbname = $"SELECT databasename FROM users where login = '{login}';";
                    MySqlCommand DBNameGetter = new MySqlCommand(getdbname, connection);
                    string DBname = DBNameGetter.ExecuteScalar().ToString();

                    DataSource.UserName = name.ToString();
                    DataSource.Role = role.ToString();
                    DataSource.Login = login;
                    DataSource.DBname = DBname;

                    MainWindow MW = new MainWindow();
                    MW.Show();
                    this.Close();
                    connection.Close();
                }

            }
            else
            {
                MessageBox.Show("Неверный логин или пароль");
            }
        }
  

        private string GetPasswordHashFromDatabase(string username)
        {
            // Пример получения хеша из БД
            using (MySqlConnection connection = new MySqlConnection(CH.GetConnectionString()))
            {
                var cmd = new MySqlCommand("SELECT password FROM users WHERE login = @username", connection);
                cmd.Parameters.AddWithValue("@username", username);
                connection.Open();
                return cmd.ExecuteScalar()?.ToString();
            }
        }

        private void RegButton_Click(object sender, RoutedEventArgs e)
        {
            var DialogResult = MessageBox.Show("Создать все базовые таблицы?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            string query1 = "CREATE TABLE  if not exists `monthplan` (\r\n  `id` int NOT NULL AUTO_INCREMENT,\r\n  `Role` varchar(45) DEFAULT NULL,\r\n  `Month` varchar(45) DEFAULT NULL,\r\n  PRIMARY KEY (`id`),\r\n  UNIQUE KEY `idMonthPlan_UNIQUE` (`id`)\r\n) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;\r\n\r\nCREATE TABLE  if not exists `product price` (\r\n  `id` int NOT NULL AUTO_INCREMENT,\r\n  `Product_name` varchar(45) DEFAULT NULL,\r\n  `Product_price` int DEFAULT NULL,\r\n  `Minimum_price` varchar(45) DEFAULT NULL,\r\n  `Unit_of_measurement` varchar(45) DEFAULT NULL,\r\n  `Role` varchar(45) DEFAULT NULL,\r\n  PRIMARY KEY (`id`),\r\n  UNIQUE KEY `id_UNIQUE` (`id`),\r\n  UNIQUE KEY `Product name_UNIQUE` (`Product_name`)\r\n) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='\t\t\t';\r\n\r\nCREATE TABLE  if not exists `roles` (\r\n  `id` int NOT NULL AUTO_INCREMENT,\r\n  `role` varchar(45) DEFAULT NULL,\r\n  `MonthPlan` varchar(45) DEFAULT NULL,\r\n  PRIMARY KEY (`id`),\r\n  UNIQUE KEY `id_UNIQUE` (`id`),\r\n  UNIQUE KEY `role_UNIQUE` (`role`)\r\n) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;\r\n\r\nCREATE TABLE  if not exists `users` (\r\n  `id` int NOT NULL AUTO_INCREMENT,\r\n  `name` varchar(45) DEFAULT NULL,\r\n  `login` varchar(45) NOT NULL,\r\n  `password` varchar(128) NOT NULL,\r\n  `role` varchar(45) DEFAULT NULL,\r\n  `databasename` varchar(45) DEFAULT NULL,\r\n  `image` longtext,\r\n  PRIMARY KEY (`id`),\r\n  UNIQUE KEY `id_UNIQUE` (`id`),\r\n  UNIQUE KEY `login_UNIQUE` (`login`),\r\n  UNIQUE KEY `databasename_UNIQUE` (`databasename`)\r\n) ENGINE=InnoDB AUTO_INCREMENT=49 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;\r\n\r\nCREATE TABLE  if not exists `warehouse` (\r\n  `id` int NOT NULL AUTO_INCREMENT,\r\n  `name` varchar(45) DEFAULT NULL,\r\n  `adress` varchar(45) DEFAULT NULL,\r\n  PRIMARY KEY (`id`),\r\n  UNIQUE KEY `idwarehouse_UNIQUE` (`id`),\r\n  UNIQUE KEY `name_UNIQUE` (`name`)\r\n) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;\r\n CREATE TABLE if not exists `admin` ( \r\n                `id` int NOT NULL AUTO_INCREMENT, \r\n                `ShipmentDate` date DEFAULT NULL, \r\n                `ShipmentWarehouse` varchar(60) DEFAULT NULL, \r\n                `ClientCity` varchar(45) DEFAULT NULL, \r\n                `ClientName` varchar(45) DEFAULT NULL, \r\n                `ProductName` varchar(45) DEFAULT NULL, \r\n                `ProductAmount` varchar(45) DEFAULT NULL, \r\n                `UnitOfMeasurement` varchar(45) DEFAULT NULL, \r\n                `Price` int DEFAULT NULL, \r\n                `MinimumPrice` int DEFAULT NULL, \r\n                `ShipmentValue` int DEFAULT NULL, \r\n                `ShipmentValue(Minimum_price)` int DEFAULT NULL, \r\n                `Reward` int DEFAULT NULL, \r\n                `UPDNumber` int DEFAULT NULL, \r\n                `ShipmentPrice` int DEFAULT NULL, \r\n                PRIMARY KEY (`id`), \r\n                UNIQUE KEY `id_UNIQUE` (`id`), \r\n                UNIQUE KEY `UPDNumber_UNIQUE` (`UPDNumber`) \r\n                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;";
            string query2 = $"INSERT INTO users(`name`, `login`, `password`, `role`, `databasename`) VALUES ('Admin','admin', '{PasswordHasher.HashPassword("admin")}', 'Admin', 'admin');";
            if (DialogResult == MessageBoxResult.Yes)
            {
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
                                //ClearUserForm();
                            }
                        }
                        using (MySqlCommand command = new MySqlCommand(query2, conn))
                        {
                            int result = command.ExecuteNonQuery();

                            if (result > 0)
                            {
                                MessageBox.Show("Admin пользователь создан!\nЛогин: admin Пароль: admin");
                                //ClearUserForm();
                            }
                        }
                    }
                }
                catch (Exception ex) { }
            }
        }
    }
}
