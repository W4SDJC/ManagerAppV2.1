using MySql.Data.MySqlClient;
using System.Windows;

namespace ManagerAppV3._5
{
    public partial class AddWarehouse : Window
    {
        // Замените эти параметры на свои
        ConnectHelper CH = new ConnectHelper();
        public AddWarehouse()
        {
            InitializeComponent();
        }

        private void BtnAddWarehouse_Click(object sender, RoutedEventArgs e)
        {
            // Проверка на пустые поля
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название склада!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                txtName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtAddress.Text))
            {
                MessageBox.Show("Введите адрес склада!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                txtAddress.Focus();
                return;
            }

            try
            {
                using (MySqlConnection connection = new MySqlConnection(CH.GetConnectionString()))
                {
                    connection.Open();

                    string query = "INSERT INTO warehouses (name, address) VALUES (@name, @address)";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@name", txtName.Text.Trim());
                        command.Parameters.AddWithValue("@address", txtAddress.Text.Trim());

                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Склад успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            // Очищаем поля после успешного добавления
                            txtName.Clear();
                            txtAddress.Clear();
                        }
                        else
                        {
                            MessageBox.Show("Не удалось добавить склад.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
}