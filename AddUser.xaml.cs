using MySql.Data.MySqlClient;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ManagerAppV4._0
{
    public partial class AddUser : Window
    {
        public AddUser()
        {
            InitializeComponent();
            LoadRolesIntoComboBox();
        }
        ConnectHelper CH = new ConnectHelper();
        private void LoadRolesIntoComboBox()
        {
            string query = "SELECT DISTINCT role FROM `roles` WHERE id != 0;";
            List<string> items = new List<string>();

            try
            {
                using (MySqlConnection connection = new MySqlConnection(CH.GetConnectionString()))
                {
                    connection.Open();

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (!reader.IsDBNull(0))
                                {
                                    items.Add(reader.GetString(0));
                                }
                            }
                        }
                    }
                }

                RoleComboBox.ItemsSource = items;

                if (items.Count > 0)
                    RoleComboBox.SelectedIndex = 0;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Ошибка MySQL: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private string ValidateTextBox(TextBox textBox, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.BorderBrush = Brushes.Red;
                return $"Пожалуйста, заполните поле {fieldName}.\n";
            }
            return "";
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {

            string dbname;
            ValidateTextBox(LoginTextBox, "Login");
            ValidateTextBox(NameTextBox, "Name");
            ValidateTextBox(PasswordTextBox, "Password");
            ValidateTextBox(ConfirmPasswordTextBox, "Password confirmation");
            ValidateTextBox(OkladTextBox, "Оклад");
            if (string.IsNullOrEmpty(DataBaseNameTextBox.Text))
            {
                if (!ValidateInputFields())
                {
                    return; // выход, если не все поля заполнены
                }
                else
                {
                    dbname = LoginTextBox.Text + RoleComboBox.SelectedItem.ToString();
                }
            }
            else
            { dbname = DataBaseNameTextBox.Text; }
            if (ConfirmingPassword(PasswordTextBox.Text, ConfirmPasswordTextBox.Text))
            {
                string query = $"CREATE TABLE if not exists `{dbname}` (" +
                $"`id` int NOT NULL AUTO_INCREMENT," +
                $"`ShipmentDate` date DEFAULT NULL," +
                $"`ShipmentWarehouse` varchar(60) DEFAULT NULL," +
                $"`ClientCity` varchar(45) DEFAULT NULL," +
                $"`ClientName` varchar(45) DEFAULT NULL," +
                $"`ProductName` varchar(45) DEFAULT NULL," +
                $"`ProductAmount` varchar(45) DEFAULT NULL," +
                $"`UnitOfMeasurement` varchar(45) DEFAULT NULL," +
                $"`Price` int DEFAULT NULL," +
                $"`MinimumPrice` int DEFAULT NULL," +
                $"`ShipmentValue` int DEFAULT NULL," +
                $"`ShipmentValue(Minimum_price)` int DEFAULT NULL," +
                $"`UPDNumber` varchar(50) DEFAULT NULL," +
                $"`ShipmentPrice` int DEFAULT NULL," +
                $"PRIMARY KEY (`id`)," +
                $"UNIQUE KEY `id_UNIQUE` (`id`)," +
                $"UNIQUE KEY `UPDNumber_UNIQUE` (`UPDNumber`)" +
                $") ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;" +
                $"INSERT INTO users(`name`, `login`, `password`, `role`, `databasename`, `oklad`) VALUES ('{NameTextBox.Text}','{LoginTextBox.Text}', '{PasswordHasher.HashPassword(ConfirmPasswordTextBox.Text)}', '{RoleComboBox.SelectedItem}', '{dbname}', '{OkladTextBox.Text}');";
                try
                {
                    using (MySqlConnection connection = new MySqlConnection(CH.GetConnectionString()))
                    {
                        connection.Open();

                        using (MySqlCommand command = new MySqlCommand(query, connection))
                        {
                            int result = command.ExecuteNonQuery();
                            MessageBox.Show($"Таблица {dbname} успешно создана!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show($"Ошибка при создании таблицы:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();

                if (mainWindow != null)
                {
                    mainWindow.UpdateAll();
                }
            }
            else
            {
                MessageBox.Show("Пароли не совпадают", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }
        private bool ConfirmingPassword(string password, string confirmPassword)
        {
            return password == confirmPassword;
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string password = PasswordTextBox.Text;
            string confirmPassword = ConfirmPasswordTextBox.Text;

            if (password != confirmPassword)
            {
                MessageBox.Show("Пароли не совпадают", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string hashedPassword = PasswordHasher.HashPassword(password);

            // Сохраняем в БД
            SaveUserToDatabase(LoginTextBox.Text, hashedPassword);
        }
        private void SaveUserToDatabase(string username, string hashedPassword)
        {
            using (MySqlConnection connection = new MySqlConnection(CH.GetConnectionString()))
            {
                var cmd = new MySqlCommand("INSERT INTO Users (Username, PasswordHash) VALUES (@username, @passwordHash)",connection);

                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@passwordHash", hashedPassword);

                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }
        private void AddRole_Click(object sender, RoutedEventArgs e)
        {
            EditRole roleWindow = new EditRole();
            roleWindow.RoleChanged += LoadRolesIntoComboBox; // Подписка на событие для обновления ComboBox
            roleWindow.ShowDialog();
        }
        private bool ValidateInputFields()
        {
            StringBuilder errorMessages = new StringBuilder();

            // Проверка TextBox
            if (string.IsNullOrWhiteSpace(LoginTextBox.Text))
                errorMessages.AppendLine("Поле 'Логин' не заполнено.");

            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
                errorMessages.AppendLine("Поле 'Имя' не заполнено.");

            if (string.IsNullOrWhiteSpace(PasswordTextBox.Text))
                errorMessages.AppendLine("Поле 'Пароль' не заполнено.");

            if (string.IsNullOrWhiteSpace(ConfirmPasswordTextBox.Text))
                errorMessages.AppendLine("Поле 'Подтверждение пароля' не заполнено.");

            // Проверка ComboBox
            if (RoleComboBox.SelectedItem == null)
                errorMessages.AppendLine("Не выбрана роль.");

            // Если есть ошибки - вывести и вернуть false
            if (errorMessages.Length > 0)
            {
                MessageBox.Show(errorMessages.ToString(), "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true; // все проверки пройдены
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close(); // Закрываем текущее окно
            }
        }
    }
}