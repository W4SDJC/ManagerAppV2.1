using MySql.Data.MySqlClient;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace ManagerAppV4._0
{
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
            ConnectionErrorText = "";

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
            try
            {
                MySQLTableChecker checker = new MySQLTableChecker(CH.GetConnectionString());

                if (!checker.AreRequiredTablesPresent())
                {
                    SetSettingButtonImage();
                    StartErrorAnimation(SettingButton);
                    LoginButton.IsEnabled = false;
                }
                else
                {
                    SetSettingButtonImage();
                    StopErrorAnimation(SettingButton);
                    LoginButton.IsEnabled = true;
                    this.Title = $"M.App 4.0 Pre-release";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetSettingButtonImage()
        {
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(@"/Icons/Setting.png", UriKind.RelativeOrAbsolute);
            bi.EndInit();
            SettingButtonImage.Source = bi;
        }

        private void Login()
        {
            string login = LoginTextBox.Text.Trim();
            string password = Passwordbox.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            bool isConnected = CH.CheckMySQLConnection(filePath);

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
                        string getRole = "SELECT role FROM users WHERE login = @login";
                        MySqlCommand roleCmd = new MySqlCommand(getRole, connection);
                        roleCmd.Parameters.AddWithValue("@login", login);
                        object role = roleCmd.ExecuteScalar();

                        // Получение имени пользователя
                        string getName = "SELECT name FROM users WHERE login = @login";
                        MySqlCommand nameCmd = new MySqlCommand(getName, connection);
                        nameCmd.Parameters.AddWithValue("@login", login);
                        string name = nameCmd.ExecuteScalar()?.ToString();

                        // Получение имени базы данных
                        string getDbName = "SELECT databasename FROM users WHERE login = @login";
                        MySqlCommand dbCmd = new MySqlCommand(getDbName, connection);
                        dbCmd.Parameters.AddWithValue("@login", login);
                        string dbName = dbCmd.ExecuteScalar()?.ToString();

                        // Установка глобальных данных
                        DataSource.UserName = name;
                        DataSource.Role = role?.ToString();
                        DataSource.Login = login;
                        DataSource.DBname = dbName;

                        // Переход в главное окно
                        MainWindow mainWindow = new MainWindow();
                        mainWindow.Tablename = dbName;
                        mainWindow.Show();
                        this.Close();

                        connection.Close();
                    }
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль","Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            Login();
        }

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
        private void Enter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Login(); // Закрываем текущее окно
            }
        }

        
    }
}