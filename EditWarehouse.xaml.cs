using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace ManagerAppV2._1
{
    public partial class EditWarehouse : Window
    {
        ConnectHelper CH = new ConnectHelper();
        private List<Warehouse> warehouses = new List<Warehouse>();

        public EditWarehouse()
        {
            InitializeComponent();
            LoadWarehouses();
        }

        private void LoadWarehouses()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(CH.GetConnectionString()))
                {
                    connection.Open();

                    string query = "SELECT id, name, address FROM warehouses";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            warehouses.Clear();
                            while (reader.Read())
                            {
                                warehouses.Add(new Warehouse
                                {
                                    Id = reader.GetInt32("id"),
                                    Name = reader.GetString("name"),
                                    Address = reader.GetString("address")
                                });
                            }
                        }
                    }
                }

                cmbWarehouses.ItemsSource = warehouses;
                cmbWarehouses.SelectedIndex = -1;
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

        private void CmbWarehouses_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbWarehouses.SelectedItem is Warehouse selectedWarehouse)
            {
                txtAddress.Text = selectedWarehouse.Address;
            }
            else
            {
                txtAddress.Clear();
            }
        }

        private void BtnUpdateAddress_Click(object sender, RoutedEventArgs e)
        {
            if (cmbWarehouses.SelectedItem == null)
            {
                MessageBox.Show("Выберите склад для редактирования!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtAddress.Text))
            {
                MessageBox.Show("Введите новый адрес склада!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                txtAddress.Focus();
                return;
            }

            try
            {
                var selectedWarehouse = (Warehouse)cmbWarehouses.SelectedItem;

                using (MySqlConnection connection = new MySqlConnection(CH.GetConnectionString()))
                {
                    connection.Open();

                    string query = "UPDATE warehouses SET address = @address WHERE id = @id";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@address", txtAddress.Text.Trim());
                        command.Parameters.AddWithValue("@id", selectedWarehouse.Id);

                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Адрес склада успешно обновлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            // Обновляем список складов
                            LoadWarehouses();
                        }
                        else
                        {
                            MessageBox.Show("Не удалось обновить адрес склада.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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

    public class Warehouse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
    }
}