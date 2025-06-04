using Kursovaya2;
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
        public string Role = "standart2222";
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


                    MainWindow MW = new MainWindow(/*role.ToString(), login, name*/);
                    //MW.LoggedInUsername = login;
                    //MW.LoggedInRole = role.ToString();
                    //MW.DatabaseName = DBname;
                    MessageBox.Show(DBname);
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

    }
}
