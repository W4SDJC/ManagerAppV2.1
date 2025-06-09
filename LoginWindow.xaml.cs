using ManagerAppV2;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace ManagerAppV2._1
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>

    public partial class LoginWindow : Window
    {
        public string ConnectionErrorText { get; private set; } = string.Empty;

        private readonly ConnectHelper CH = new ConnectHelper();
        private readonly string filePath = "Config.json";

        // Конструктор окна
        public LoginWindow()
        {
            InitializeComponent();
            CheckConfigFile(filePath, SettingButton);
            Load();
        }

        private void StartErrorAnimation(Button button)
        {
            // Установка текста ошибки
            ConnectionErrorText = "Ошибка наличия таблиц, проверьте данные и нажмите Сохранить";

            ColorAnimation animation = new ColorAnimation
            {
                From = Colors.LightGray,
                To = Colors.Red,
                Duration = TimeSpan.FromSeconds(0.5),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };

            SolidColorBrush brush = new SolidColorBrush(Colors.LightGray);
            button.Background = brush;
            brush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
        }
        public void StopErrorAnimation(Button button)
        {
            // Остановка текущей анимации
            if (button.Background is SolidColorBrush brush)
            {
                brush.BeginAnimation(SolidColorBrush.ColorProperty, null);
            }

            Color hexColor = (Color)ColorConverter.ConvertFromString("#333333");
            button.Background = new SolidColorBrush(hexColor);
        }
        private void Load()
        {
            MySQLTableChecker checker = new MySQLTableChecker(CH.GetConnectionString());

            if (!checker.AreRequiredTablesPresent())
            {
                BitmapImage bi = new BitmapImage();
                // BitmapImage.UriSource must be in a BeginInit/EndInit block.
                bi.BeginInit();
                bi.UriSource = new Uri(@"/Icons/Setting.png", UriKind.RelativeOrAbsolute);
                bi.EndInit();
                // Set the image source.
                SettingButtonImage.Source = bi;
                StartErrorAnimation(SettingButton);
                LoginButton.IsEnabled = false;
            }
            else
            {
                BitmapImage bi = new BitmapImage();
                // BitmapImage.UriSource must be in a BeginInit/EndInit block.
                bi.BeginInit();
                bi.UriSource = new Uri(@"/Icons/Setting.png", UriKind.RelativeOrAbsolute);
                bi.EndInit();
                // Set the image source.
                SettingButtonImage.Source = bi;
                StopErrorAnimation(SettingButton);
                LoginButton.IsEnabled = true;
            }
        }

        // Обработчик кнопки "Войти"
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text.Trim();
            string password = Passwordbox.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль");
                return;
            }

            bool isConnected = CheckMySQLConnection(filePath);

            if (isConnected)
            {
                MessageBox.Show("Успешное подключение к базе данных!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                CH.Load();

                string storedHash = GetPasswordHashFromDatabase(login);
                if (storedHash != null && PasswordHasher.VerifyPassword(password, storedHash))
                {
                    using (MySqlConnection connection = new MySqlConnection(CH.GetConnectionString()))
                    {
                        connection.Open();

                        // Получение роли пользователя
                        string getRole = $"SELECT role FROM users WHERE login = '{login}'";
                        MySqlCommand roleCmd = new MySqlCommand(getRole, connection);
                        object role = roleCmd.ExecuteScalar();

                        // Получение имени пользователя
                        string getName = $"SELECT name FROM users WHERE login = '{login}'";
                        MySqlCommand nameCmd = new MySqlCommand(getName, connection);
                        string name = nameCmd.ExecuteScalar()?.ToString();

                        // Получение имени базы данных
                        string getDbName = $"SELECT databasename FROM users WHERE login = '{login}'";
                        MySqlCommand dbCmd = new MySqlCommand(getDbName, connection);
                        string dbName = dbCmd.ExecuteScalar()?.ToString();

                        // Установка глобальных данных
                        DataSource.UserName = name;
                        DataSource.Role = role?.ToString();
                        DataSource.Login = login;
                        DataSource.DBname = dbName;

                        // Переход в главное окно
                        MainWindow mainWindow = new MainWindow();
                        mainWindow.Show();
                        this.Close();

                        connection.Close();
                    }
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль");
                }
            }
            // Ошибки уже обработаны внутри CheckMySQLConnection
        }

        // Проверка наличия конфигурационного файла и активация кнопки входа
        private void CheckConfigFile(string path, Button settingButton)
        {
            if (File.Exists(path))
            {
                LoginButton.IsEnabled = true;
            }
            else
            {
                LoginButton.Style = (Style)FindResource("DisabledButtonStyle");
                LoginButton.IsEnabled = false;
                settingButton.Background = new SolidColorBrush(Colors.Red);
            }
        }

        // Получение хеша пароля из базы данных по логину
        private string GetPasswordHashFromDatabase(string username)
        {
            using (MySqlConnection connection = new MySqlConnection(CH.GetConnectionString()))
            {
                var cmd = new MySqlCommand("SELECT password FROM users WHERE login = @username", connection);
                cmd.Parameters.AddWithValue("@username", username);
                connection.Open();
                return cmd.ExecuteScalar()?.ToString();
            }
        }

        // Включение или отключение кнопки входа из других окон
        public void UpdateLoginButtonState(bool isEnabled)
        {
            LoginButton.IsEnabled = isEnabled;
        }

        // Обработчик кнопки настроек подключения
        private void SettingButton_Click(object sender, RoutedEventArgs e)
        {
            ConnectionSettings CS = new ConnectionSettings(this); // передаем ссылку на главное окно
            CS.ShowDialog();
        }

        // Проверка подключения к MySQL через конфигурационный файл
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


    }
}
