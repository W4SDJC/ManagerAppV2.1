using ManagerAppV2._1;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace ManagerAppV2._1
{
    public partial class SetMonthPlan : Window
    {
        ConnectHelper CH = new ConnectHelper();
        private List<User> users = new List<User>();

        public SetMonthPlan()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(CH.GetConnectionString()))
                {
                    connection.Open();

                    string query = "SELECT name, login, role, monthplan FROM users";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            users.Clear();
                            while (reader.Read())
                            {
                                users.Add(new User
                                {
                                    Name = reader.GetString("name"),
                                    Login = reader.GetString("login"),
                                    Role = reader.GetString("role"),
                                    MonthPlan = reader.IsDBNull("monthplan") ? 0 : reader.GetDecimal("monthplan")
                                });
                            }
                        }
                    }
                }

                cmbUsers.ItemsSource = users;
                cmbUsers.SelectedIndex = -1;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Ошибка MySQL: {ex.Message}", "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CmbUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbUsers.SelectedItem is User selectedUser)
            {
                txtName.Text = selectedUser.Name;
                txtRole.Text = selectedUser.Role;
                txtMonthPlan.Text = selectedUser.MonthPlan.ToString();
            }
            else
            {
                txtName.Clear();
                txtRole.Clear();
                txtMonthPlan.Clear();
            }
        }

        private void BtnSavePlan_Click(object sender, RoutedEventArgs e)
        {
            if (cmbUsers.SelectedItem == null)
            {
                MessageBox.Show("Выберите пользователя!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtMonthPlan.Text) || !decimal.TryParse(txtMonthPlan.Text, out decimal planValue))
            {
                MessageBox.Show("Введите корректное значение месячного плана (число)!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                txtMonthPlan.Focus();
                return;
            }

            try
            {
                var selectedUser = (User)cmbUsers.SelectedItem;

                using (MySqlConnection connection = new MySqlConnection(CH.GetConnectionString()))
                {
                    connection.Open();

                    string query = "UPDATE users SET monthplan = @monthplan WHERE login = @login";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@monthplan", planValue);
                        command.Parameters.AddWithValue("@login", selectedUser.Login);

                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Месячный план успешно обновлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            // Обновляем список пользователей
                            LoadUsers();
                        }
                        else
                        {
                            MessageBox.Show("Не удалось обновить месячный план.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Ошибка MySQL: {ex.Message}", "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class User
    {
        public string Name { get; set; }
        public string Login { get; set; }
        public string Role { get; set; }
        public decimal MonthPlan { get; set; }
    }
}