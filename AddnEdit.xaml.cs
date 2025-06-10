using MySql.Data.MySqlClient;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace ManagerAppV2._1
{

    public partial class AddnEdit : Window
    {
        public string db = DataSource.DBname;
        public string ID;
        public bool AdminMode = false;

        MainWindow MW = new MainWindow();
        ConnectHelper CH = new ConnectHelper();
        public AddnEdit(string Mode, string database = null, DataRow data = null, string id = null, bool adminmode = false)
        {
            InitializeComponent();
            FormMode(Mode, database, data, id, adminmode);
            LoadComboBoxData();
        }
        public void FormMode(string Mode, string dbase, DataRow data = null, string id = null, bool adminmode = false)
        {
            AdminMode = adminmode;
            if (Mode == "Add")
            {
                MainWindow.Title = "Добавить";
                AddnSaveTextBlock.Text = "Добавить";
                db = dbase;

            }
            else
            {
                MainWindow.Title = "Изменить";
                mainLabel.Text = "Изменение существующей отгрузки";
                AddnSaveTextBlock.Text = "Сохранить";
                ID = id;
                if (id == null) { fill(data); }
                else { FillbyID(dbase, id); }
            }
        }
        private void LoadComboBoxData()
        {

            string connectionString = CH.GetConnectionString();
            string query = "SELECT Product_name FROM `product price`;"; // DISTINCT для уникальных значений
            List<string> items = new List<string>();
            string query2 = "SELECT name FROM warehouses;"; // DISTINCT для уникальных значений
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

        // ============================== ADD MODE ==============================

        private void AddShipment(string db, string id, bool adminmode)
        {
            if (adminmode)
            {
                if (AddnSaveTextBlock.Text == "Добавить")
                {
                    // Проверка обязательных полей
                    if (!ValidateInput()) return;

                    // SQL запрос с параметрами
                    if (MW.AdminTabControl.SelectedItem is TabItem selectedTab)
                    {
                        string query = @$"INSERT INTO `{db}` (
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
                            DateTime shipmentDate = DateTime.ParseExact(shipmentDateTextBox.Text, "dd.MM.yyyy", CultureInfo.InvariantCulture);
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
                }
                else
                {
                    // Проверка обязательных полей
                    if (!ValidateInput()) return;

                    // SQL запрос с параметрами
                    if (MW.AdminTabControl.SelectedItem is TabItem selectedTab)
                    {
                        string query = @$"UPDATE `{db}` SET
                            ShipmentDate = @date, 
                            ShipmentWarehouse = @warehouse, 
                            `ClientCity` = @city, 
                            `ClientName` = @clientName, 
                            ProductName = @productName, 
                            `ProductAmount` = @amount, 
                            UnitOfMeasurement = @unit, 
                            Price = @price, 
                            MinimumPrice =  @minPrice, 
                            ShipmentValue = @value, 
                            `ShipmentValue(Minimum_price)` = @minValue, 
                            Reward = @reward, 
                            UPDNumber = @updNumber, 
                            ShipmentPrice = @shipmentPrice
                         WHERE id = {id}";

                        try
                        {
                            // Парсим и вычисляем значения
                            DateTime shipmentDate = DateTime.ParseExact(shipmentDateTextBox.Text, "dd.MM.yyyy", CultureInfo.InvariantCulture);
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

                }
            }
            else
            {
                if (AddnSaveTextBlock.Text == "Добавить")
                {

                    // Проверка обязательных полей
                    if (!ValidateInput()) return;

                    // SQL запрос с параметрами
                    
                    string query = @$"INSERT INTO `{db}` (
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
                        DateTime shipmentDate = DateTime.ParseExact(shipmentDateTextBox.Text, "dd.MM.yyyy", CultureInfo.InvariantCulture);
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
                    // Проверка обязательных полей
                    if (!ValidateInput()) return;

                    // SQL запрос с параметрами

                    string query = @$"UPDATE `{db}` SET
                            ShipmentDate = @date, 
                            ShipmentWarehouse = @warehouse, 
                            `ClientCity` = @city, 
                            `ClientName` = @clientName, 
                            ProductName = @productName, 
                            `ProductAmount` = @amount, 
                            UnitOfMeasurement = @unit, 
                            Price = @price, 
                            MinimumPrice =  @minPrice, 
                            ShipmentValue = @value, 
                            `ShipmentValue(Minimum_price)` = @minValue, 
                            Reward = @reward, 
                            UPDNumber = @updNumber, 
                            ShipmentPrice = @shipmentPrice
                         WHERE id = {id}";

                        try
                        {
                            // Парсим и вычисляем значения
                            DateTime shipmentDate = DateTime.ParseExact(shipmentDateTextBox.Text, "dd.MM.yyyy", CultureInfo.InvariantCulture);
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
                                        this.Close();
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка при добавлении отгрузки: {ex.Message}");
                        }
                    }

                }
            }
        

        private void AddShipment_Click(object sender, RoutedEventArgs e)
        {
            AddShipment(db,ID,AdminMode);
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ProductCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string query = $"SELECT Product_price FROM `product price` where Product_name = \"{ProductCB.SelectedItem}\";"; 
            string query2 = $"SELECT Unit_of_measurement FROM `product price` where Product_name = \"{ProductCB.SelectedItem}\";"; 

            try
            {
                using (MySqlConnection connection = new MySqlConnection(CH.GetConnectionString()))
                {
                    connection.Open();
                    MySqlCommand command = new MySqlCommand(query, connection);
                    object Price= command.ExecuteScalar();
                    priceTextBox.Text = Price?.ToString() ?? string.Empty;
                    MySqlCommand command2 = new MySqlCommand(query2, connection);
                    object UofM= command2.ExecuteScalar();
                    unitTextBox.Text = UofM.ToString() ?? string.Empty;

                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Ошибка MySQL: {ex.Message}");
            }
        }

        // ============================= EDIT MODE =============================

        public void fill(DataRow data)
        {
            InitializeComponent();

            // Заполняем поля
            shipmentDateTextBox.Text = DateTime.TryParse(data["ShipmentDate"]?.ToString(), out var dt)? dt.ToString("dd.MM.yyyy"): "";
            WarehouseCB.Text = data["ShipmentWarehouse"]?.ToString();
            cityTextBox.Text = data["ClientCity"]?.ToString();
            clientNameTextBox.Text = data["ClientName"]?.ToString();
            ProductCB.Text = data["ProductName"]?.ToString();
            amountTextBox.Text = data["ProductAmount"]?.ToString();
            unitTextBox.Text = data["UnitOfMeasurement"]?.ToString();
            priceTextBox.Text = data["Price"]?.ToString();
            minPriceTextBox.Text = data["MinimumPrice"]?.ToString();
            updNumberTextBox.Text = data["UPDNumber"]?.ToString();
            shipmentPriceTextBox.Text = data["ShipmentValue"]?.ToString();
        }

        private void FillbyID(string table, string id)
        {
            string connStr = CH.GetConnectionString();
            string query = $"SELECT ShipmentDate, ShipmentWarehouse, ClientCity, ClientName, ProductName, ProductAmount, UnitOfMeasurement, Price, MinimumPrice, UPDNumber, ShipmentValue FROM {table} WHERE id = @id";


            using var conn = new MySqlConnection(connStr);
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);

            conn.Open();
            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {


                shipmentDateTextBox.Text = DateTime.TryParse(reader["ShipmentDate"]?.ToString(), out var dt) ? dt.ToString("dd.MM.yyyy") : "";
                WarehouseCB.SelectedItem = reader["ShipmentWarehouse"]?.ToString();
                cityTextBox.Text = reader["ClientCity"]?.ToString();
                clientNameTextBox.Text = reader["ClientName"]?.ToString();
                ProductCB.Text = reader["ProductName"]?.ToString();
                amountTextBox.Text = reader["ProductAmount"]?.ToString();
                unitTextBox.Text = reader["UnitOfMeasurement"]?.ToString();
                priceTextBox.Text = reader["Price"]?.ToString();
                minPriceTextBox.Text = reader["MinimumPrice"]?.ToString();
                updNumberTextBox.Text = reader["UPDNumber"]?.ToString();
                shipmentPriceTextBox.Text = reader["ShipmentValue"]?.ToString();
            }
        }


        // ========================== SYSTEM FUNCTIONS ==========================
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
    }
}
