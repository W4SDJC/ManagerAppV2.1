using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Логика взаимодействия для AddnEditProduct.xaml
    /// </summary>
    public partial class AddnEditProduct : Window
    {
        ConnectHelper CH = new ConnectHelper();
        public AddnEditProduct(string mode)
        {
            InitializeComponent();
            FormMode(mode); 
        }

        private void FormMode(string mode)
        {
            if (mode == "Add")
            {
                SaveButton.Content = "Добавить";
                ProductCB.Visibility = Visibility.Collapsed;
                DeleteButton.Visibility = Visibility.Collapsed;
                LoadRoleComboBoxData();
            }
            else
            {
                SaveButton.Content = "Изменить";
                //NameTextBox.Visibility = Visibility.Collapsed;
                ProductCB.Visibility = Visibility.Visible;
                DeleteButton.Visibility = Visibility.Visible;
                LoadComboBoxDataAsync();
                LoadRoleComboBoxData();
            }

        }
        private async void ProductCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                // Проверяем, что есть выбранный элемент и это строка
                if (ProductCB.SelectedItem is string selectedLogin && !string.IsNullOrWhiteSpace(selectedLogin))
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
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async Task LoadComboBoxDataAsync()
        {
            const string query = "SELECT Product_name FROM `product price` WHERE id != 0 ORDER BY Product_name;"; // Добавлена сортировка

            try
            {
                // Показываем индикатор загрузки
                ProductCB.IsEnabled = false;
                ProductCB.DisplayMemberPath = null;
                ProductCB.ItemsSource = new List<string> { "Загрузка..." };

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
                    ProductCB.ItemsSource = items;
                    ProductCB.DisplayMemberPath = ".";
                    ProductCB.SelectedIndex = 0;
                }
                else
                {
                    ProductCB.ItemsSource = new List<string> { "Нет данных" };
                    ProductCB.SelectedIndex = -1;
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

                ProductCB.ItemsSource = new List<string> { "Ошибка загрузки" };
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
                ProductCB.IsEnabled = true;
            }
        }
        private void LoadRoleComboBoxData()
        {

            string query = "SELECT DISTINCT role FROM `roles` WHERE id != 0;"; // DISTINCT для уникальных значений

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

                // Привязка данных к ComboBox
                RoleComboBox.ItemsSource = items;

                RoleComboBox.SelectedIndex = 1;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Ошибка MySQL: {ex.Message}");
            }
        }
        private async Task LoadUserDataAsync(string login)
        {
            const string query = @"
                SELECT Product_name, Product_price, Minimum_price, Unit_of_measurement 
                FROM `product price` 
                WHERE Product_name = @Product_name 
                LIMIT 1";

            try
            {
                using var conn = new MySqlConnection(CH.GetConnectionString());
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Product_name", login);

                await conn.OpenAsync();

                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    // Безопасное чтение данных
                    NameTextBox.Text = reader["Product_name"]?.ToString() ?? "";
                    PriceTextBox.Text = reader["Product_price"]?.ToString() ?? "";
                    MinPriceTextBox.Text = reader["Minimum_price"]?.ToString() ?? "";
                    UnitTextBox.Text = reader["Unit_of_measurement"]?.ToString() ?? "";


                    // Установка значения в ComboBox ролей
                    //var role = reader["role"]?.ToString();
                    //if (!string.IsNullOrEmpty(role))
                    //{
                    //    RoleComboBox.SelectedItem = role;
                    //}
                    //else
                    //{
                    //    RoleComboBox.SelectedIndex = -1;
                    //}
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
        private void ClearUserForm()
        {
            UnitTextBox.Text = "";
            MinPriceTextBox.Text = "";
            //RoleComboBox.SelectedIndex = -1;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string id;
            using (MySqlConnection con = new MySqlConnection(CH.GetConnectionString()))
            {
                con.Open();
                string getID = $"SELECT id FROM `product price` where Product_name = '{ProductCB.SelectedItem.ToString()}';";
                MySqlCommand IDGetter = new MySqlCommand(getID, con);
                id = IDGetter.ExecuteScalar().ToString();
            }
            if (SaveButton.Content == "Добавить")
            {
                string query =
                $"INSERT INTO `product price` " +
                $"(" +
                $"Product_name," +
                $"Product_price," +
                $"Minimum_price," +
                $"Unit_of_measurement," +
                $"Role" +
                $") VALUES ( " +
                $"@Pname, @Pprice, @MinPrice, @UOM, @Role)";
                try
                {
                    using (MySqlConnection connection = new MySqlConnection(CH.GetConnectionString()))
                    {
                        connection.Open();

                        using (MySqlCommand command = new MySqlCommand(query, connection))
                        {
                            // Добавляем параметры
                            command.Parameters.AddWithValue("@Pname", NameTextBox.Text);
                            command.Parameters.AddWithValue("@Pprice", PriceTextBox.Text);
                            command.Parameters.AddWithValue("@MinPrice", MinPriceTextBox.Text);
                            command.Parameters.AddWithValue("@UOM", UnitTextBox.Text);
                            command.Parameters.AddWithValue("@Role", RoleComboBox.SelectedItem);

                            int result = command.ExecuteNonQuery();

                            if (result > 0)
                            {
                                MessageBox.Show("Товар успешно добавлен!");
                                //ClearUserForm();
                            }
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show($"Ошибка при создании элементов:\n{ex.Message}", "Ошибка",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
            else
            {
                string query = $"UPDATE `product price` SET Product_name = @Pname, Product_price = @Pprice, Minimum_price = @MinPrice, Unit_of_measurement = @UOM, Role = @Role WHERE id = { id }";
                try
                {
                    using (MySqlConnection connection = new MySqlConnection(CH.GetConnectionString()))
                    {
                        connection.Open();

                        using (MySqlCommand command = new MySqlCommand(query, connection))
                        {
                            // Добавляем параметры
                            command.Parameters.AddWithValue("@Pname", NameTextBox.Text);
                            command.Parameters.AddWithValue("@Pprice", PriceTextBox.Text);
                            command.Parameters.AddWithValue("@MinPrice", MinPriceTextBox.Text);
                            command.Parameters.AddWithValue("@UOM", UnitTextBox.Text);
                            command.Parameters.AddWithValue("@Role", RoleComboBox.SelectedItem);

                            int result = command.ExecuteNonQuery();

                            if (result > 0)
                            {
                                MessageBox.Show("Товар успешно изменен!");
                                //ClearUserForm();
                            }
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show($"Ошибка при создании элементов:\n{ex.Message}", "Ошибка",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }

        }


    }
}
