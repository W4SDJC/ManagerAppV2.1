using MySql.Data.MySqlClient;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ManagerAppV3._5
{

    public partial class EditUser : Window
    {
        ConnectHelper CH = new ConnectHelper();
        public EditUser()
        {
            InitializeComponent();
            LoadComboBoxDataAsync();
            LoadRoleComboBoxData();
        }
        private async Task LoadComboBoxDataAsync()
        {
            const string query = "SELECT DISTINCT login FROM `users` WHERE id != 0 ORDER BY login;"; // Добавлена сортировка

            try
            {
                // Показываем индикатор загрузки
                LoginCB.IsEnabled = false;
                LoginCB.DisplayMemberPath = null;
                LoginCB.ItemsSource = new List<string> { "Загрузка..." };

                // Асинхронная загрузка данных
                var items = await Task.Run(() =>
                {
                    var result = new List<string>();

                    using (var connection = new MySqlConnection(CH.GetConnectionString()))
                    using (var command = new MySqlCommand(query, connection))
                    {
                        connection.Open();

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (!reader.IsDBNull(0))
                                {
                                    string login = reader.GetString(0);
                                    if (!string.IsNullOrWhiteSpace(login))
                                    {
                                        result.Add(login);
                                    }
                                }
                            }
                        }
                    }
                    return result;
                });

                // Обновляем UI в основном потоке
                if (items.Count > 0)
                {
                    LoginCB.ItemsSource = items;
                    LoginCB.DisplayMemberPath = ".";
                    LoginCB.SelectedIndex = 0;
                }
                else
                {
                    LoginCB.ItemsSource = new List<string> { "Нет данных" };
                    LoginCB.SelectedIndex = -1;
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}\n\nКод ошибки: {ex.Number}",
                              "Ошибка MySQL",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);

                // Логирование ошибки
                Debug.WriteLine($"MySQL Error {ex.Number}: {ex.Message}\n{ex.StackTrace}");

                LoginCB.ItemsSource = new List<string> { "Ошибка загрузки" };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Неожиданная ошибка: {ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);

                Debug.WriteLine($"Error: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                LoginCB.IsEnabled = true;
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверка обязательных полей
            ValidateTextBox(LoginTextBox, "Login");
            ValidateTextBox(NameTextBox, "Name");

            // Получаем ID пользователя
            string id;
            using (MySqlConnection con = new MySqlConnection(CH.GetConnectionString()))
            {
                con.Open();
                string getID = $"SELECT id FROM users where login = '{LoginCB.SelectedItem.ToString()}';";
                MySqlCommand IDGetter = new MySqlCommand(getID, con);
                id = IDGetter.ExecuteScalar().ToString();
            }

            // Определяем имя базы данных
            string dbname = string.IsNullOrEmpty(DataBaseNameTextBox.Text)
                ? LoginTextBox.Text + RoleComboBox.SelectedItem.ToString()
                : DataBaseNameTextBox.Text;

            // Проверяем, были ли заполнены поля пароля
            bool passwordFieldsEmpty = string.IsNullOrEmpty(PasswordTextBox.Text) &&
                                      string.IsNullOrEmpty(ConfirmPasswordTextBox.Text);

            // Если поля пароля не пустые, проверяем их совпадение
            if (!passwordFieldsEmpty)
            {
                ValidateTextBox(PasswordTextBox, "Password");
                ValidateTextBox(ConfirmPasswordTextBox, "Password confirmation");

                if (!ConfirmingPassword(PasswordTextBox.Text, ConfirmPasswordTextBox.Text))
                {
                    MessageBox.Show("Пароли не совпадают", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            // Строим SQL запрос
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
                $"`Reward` int DEFAULT NULL," +
                $"`UPDNumber` int DEFAULT NULL," +
                $"`ShipmentPrice` int DEFAULT NULL," +
                $"PRIMARY KEY (`id`)," +
                $"UNIQUE KEY `id_UNIQUE` (`id`)," +
                $"UNIQUE KEY `UPDNumber_UNIQUE` (`UPDNumber`)" +
                $") ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;" +
                $"UPDATE users SET name = '{NameTextBox.Text}', login = '{LoginTextBox.Text}', " +
                $"role = '{RoleComboBox.SelectedItem}', databasename = '{dbname}'";

            // Добавляем обновление пароля только если поля не пустые
            if (!passwordFieldsEmpty)
            {
                query += $", password = '{PasswordHasher.HashPassword(ConfirmPasswordTextBox.Text)}'";
            }

            query += $" WHERE id = '{id}'";

            try
            {
                using (MySqlConnection connection = new MySqlConnection(CH.GetConnectionString()))
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        int result = command.ExecuteNonQuery();
                        MessageBox.Show($"Пользователь {dbname} успешно изменен!", "Успех",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Ошибка при создании таблицы:\n{ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }

            LoadComboBoxDataAsync();
            var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();

            if (mainWindow != null)
            {
                mainWindow.UpdateAll();
            }
        }

        private bool ConfirmingPassword(string password, string confirmPassword)
        {
            return password == confirmPassword;
        }


        private async Task LoadUserDataAsync(string login)
        {
            const string query = @"
        SELECT login, name, password, role, databasename 
        FROM users 
        WHERE login = @login 
        LIMIT 1";

            try
            {
                using var conn = new MySqlConnection(CH.GetConnectionString());
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@login", login);

                await conn.OpenAsync();

                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    // Безопасное чтение данных
                    LoginTextBox.Text = reader["login"]?.ToString() ?? "";
                    NameTextBox.Text = reader["name"]?.ToString() ?? "";
                    DataBaseNameTextBox.Text = reader["databasename"]?.ToString() ?? "";

                    // Безопасное преобразование пароля
                    if (reader["password"] != DBNull.Value)
                    {
                        try
                        {
                            byte[] hashBytes = Convert.FromBase64String(reader["password"].ToString());
                            // Дальнейшая обработка хеша
                        }
                        catch (FormatException)
                        {
                            Debug.WriteLine("Некорректный формат хеша пароля");
                        }
                    }

                    // Установка значения в ComboBox ролей
                    var role = reader["role"]?.ToString();
                    if (!string.IsNullOrEmpty(role))
                    {
                        RoleComboBox.SelectedItem = role;
                    }
                    else
                    {
                        RoleComboBox.SelectedIndex = -1;
                    }
                }
                else
                {
                    MessageBox.Show("Пользователь не найден", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearUserForm();
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Ошибка базы данных: {ex.Message}\nКод ошибки: {ex.Number}",
                              "Ошибка MySQL",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
                ClearUserForm();
            }
        }

        private void LoadRoleComboBoxData()
        {
            string query = "SELECT DISTINCT role FROM `roles` WHERE id != 0;";
            List<string> items = new List<string>();

            try
            {
                using (MySqlConnection connection = new MySqlConnection(CH.GetConnectionString()))
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (!reader.IsDBNull(0))
                                items.Add(reader.GetString(0));
                        }
                    }
                }

                RoleComboBox.ItemsSource = items;
                if (items.Count > 0)
                    RoleComboBox.SelectedIndex = 0;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Ошибка MySQL: {ex.Message}");
            }
        }
        private string ValidateTextBox(TextBox textBox, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.BorderBrush = Brushes.Red;
                return $"Please, fill the field {fieldName}.\n";
            }
            return "";
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void LoginCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                // Проверяем, что есть выбранный элемент и это строка
                if (LoginCB.SelectedItem is string selectedLogin && !string.IsNullOrWhiteSpace(selectedLogin))
                {
                    await LoadUserDataAsync(selectedLogin);
                }
                else
                {
                    ClearUserForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных пользователя: {ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
                ClearUserForm();
            }
        }
        private void ClearUserForm()
        {
            LoginTextBox.Text = "";
            NameTextBox.Text = "";
            DataBaseNameTextBox.Text = "";
            RoleComboBox.SelectedIndex = -1;
        }
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            string id;
            using (MySqlConnection con = new MySqlConnection(CH.GetConnectionString()))
            {
                con.Open();


                string getID = $"SELECT id FROM users where login = '{LoginCB.SelectedItem.ToString()}';";
                MySqlCommand IDGetter = new MySqlCommand(getID, con);
                id = IDGetter.ExecuteScalar().ToString();

                string getdbname = $"SELECT databasename FROM users where login = '{LoginCB.SelectedItem.ToString()}';";
                MySqlCommand DBNameGetter = new MySqlCommand(getdbname, con);
                string DBname = DBNameGetter.ExecuteScalar().ToString();

                if (ConfirmingPassword(PasswordTextBox.Text, ConfirmPasswordTextBox.Text))
                {
                    string query =
                        $"DROP TABLE `{DBname}`;" +
                        $"DELETE FROM users WHERE (`id` = {id});";
                    try
                    {
                        using (MySqlConnection connection = new MySqlConnection(CH.GetConnectionString()))
                        {
                            connection.Open();

                            using (MySqlCommand command = new MySqlCommand(query, connection))
                            {
                                int result = command.ExecuteNonQuery();
                                MessageBox.Show($"Пользователь {LoginCB.SelectedItem} успешно изменен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }
                    catch (MySqlException ex)
                    {
                        MessageBox.Show($"Ошибка при создании таблицы:\n{ex.Message}", "Ошибка",
                                        MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                }
                else
                {
                    MessageBox.Show("Passwords don't match", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                LoginCB.ItemsSource = null;
                LoginCB.Items.Clear();
                LoadComboBoxDataAsync();
                var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();

                if (mainWindow != null)
                {
                    mainWindow.UpdateAll();
                }
            }
        }

        private void EditRoleBtn_Click(object sender, RoutedEventArgs e)
        {
            EditRole roleWindow = new EditRole();
            roleWindow.RoleChanged += LoadRoleComboBoxData; // Подписка на событие для обновления ComboBox
            roleWindow.ShowDialog();
        }
    }

}
