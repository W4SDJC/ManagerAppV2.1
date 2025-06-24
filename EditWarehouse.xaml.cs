using MySql.Data.MySqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ManagerAppV4._0
{
    public partial class EditWarehouse : Window
    {
        private readonly ConnectHelper _connectHelper = new ConnectHelper();
        private List<Warehouse> _warehouses = new List<Warehouse>();
        public class Warehouse
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Address { get; set; }
        }
        public EditWarehouse()
        {
            InitializeComponent();
            LoadData();
        }

        #region Data Loading
        private void LoadData()
        {
            try
            {
                LoadWarehousesFromDatabase();
                InitializeComboBox();
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Ошибка при загрузке данных", ex);
            }
        }

        private void LoadWarehousesFromDatabase()
        {
            _warehouses.Clear();

            using (var connection = new MySqlConnection(_connectHelper.GetConnectionString()))
            {
                connection.Open();

                const string query = "SELECT id, name, address FROM warehouses";
                using (var command = new MySqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        _warehouses.Add(new Warehouse
                        {
                            Id = reader.GetInt32("id"),
                            Name = reader.GetString("name"),
                            Address = reader.GetString("address")
                        });
                    }
                }
            }
        }

        private void InitializeComboBox()
        {
            cmbWarehouses.ItemsSource = _warehouses;
            cmbWarehouses.DisplayMemberPath = "Name";
            cmbWarehouses.SelectedIndex = -1;
        }
        #endregion

        #region Event Handlers
        private void CmbWarehouses_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbWarehouses.SelectedItem is Warehouse selectedWarehouse)
            {
                // Заполняем оба текстовых поля при выборе склада
                WarehouseNameTextBox.Text = selectedWarehouse.Name;
                txtAddress.Text = selectedWarehouse.Address;
            }
            else
            {
                // Очищаем поля, если ничего не выбрано
                WarehouseNameTextBox.Clear();
                txtAddress.Clear();
            }
        }

        private void BtnUpdateAddress_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
                return;

            try
            {
                var selectedWarehouse = (Warehouse)cmbWarehouses.SelectedItem;
                var newName = WarehouseNameTextBox.Text.Trim();
                var newAddress = txtAddress.Text.Trim();

                if (UpdateWarehouse(selectedWarehouse.Id, newName, newAddress))
                {
                    ShowSuccessMessage("Данные склада успешно обновлены!");
                    RefreshData();
                    ResetForm();
                }
                else
                {
                    ShowErrorMessage("Не удалось обновить данные склада");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Ошибка при обновлении данных склада", ex);
            }
        }
        #endregion

        #region Validation Logic
        private bool ValidateInput()
        {
            if (cmbWarehouses.SelectedItem == null)
            {
                ShowWarningMessage("Выберите склад для редактирования!");
                return false;
            }

            if (string.IsNullOrWhiteSpace(WarehouseNameTextBox.Text))
            {
                ShowWarningMessage("Введите название склада!");
                WarehouseNameTextBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtAddress.Text))
            {
                ShowWarningMessage("Введите адрес склада!");
                txtAddress.Focus();
                return false;
            }

            return true;
        }

        private bool UpdateWarehouse(int warehouseId, string newName, string newAddress)
        {
            using (var connection = new MySqlConnection(_connectHelper.GetConnectionString()))
            {
                connection.Open();

                const string query = "UPDATE warehouses SET name = @name, address = @address WHERE id = @id";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", newName);
                    command.Parameters.AddWithValue("@address", newAddress);
                    command.Parameters.AddWithValue("@id", warehouseId);

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        private void RefreshData()
        {
            LoadWarehousesFromDatabase();
            cmbWarehouses.Items.Refresh();
        }

        private void ResetForm()
        {
            cmbWarehouses.SelectedIndex = -1;
            WarehouseNameTextBox.Clear();
            txtAddress.Clear();
        }
        #endregion

        #region Message Helpers
        private void ShowSuccessMessage(string message)
        {
            MessageBox.Show(message, "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowWarningMessage(string message)
        {
            MessageBox.Show(message, "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void ShowErrorMessage(string message, Exception ex = null)
        {
            var fullMessage = ex != null ? $"{message}: {ex.Message}" : message;
            MessageBox.Show(fullMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        #endregion

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (cmbWarehouses.SelectedItem == null)
            {
                ShowWarningMessage("Выберите склад для удаления!");
                return;
            }

            var selectedWarehouse = (Warehouse)cmbWarehouses.SelectedItem;

            // Подтверждение удаления
            var confirmResult = MessageBox.Show(
                $"Вы уверены, что хотите удалить склад '{selectedWarehouse.Name}'?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmResult != MessageBoxResult.Yes)
                return;

            try
            {
                bool deletionResult = DeleteWarehouseFromDatabase(selectedWarehouse.Id);

                if (deletionResult)
                {
                    ShowSuccessMessage($"Склад '{selectedWarehouse.Name}' успешно удален!");
                    RefreshData();
                    ResetForm();
                }
                else
                {
                    ShowErrorMessage("Не удалось удалить склад. Возможно, он уже был удален.");
                }
            }
            catch (MySqlException ex) when (ex.Number == 1451) // Ошибка внешнего ключа
            {
                ShowErrorMessage("Невозможно удалить склад: существуют связанные записи в других таблицах.");
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Ошибка при удалении склада", ex);
            }
        }

        private bool DeleteWarehouseFromDatabase(int warehouseId)
        {
            using (var connection = new MySqlConnection(_connectHelper.GetConnectionString()))
            {
                connection.Open();

                const string query = "DELETE FROM warehouses WHERE id = @id";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", warehouseId);
                    return command.ExecuteNonQuery() > 0;
                }
            }
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