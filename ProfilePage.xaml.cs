using ManagerAppV2;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
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
    /// Логика взаимодействия для ProfilePage.xaml
    /// </summary>
    public partial class ProfilePage : Window
    {
        ConnectHelper CH = new ConnectHelper();
        public ProfilePage()
        {
            InitializeComponent();
            Load();
        }

        private void Load()
        {
            NameTextBox.Text = DataSource.UserName;
            RoleTextBox.Text = DataSource.Role;
        }

        private void UpdateName()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(CH.GetConnectionString()))
                {
                    connection.Open();

                    // Предположим, что у нас есть таблица Users с колонками Id и Name
                    string query = "UPDATE users SET name = @newName WHERE login = @login";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        // Параметры для защиты от SQL-инъекций
                        command.Parameters.AddWithValue("@newName", NameTextBox.Text);
                        command.Parameters.AddWithValue("@login", DataSource.Login);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Имя успешно обновлено!");
                        }
                        else
                        {
                            MessageBox.Show("Запись не найдена или не изменена.");
                        }
                    }
                }
                DataSource.UserName = NameTextBox.Text;
                MainWindow MW = new MainWindow();
                MW.NameLabel.Content = DataSource.UserName;

                var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();

                if (mainWindow != null)
                {
                    // Закрываем текущее главное окно
                    mainWindow.Close();

                    // Создаем новое главное окно
                    var newMainWindow = new MainWindow();
                    newMainWindow.Show();

                    // Закрываем текущее окно (если нужно)
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Уверены, что хотите поменять имя профиля? \nЛогин изменен не будет", "Подтверждение изменения", MessageBoxButton.YesNo, MessageBoxImage.Question, defaultResult: MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                UpdateName();
            }
        }
    }
}

