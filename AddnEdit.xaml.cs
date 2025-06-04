using Kursovaya2;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
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
    /// Логика взаимодействия для AddnEdit.xaml
    /// </summary>
    public partial class AddnEdit : Window
    {
        MainWindow MW = new MainWindow();
        ConnectHelper CH = new ConnectHelper();
        public AddnEdit(string Mode)
        {
            InitializeComponent();
            FormMode(Mode);
            LoadComboBoxData();
        }

        private void LoadComboBoxData()
        {

            string connectionString = CH.GetConnectionString();
            string query = "SELECT Product_name FROM `product price`;"; // DISTINCT для уникальных значений
            List<string> items = new List<string>();
            string query2 = "SELECT name FROM warehouse;;"; // DISTINCT для уникальных значений
            List<string> items2 = new List<string>();

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
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
                    ProductCB.ItemsSource = items;
                    ProductCB.SelectedIndex = 0;
                    using (MySqlCommand command = new MySqlCommand(query2, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (!reader.IsDBNull(0))
                                {
                                    items2.Add(reader.GetString(0));
                                }
                            }
                        }
                    }
                    WarehouseCB.ItemsSource = items2;
                    WarehouseCB.SelectedIndex = 0;
                }

                // Привязка данных к ComboBox



            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Ошибка MySQL: {ex.Message}");
            }
        }

        public void FormMode(string Mode)
        {

            if (Mode == "Add")
            {
                MainWindow.Title = "Add";
                AddnSaveTextBlock.Text = "Add";
            }
            else
            {
                AddnSaveTextBlock.Text = "Save";
            }
        }
        private void AddShipment_Click(object sender, RoutedEventArgs e)
        {
            if (AddnSaveTextBlock.Text == "Add")
            {
                // Проверка обязательных полей
                if (!ValidateInput()) return;

                // SQL запрос с параметрами
                string query = @"INSERT INTO manager (
                            ShipmentDate, 
                            ShipmentWarehouse, 
                            `ClientCity`, 
                            `ClientName`, 
                            ProductName, 
                            `ProductAmount`, 
                            UnitOfMeasurement, 
                            Price, 
                            MinimumPrice, 
                            ShipmentValue, 
                            `ShipmentValue(Minimum_price)`, 
                            Reward, 
                            UPDNumber, 
                            ShipmentPrice
                        ) VALUES (
                            @date, @warehouse, @city, @clientName, @productName, 
                            @amount, @unit, @price, @minPrice, 
                            @value, @minValue, @reward, @updNumber, @shipmentPrice
                        )";

                try
                {
                    // Парсим и вычисляем значения
                    DateTime shipmentDate = DateTime.ParseExact(shipmentDateTextBox.Text,"dd.MM.yyyy",CultureInfo.InvariantCulture);
                    decimal price = decimal.Parse(priceTextBox.Text);
                    decimal minPrice = decimal.Parse(minPriceTextBox.Text);
                    decimal amount = decimal.Parse(amountTextBox.Text);
                    decimal shipmentValue = price * amount;
                    decimal minShipmentValue = minPrice * amount;
                    decimal reward = shipmentValue - minShipmentValue;
                    decimal shipmentPrice = decimal.Parse(shipmentPriceTextBox.Text);

                    using (MySqlConnection connection = new MySqlConnection(CH.GetConnectionString()))
                    {
                        connection.Open();

                        using (MySqlCommand command = new MySqlCommand(query, connection))
                        {
                            // Добавляем параметры
                            command.Parameters.AddWithValue("@date", shipmentDate);
                            command.Parameters.AddWithValue("@warehouse", WarehouseCB.SelectedItem);
                            command.Parameters.AddWithValue("@city", cityTextBox.Text);
                            command.Parameters.AddWithValue("@clientName", clientNameTextBox.Text);
                            command.Parameters.AddWithValue("@productName", ProductCB.SelectedItem);
                            command.Parameters.AddWithValue("@amount", amount);
                            command.Parameters.AddWithValue("@unit", unitTextBox.Text);
                            command.Parameters.AddWithValue("@price", price);
                            command.Parameters.AddWithValue("@minPrice", minPrice);
                            command.Parameters.AddWithValue("@value", shipmentValue);
                            command.Parameters.AddWithValue("@minValue", minShipmentValue);
                            command.Parameters.AddWithValue("@reward", reward);
                            command.Parameters.AddWithValue("@updNumber", updNumberTextBox.Text);
                            command.Parameters.AddWithValue("@shipmentPrice", shipmentPrice);

                            int result = command.ExecuteNonQuery();

                            if (result > 0)
                            {
                                MessageBox.Show("Отгрузка успешно добавлена!");
                                MW.ReLoadData(DataSource.DBname);
                                ClearForm();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при добавлении отгрузки: {ex.Message}");
                }
            }
            else
            {

            }
        }

        private bool ValidateInput()
        {
            // Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(shipmentDateTextBox.Text) ||
                string.IsNullOrWhiteSpace(cityTextBox.Text) ||
                string.IsNullOrWhiteSpace(clientNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(amountTextBox.Text) ||
                string.IsNullOrWhiteSpace(unitTextBox.Text) ||
                string.IsNullOrWhiteSpace(priceTextBox.Text) ||
                string.IsNullOrWhiteSpace(minPriceTextBox.Text) ||
                string.IsNullOrWhiteSpace(shipmentPriceTextBox.Text))
            {
                MessageBox.Show("Заполните все обязательные поля!");
                return false;
            }

            // Проверка числовых значений
            if (!DateTime.TryParse(shipmentDateTextBox.Text, out _))
            {
                MessageBox.Show("Введите корректную дату в формате DD-MM-YYYY!");
                shipmentDateTextBox.Focus();
                return false;
            }

            if (!decimal.TryParse(amountTextBox.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Количество должно быть положительным числом!");
                amountTextBox.Focus();
                return false;
            }

            if (!decimal.TryParse(priceTextBox.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Цена должна быть положительным числом!");
                priceTextBox.Focus();
                return false;
            }

            if (!decimal.TryParse(minPriceTextBox.Text, out decimal minPrice) || minPrice <= 0)
            {
                MessageBox.Show("Минимальная цена должна быть положительным числом!");
                minPriceTextBox.Focus();
                return false;
            }

            if (!decimal.TryParse(shipmentPriceTextBox.Text, out decimal shipmentPrice) || shipmentPrice <= 0)
            {
                MessageBox.Show("Цена отгрузки должна быть положительным числом!");
                shipmentPriceTextBox.Focus();
                return false;
            }

            return true;
        }

        private void ClearForm()
        {
            shipmentDateTextBox.Text = "";
            cityTextBox.Text = "";
            clientNameTextBox.Text = "";
            amountTextBox.Text = "";
            unitTextBox.Text = "";
            priceTextBox.Text = "";
            minPriceTextBox.Text = "";
            updNumberTextBox.Text = "";
            shipmentPriceTextBox.Text = "";
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ProductCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string connectionString = CH.GetConnectionString();
            string query = $"SELECT Product_price FROM `product price` where Product_name = \"{ProductCB.SelectedItem}\";"; 
            string query2 = $"SELECT Unit_of_measurement FROM `product price` where Product_name = \"{ProductCB.SelectedItem}\";"; 


            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    MySqlCommand command = new MySqlCommand(query, connection);
                    object Price= command.ExecuteScalar();
                    priceTextBox.Text = Price.ToString();
                    MySqlCommand command2 = new MySqlCommand(query2, connection);
                    object UofM= command2.ExecuteScalar();
                    unitTextBox.Text = UofM.ToString();

                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Ошибка MySQL: {ex.Message}");
            }
        }
    }
}
