using MySql.Data.MySqlClient;
using System.Collections;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace ManagerAppV2._1
{

    public partial class MainWindow : Window
    {
        ConnectHelper CH = new ConnectHelper();
        private string Name = DataSource.UserName;
        private string Login = DataSource.Login;
        private string Role = DataSource.Role;
        private string DBname = DataSource.DBname;
        private bool AdminMode = false;
        private bool _isMouseOverDataGrid = false;
        private bool _isCtrlPressed = false;

        public MainWindow()
        {
            InitializeComponent();
            MainLoad();
        }


        // =========================== DATABASE MENU ===========================
        private void DatabaseMenuOpen(object sender, RoutedEventArgs e)
        {
            if (((int)DatabaseMenu.ActualHeight) > 0)
            {
                DoubleAnimation DatabaseBtnAnimation = new DoubleAnimation();
                DatabaseBtnAnimation.From = DatabaseMenu.ActualHeight;
                DatabaseBtnAnimation.To = 0;
                DatabaseBtnAnimation.Duration = TimeSpan.FromSeconds(0.2);
                DatabaseMenu.BeginAnimation(Grid.HeightProperty, DatabaseBtnAnimation);
                BitmapImage bi = new BitmapImage();
                // BitmapImage.UriSource must be in a BeginInit/EndInit block.
                bi.BeginInit();
                bi.UriSource = new Uri(@"/Icons/ArrowDown.png", UriKind.RelativeOrAbsolute);
                bi.EndInit();
                // Set the image source.
                DataBaseBtnimg.Source = bi;
            }
            else
            {
                DoubleAnimation DatabaseBtnAnimation = new DoubleAnimation();
                DatabaseBtnAnimation.From = DatabaseMenu.ActualHeight;
                DatabaseBtnAnimation.To = 155;
                DatabaseBtnAnimation.Duration = TimeSpan.FromSeconds(0.2);
                DatabaseMenu.BeginAnimation(Grid.HeightProperty, DatabaseBtnAnimation);
                BitmapImage bi = new BitmapImage();
                // BitmapImage.UriSource must be in a BeginInit/EndInit block.
                bi.BeginInit();
                bi.UriSource = new Uri(@"/Icons/ArrowUp.png", UriKind.RelativeOrAbsolute);
                bi.EndInit();
                // Set the image source.
                DataBaseBtnimg.Source = bi;
            }
        }

        // ======================== ADD DATA TO DATABASE ========================
        public void AddData_Click(object sender, RoutedEventArgs e)
        {
            if (AdminMode)
            {

                AddnEdit addnEdit = new AddnEdit("Add", GetTabName());
                addnEdit.ShowDialog();
            }
            else
            {
                AddnEdit addnEdit = new AddnEdit("Add",DBname);
                addnEdit.ShowDialog();
            }
            ReLoadData(DBname);
            ApplyColumnVisibility();
        }

        // ============================= EDIT DATA =============================
        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            if (AdminMode)
            {
                TabItem selectedTab = AdminTabControl.SelectedItem as TabItem;
                if (selectedTab?.Content is DataGrid dataGrid && dataGrid.SelectedItem is DataRowView row)
                {
                    var selectedData = row.Row; // DataRow с доступом по именам колонок
                    var editWindow = new AddnEdit("Edit", GetTabName(), row.Row, null, true); // передаём DataRow
                    editWindow.ShowDialog();
                }

            }
            else
            {
                if (MainDataGrid.SelectedItem == null)
                {
                    MessageBox.Show("Выберите строку для изменения");
                    return;
                }
                MessageBox.Show(AdminMode.ToString());
                DataRowView selectedRow = (DataRowView)MainDataGrid.SelectedItem;
                string id = (selectedRow["id"]).ToString();

                var editWindow = new AddnEdit("Edit",DBname, null, id); // передаём DataRow
                editWindow.ShowDialog();
                ReLoadData(DBname);
            }
        }
        // ============================ DELETE DATA ============================
        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Role == "Admin" || Role == "Developer")
            {
                AdminDelete();
                // Получаем текущую вкладку
                var currentTab = AdminTabControl.SelectedItem as TabItem;

                // Ищем DataGrid внутри вкладки
                if (currentTab?.Content is StackPanel panel)
                {
                    var dataGrid = panel.Children.OfType<DataGrid>().FirstOrDefault();

                    if (dataGrid?.SelectedItem != null && dataGrid.ItemsSource is IList list)
                    {
                        list.Remove(dataGrid.SelectedItem);
                    }
                }
            }
            else
            {
                DeleteData();
            }
        }
        private void DatabaseMenuClose()
        {
            DoubleAnimation DatabaseBtnAnimation = new DoubleAnimation();
            DatabaseBtnAnimation.From = DatabaseMenu.ActualHeight;
            DatabaseBtnAnimation.To = 0;
            DatabaseBtnAnimation.Duration = TimeSpan.FromSeconds(0.2);
            DatabaseMenu.BeginAnimation(Grid.HeightProperty, DatabaseBtnAnimation);
            BitmapImage bi = new BitmapImage();
            // BitmapImage.UriSource must be in a BeginInit/EndInit block.
            bi.BeginInit();
            bi.UriSource = new Uri(@"/Icons/ArrowDown.png", UriKind.RelativeOrAbsolute);
            bi.EndInit();
            // Set the image source.
            DataBaseBtnimg.Source = bi;
        }
        // ========================= DATABASE MENU END =========================

        // =========================== UPDATE BUTTON ===========================
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            ReLoadData(DBname, AdminMode);
            ApplyColumnVisibility();
            LoadDataToLabel();
        }

        // ============================= EDIT MENU =============================

        private void ManagersButton_Click(object sender, RoutedEventArgs e)
        {
            if (((int)EditMenu.ActualHeight) > 0)
            {
                DoubleAnimation ManagersBtnAnimation = new DoubleAnimation();
                ManagersBtnAnimation.From = EditMenu.ActualHeight;
                ManagersBtnAnimation.To = 0;
                ManagersBtnAnimation.Duration = TimeSpan.FromSeconds(0.2);
                EditMenu.BeginAnimation(Grid.HeightProperty, ManagersBtnAnimation);
            }
            else
            {
                DoubleAnimation ManagersBtnAnimation = new DoubleAnimation();
                ManagersBtnAnimation.From = EditMenu.ActualHeight;
                ManagersBtnAnimation.To = 155;
                ManagersBtnAnimation.Duration = TimeSpan.FromSeconds(0.2);
                EditMenu.BeginAnimation(Grid.HeightProperty, ManagersBtnAnimation);
            }
        }

        // ============================= USER MENU =============================
        private void UsersMenu_Click(object sender, RoutedEventArgs e)
        {
            if (((int)UserMenu.ActualHeight) > 0)
            {
                DoubleAnimation UserBtnAnimation = new DoubleAnimation();
                UserBtnAnimation.From = UserMenu.ActualHeight;
                UserBtnAnimation.To = 0;
                UserBtnAnimation.Duration = TimeSpan.FromSeconds(0.2);
                UserMenu.BeginAnimation(Grid.HeightProperty, UserBtnAnimation);
            }
            else
            {
                ProductMenuClose();
                WarehouseMenuClose();
                DoubleAnimation UserBtnAnimation = new DoubleAnimation();
                UserBtnAnimation.From = UserMenu.ActualHeight;
                UserBtnAnimation.To = 65;
                UserBtnAnimation.Duration = TimeSpan.FromSeconds(0.2);
                UserMenu.BeginAnimation(Grid.HeightProperty, UserBtnAnimation);
            }
        }
        // ============================ CREATE USER ============================
        
        private void AddUserBtn_Click(object sender, RoutedEventArgs e)
        {
            AddUser ad = new AddUser();
            ad.ShowDialog();
        }
        // ============================= EDIT USER =============================
        private void EditUserBtn_Click(object sender, RoutedEventArgs e)
        {
            EditUser EU = new EditUser();
            EU.Show();
        }
        // =========================== USER MENU END ===========================

        private void UserMenuClose()
        {
            DoubleAnimation UserBtnAnimation = new DoubleAnimation();
            UserBtnAnimation.From = UserMenu.ActualHeight;
            UserBtnAnimation.To = 0;
            UserBtnAnimation.Duration = TimeSpan.FromSeconds(0.2);
            UserMenu.BeginAnimation(Grid.HeightProperty, UserBtnAnimation);
        }
        // ============================ PRODUCT MENU ============================
        private void ProductMenu_Click(object sender, RoutedEventArgs e)
        {
            if (((int)ProductMenu.ActualHeight) > 0)
            {
                DoubleAnimation ProductBtnAnimation = new DoubleAnimation();
                ProductBtnAnimation.From = ProductMenu.ActualHeight;
                ProductBtnAnimation.To = 0;
                ProductBtnAnimation.Duration = TimeSpan.FromSeconds(0.2);
                ProductMenu.BeginAnimation(Grid.HeightProperty, ProductBtnAnimation);
            }
            else
            {
                UserMenuClose();
                WarehouseMenuClose();
                DoubleAnimation ProductBtnAnimation = new DoubleAnimation();
                ProductBtnAnimation.From = ProductMenu.ActualHeight;
                ProductBtnAnimation.To = 65;
                ProductBtnAnimation.Duration = TimeSpan.FromSeconds(0.2);
                ProductMenu.BeginAnimation(Grid.HeightProperty, ProductBtnAnimation);
            }
        }
        // ============================ ADD PRODUCT ============================
        private void CreateProduct_Click(object sender, RoutedEventArgs e)
        {
            AddnEditProduct AEP = new AddnEditProduct("Add");
            AEP.ShowDialog();
        }
        // ============================ EDIT PRODUCT ============================
        private void EditProductButton_Click(object sender, RoutedEventArgs e)
        {
            AddnEditProduct AEP = new AddnEditProduct("Edit");
            AEP.ShowDialog();
        }

        private void ProductMenuClose()
        {
            DoubleAnimation ProductBtnAnimation = new DoubleAnimation();
            ProductBtnAnimation.From = ProductMenu.ActualHeight;
            ProductBtnAnimation.To = 0;
            ProductBtnAnimation.Duration = TimeSpan.FromSeconds(0.2);
            ProductMenu.BeginAnimation(Grid.HeightProperty, ProductBtnAnimation);
        }
        // ========================== PRODUCT MENU END ==========================


        // =========================== WAREHOUSE MENU ===========================
        private void WarehouseMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            if (((int)WarehouseMenu.ActualHeight) > 0)
            {
                DoubleAnimation WarehouseBtnAnimation = new DoubleAnimation();
                WarehouseBtnAnimation.From = WarehouseMenu.ActualHeight;
                WarehouseBtnAnimation.To = 0;
                WarehouseBtnAnimation.Duration = TimeSpan.FromSeconds(0.2);
                WarehouseMenu.BeginAnimation(Grid.HeightProperty, WarehouseBtnAnimation);
            }
            else
            {
                UserMenuClose();
                ProductMenuClose();
                DoubleAnimation WarehouseBtnAnimation = new DoubleAnimation();
                WarehouseBtnAnimation.From = WarehouseMenu.ActualHeight;
                WarehouseBtnAnimation.To = 65;
                WarehouseBtnAnimation.Duration = TimeSpan.FromSeconds(0.2);
                WarehouseMenu.BeginAnimation(Grid.HeightProperty, WarehouseBtnAnimation);
            }
        }
        // =========================== ADD WAREHOUSE ===========================
        private void AddWarehouse_Click(object sender, RoutedEventArgs e)
        {
            AddWarehouse addWarehouse = new AddWarehouse();
            addWarehouse.Show();
        }
        // =========================== EDIT WAREHOUSE ===========================
        private void EditWarehouse_Click(object sender, RoutedEventArgs e)
        {
            EditWarehouse EW = new EditWarehouse();
            EW.Show();
        }
        private void WarehouseMenuClose()
        {
            DoubleAnimation WarehouseBtnAnimation = new DoubleAnimation();
            WarehouseBtnAnimation.From = WarehouseMenu.ActualHeight;
            WarehouseBtnAnimation.To = 0;
            WarehouseBtnAnimation.Duration = TimeSpan.FromSeconds(0.2);
            WarehouseMenu.BeginAnimation(Grid.HeightProperty, WarehouseBtnAnimation);
        }
        // ========================= WAREHOUSE MENU END =========================

        // =========================== SET MONTH PLAN ===========================
        private void SetMonthPlan_Click(object sender, RoutedEventArgs e)
        {
            SetMonthPlan SMP = new SetMonthPlan();
            SMP.Show();
        }

        private void EditMenuClose()
        {
            DoubleAnimation ManagersBtnAnimation = new DoubleAnimation();
            ManagersBtnAnimation.From = EditMenu.ActualHeight;
            ManagersBtnAnimation.To = 0;
            ManagersBtnAnimation.Duration = TimeSpan.FromSeconds(0.2);
            EditMenu.BeginAnimation(Grid.HeightProperty, ManagersBtnAnimation);
        }
        // =========================== EDIT MENU END ===========================

        // ============================ SEARCH FIELD ============================
        private void SearchField_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (AdminMode)
            {
                string searchText = SearchTextBox.Text.Trim().ToLower();

                // Получаем выбранную вкладку
                if (AdminTabControl.SelectedItem is TabItem selectedTab)
                {
                    // Получаем DataGrid из содержимого вкладки
                    if (selectedTab.Content is DataGrid dataGrid)
                    {
                        // Источник должен быть DataView
                        if (dataGrid.ItemsSource is DataView dataView)
                        {
                            List<string> filterConditions = new List<string>();

                            foreach (DataColumn column in dataView.Table.Columns)
                            {
                                // Строим условие для фильтрации всех столбцов
                                filterConditions.Add($"CONVERT([{column.ColumnName}], 'System.String') LIKE '%{searchText}%'");
                            }

                            // Устанавливаем фильтр
                            dataView.RowFilter = string.Join(" OR ", filterConditions);
                        }
                    }
                }
            }
            else
            {
                string searchText = SearchTextBox.Text.Trim().ToLower();

                if (MainDataGrid.ItemsSource is DataView dataView)
                {
                    List<string> filterConditions = new List<string>();

                    foreach (DataColumn column in dataView.Table.Columns)
                    {
                        // Фильтрация абсолютно всех типов данных через строковое преобразование
                        filterConditions.Add($"CONVERT([{column.ColumnName}], 'System.String') LIKE '%{searchText}%'");
                    }

                    dataView.RowFilter = string.Join(" OR ", filterConditions);
                }
            }
        }
        // ========================== SEARCH FIELD END ==========================

        // ============================ PROFILE MENU ============================
        private void ProfileMenu_Click(object sender, RoutedEventArgs e)
        {
            if (((int)ProfileMenu.ActualHeight) > 0)
            {
                DoubleAnimation ProfileBtnAnimation = new DoubleAnimation();
                ProfileBtnAnimation.From = ProfileMenu.ActualHeight;
                ProfileBtnAnimation.To = 0;
                ProfileBtnAnimation.Duration = TimeSpan.FromSeconds(0.2);
                ProfileMenu.BeginAnimation(Grid.HeightProperty, ProfileBtnAnimation);
            }
            else
            {
                DoubleAnimation ProfileBtnAnimation = new DoubleAnimation();
                ProfileBtnAnimation.From = ProfileMenu.ActualHeight;
                ProfileBtnAnimation.To = 65;
                ProfileBtnAnimation.Duration = TimeSpan.FromSeconds(0.2);
                ProfileMenu.BeginAnimation(Grid.HeightProperty, ProfileBtnAnimation);
            }
        }
        // ============================ EDIT PROFILE ============================
        private void ProfileBtn_Click(object sender, RoutedEventArgs e)
        {
            ProfilePage ProfilePageWindow = new ProfilePage();
            ProfilePageWindow.ShowDialog();
        }
        // ============================== LOG OUT ==============================
        private void LogOut_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow LoginWindow = new LoginWindow();
            LoginWindow.Show();
            this.Close();
            DataSource.Clear();

        }
        private void ProfileMenuClose()
        {
            DoubleAnimation ProfileBtnAnimation = new DoubleAnimation();
            ProfileBtnAnimation.From = ProfileMenu.ActualHeight;
            ProfileBtnAnimation.To = 0;
            ProfileBtnAnimation.Duration = TimeSpan.FromSeconds(0.2);
            ProfileMenu.BeginAnimation(Grid.HeightProperty, ProfileBtnAnimation);
        }
        // ========================== PROFILE MENU END ==========================

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Получаем элемент, по которому был сделан клик
            var clickedElement = e.OriginalSource as DependencyObject;

            // Проверяем, был ли клик вне всех меню
            if (!IsDescendantOf(clickedElement, DatabaseMenu) &&
                !IsDescendantOf(clickedElement, EditMenu) &&
                !IsDescendantOf(clickedElement, ProfileMenu))
            {
                DatabaseMenuClose();
                EditMenuClose();
                UserMenuClose(); 
                ProductMenuClose();
                WarehouseMenuClose();
                ProfileMenuClose();
            }
        }
        // =========================== FILTER BUTTON ===========================
        private void ApplyFilterButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyColumnVisibility();
        }
        // ================================ LOAD ================================
        private void MainLoad()
        {
            RoleControl();
            MinimizeElements();
            LoadDataToLabel();
            ReLoadData(DBname);
        }
        // ============================ DATA CONTROL ============================
        private void RoleControl()
        {
            NameLabel.Content = Name;
            RoleLabel.Content = Role;
            string labelText = RoleLabel.Content?.ToString(); // Получаем текст из Label, безопасно обрабатывая null

            if (labelText != "Developer" && labelText != "Admin")
            {
                this.Title = "Главное окно";
                ManagerControls();
            }
            else
            {
                this.Title = "Главное окно (ADMIN)";
                if (labelText == "Developer") { MessageBox.Show("Welcome back Developer! All systems online", "Welcome back", MessageBoxButton.OK, MessageBoxImage.Information); }
                AdminMode = true;
                AdminControls();
            }
        }

        private void LoadDataToLabel(string role = null)
        {
            string connectionString = CH.GetConnectionString();
            string query = $"SELECT monthPlan FROM users WHERE login = '{Login}'";
            string monthName = DateTime.Now.ToString("MMMM", new System.Globalization.CultureInfo("ru-RU"));

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    MySqlCommand command = new MySqlCommand(query, connection);
                    object result = command.ExecuteScalar();
                    var culture = new CultureInfo("en-US");

                    // Модифицированная проверка на null и DBNull
                    if (result == null || result == DBNull.Value)
                    {
                        MonthPlanLabel.Content = "0";
                    }
                    else
                    {
                        int convResult = Convert.ToInt32(result);
                        MonthPLabel.Content = $"План ({monthName})";
                        SoldedMLabel.Content = $"Продано ({monthName})";
                        MonthPlanLabel.Content = convResult.ToString("N0", culture);
                    }
                }
                SoldControl(role);
            }
            catch (MySqlException ex)
            {
                AddErrorMessage($"Ошибка загрузки данных (LOADDATATOLABEL): {ex.Message}"); ;
                MonthPlanLabel.Content = "0"; // Устанавливаем 0 при ошибке
            }
        }

        private void SoldControl(string role = null)
        {
            if (AdminMode)
            {
                string connectionString = CH.GetConnectionString();
                string query = $"SELECT monthplan FROM users WHERE login = '{Login}'";
                string query2 = $"SELECT SUM(ShipmentPrice) from `{GetTabName()}`";
                var culture = new CultureInfo("en-US");

                try
                {
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        MySqlCommand command = new MySqlCommand(query, connection);
                        MySqlCommand command2 = new MySqlCommand(query2, connection);

                        object MonthPlan = command.ExecuteScalar();
                        object result = command2.ExecuteScalar();

                        // Обработка пустых значений
                        int ConvMonthPlan = (MonthPlan == null || MonthPlan == DBNull.Value) ? 0 : Convert.ToInt32(MonthPlan);
                        int ConvResult = (result == null || result == DBNull.Value) ? 0 : Convert.ToInt32(result);

                        // Логика изменения цвета
                        if (ConvResult < ConvMonthPlan / 2)
                        {
                            SoldedLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC00F0C"));
                        }
                        else if (ConvResult > ConvMonthPlan / 2 && ConvResult < ConvMonthPlan)
                        {
                            SoldedLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFD324"));
                        }
                        else
                        {
                            SoldedLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#14AE5C"));
                        }

                        SoldedLabel.Content = ConvResult.ToString("N0", culture);
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message != "Unknown column 'ShipmentPrice' in 'field list'")
                    {
                        AddErrorMessage($"Ошибка загрузки данных (КОНТРОЛЬ ПРОДАЖ) ADMIN: {ex.Message}");
                    }
                    SoldedLabel.Content = "0"; // Устанавливаем 0 при ошибке
                }
            }
            else
            {
                string connectionString = CH.GetConnectionString();
                string query = $"SELECT MonthPlan FROM roles WHERE role = '{role}'";
                string query2 = $"SELECT SUM(ShipmentPrice) from `{DBname}`";
                var culture = new CultureInfo("en-US");

                try
                {
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        MySqlCommand command = new MySqlCommand(query, connection);
                        MySqlCommand command2 = new MySqlCommand(query2, connection);

                        object MonthPlan = command.ExecuteScalar();
                        object result = command2.ExecuteScalar();

                        // Обработка пустых значений
                        int ConvMonthPlan = (MonthPlan == null || MonthPlan == DBNull.Value) ? 0 : Convert.ToInt32(MonthPlan);
                        int ConvResult = (result == null || result == DBNull.Value) ? 0 : Convert.ToInt32(result);

                        // Логика изменения цвета
                        if (ConvResult < ConvMonthPlan / 2)
                        {
                            SoldedLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC00F0C"));
                        }
                        else if (ConvResult > ConvMonthPlan / 2 && ConvResult < ConvMonthPlan)
                        {
                            SoldedLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFD324"));
                        }
                        else
                        {
                            SoldedLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#14AE5C"));
                        }

                        SoldedLabel.Content = ConvResult.ToString("N0", culture);
                    }
                }
                catch (Exception ex)
                {
                    AddErrorMessage($"Ошибка загрузки данных (КОНТРОЛЬ ПРОДАЖ): {ex.Message}");
                    SoldedLabel.Content = "0"; // Устанавливаем 0 при ошибке
                }
            }
        }

        // ============================ ADMIN LOGIN ============================

        private void AdminControls()
        {
            AdminEditButton.Visibility = Visibility.Visible;
            AdminTabControl.Visibility = Visibility.Visible;
            MainDataGrid.Visibility = Visibility.Collapsed;
            GetMySQLTables(CH.GetConnectionString());
            LoadTablesIntoTabControl();
            AdminTabControl.SelectedIndex = 0;
            LoadDataAndCreateCheckBoxes(true);
        }
        private void AdminDelete()
        {
            // Получаем текущую активную вкладку

            if (AdminTabControl.SelectedItem is TabItem currentTab)
            {
                // Получаем DataGrid из содержимого вкладки
                if (currentTab.Content is DataGrid dataGrid)
                {
                    // Проверяем, есть ли выбранная строка
                    if (dataGrid.SelectedItem == null)
                    {
                        MessageBox.Show("Выберите строку для удаления!");
                        return;
                    }
                    MessageBoxResult DialogResult = MessageBox.Show("Удалить выбранную запись?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (DialogResult == MessageBoxResult.Yes)
                    {
                        // Получаем DataView (источник данных DataGrid)
                        if (dataGrid.ItemsSource is DataView dataView)
                        {
                            // Удаляем строку из DataView
                            dataView.Delete(dataGrid.SelectedIndex);

                            // Обновляем базу данных
                            UpdateDatabase(currentTab.Header.ToString(), dataView.Table);
                        }
                    }
                }
            }
            ReLoadData(DBname, true);
            ApplyColumnVisibility();
        }

        // =========================== MANAGER LOGIN ===========================
        private void ManagerControls()
        {
            AdminEditButton.Visibility = Visibility.Collapsed;
            AdminTabControl.Visibility = Visibility.Collapsed;
            AdminDatabaseGrid.Visibility = Visibility.Collapsed;
            MainDataGrid.Visibility = Visibility.Visible;
            LoadTablesIntoTabControl();
            LoadDataAndCreateCheckBoxes();
            LoadDataToLabel(Role);
        }
        private void DeleteData()
        {
            if (MainDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите строку для удаления");
                return;
            }
            DataRowView selectedRow = (DataRowView)MainDataGrid.SelectedItem;
            int id = Convert.ToInt32(selectedRow["id"]);
            if (MessageBox.Show("Вы уверены, что хотите удалить эту запись?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    using (MySqlConnection connection = new MySqlConnection(CH.GetConnectionString()))
                    {
                        connection.Open();
                        string deleteQuery = $"DELETE FROM {DBname} WHERE (`id` = @id);";
                        MySqlCommand command = new MySqlCommand(deleteQuery, connection);
                        command.Parameters.AddWithValue("@id", id);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Запись успешно удалена");
                            ReLoadData();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            ReLoadData(DBname);
            ApplyColumnVisibility();
        }

        // ========================== SYSTEM FUNCTIONS ==========================

        // ========================= ADMIN TABLES LOAD =========================
        private DataGrid CreateDataGridForTable(string connectionString, string tableName)
        {
            var dataGrid = new DataGrid
            {
                AutoGenerateColumns = false, // Отключаем авто-генерацию столбцов
                SelectionMode = DataGridSelectionMode.Single,
                SelectionUnit = DataGridSelectionUnit.FullRow,
                CanUserSortColumns = true // Разрешаем сортировку
            };

            var tableData = new DataTable();
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                var adapter = new MySqlDataAdapter($"SELECT * FROM `{tableName}`", connection);
                adapter.Fill(tableData);
            }

            // Очистка столбцов (хотя при AutoGenerateColumns=false это не обязательно)
            dataGrid.Columns.Clear();

            foreach (DataColumn column in tableData.Columns)
            {
                // Создаем правильную привязку для DataTable
                var binding = new Binding(column.ColumnName);

                var gridColumn = new DataGridTextColumn
                {
                    Header = column.ColumnName,
                    Binding = binding,
                    CanUserSort = true // Разрешаем сортировку для столбца
                };

                // Форматирование для даты
                if (column.DataType == typeof(DateTime) ||
                    column.ColumnName.Equals("ShipmentDate", StringComparison.OrdinalIgnoreCase))
                {
                    gridColumn.Binding = new Binding(column.ColumnName)
                    {
                        StringFormat = "dd.MM.yyyy"
                    };
                }

                dataGrid.Columns.Add(gridColumn);
            }

            // Устанавливаем источник данных
            dataGrid.ItemsSource = tableData.DefaultView;

            return dataGrid;
        }

        // Вспомогательная функция для проверки принадлежности элемента
        private bool IsDescendantOf(DependencyObject element, DependencyObject parent)
        {
            if (element == null || parent == null)
                return false;

            // Проверяем все родительские элементы
            while (element != null)
            {
                if (element == parent)
                    return true;

                // Для Popup элементов нужна особая проверка
                if (element is Visual || element is Visual3D)
                    element = VisualTreeHelper.GetParent(element);
                else
                    element = LogicalTreeHelper.GetParent(element);
            }

            return false;
        }

        public List<string> GetMySQLTables(string connectionString)
        {
            List<string> tables = new List<string>();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                // Запрос для получения списка таблиц
                string query = "SHOW TABLES";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tables.Add(reader.GetString(0));
                        }
                    }
                }
            }
            return tables;
        }

        // Метод для обновления базы данных после удаления
        private void UpdateDatabase(string tableName, DataTable dataTable)
        {
            string connectionString = CH.GetConnectionString();

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // Используем MySqlCommandBuilder для генерации UPDATE-запроса
                    MySqlDataAdapter adapter = new MySqlDataAdapter($"SELECT * FROM `{tableName}`", connection);
                    MySqlCommandBuilder commandBuilder = new MySqlCommandBuilder(adapter);

                    // Применяем изменения в базе данных
                    adapter.Update(dataTable);

                    MessageBox.Show("Данные успешно удалены из базы!");
                }
            }
            catch (Exception ex)
            {
                AddErrorMessage($"Ошибка при удалении данных (TAB CONTROL) ADMIN: {ex.Message}");
            }
        }
        // Метод для обновления данных


        // Модифицированный метод загрузки данных
        private void LoadTablesIntoTabControl()
        {
            string connectionString = CH.GetConnectionString();
            var tables = GetMySQLTables(connectionString);

            AdminTabControl.Items.Clear();

            foreach (string tableName in tables)
            {
                var newTab = new TabItem
                {
                    Header = tableName,
                    Content = CreateDataGridForTable(connectionString, tableName)
                };

                AdminTabControl.Items.Add(newTab);
            }

            if (AdminTabControl.Items.Count > 0)
            {
                AdminTabControl.SelectedIndex = 0;
            }
        }

        private void MinimizeElements()
        {
            DatabaseMenu.Height = 0;
            EditMenu.Height = 0;
            UserMenu.Height = 0;
            ProductMenu.Height = 0;
            WarehouseMenu.Height = 0;
            ProfileMenu.Height = 0;
        }

        public void ReLoadData(string name = null, bool AdminMode1 = false)
        {
            if (AdminMode1)
            {
                // Сохраняем индекс текущей вкладки
                int currentIndex = AdminTabControl.SelectedIndex;

                // Полностью очищаем TabControl
                AdminTabControl.ItemsSource = null;

                // Перезагружаем данные
                LoadTablesIntoTabControl();

                // Восстанавливаем выбранную вкладку (если возможно)
                if (currentIndex >= 0 && currentIndex < AdminTabControl.Items.Count)
                {
                    AdminTabControl.SelectedIndex = currentIndex;
                }

                string tableName = GetTabName();
                if (!string.IsNullOrWhiteSpace(tableName) && tableName != "error")
                {
                    SoldControl(CH.GetRole(tableName));
                }
                else
                {
                    AddErrorMessage("Ошибка: не удалось определить имя таблицы для вкладки.");
                }
            }
            else
            {
                SoldControl(CH.GetRole(DBname));
                MainDataGrid.ItemsSource = null;
                try
                {
                    using (MySqlConnection connection = new MySqlConnection(CH.GetConnectionString()))
                    {
                        connection.Open();

                        string query = CH.ManagerData(DBname);
                        using (MySqlCommand command = new MySqlCommand(query, connection))
                        {
                            MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);

                            MainDataGrid.ItemsSource = dataTable.DefaultView;
                        }
                    }
                }
                catch (Exception ex)
                {
                    AddErrorMessage($"Ошибка загрузки данных (RELOADDATA): {ex.Message}");
                }
            }
        }
        private void ApplyColumnVisibility()
        {
            if (AdminMode)
            {
                // Получаем список выбранных столбцов
                List<string> selectedColumns = new List<string>();
                foreach (UIElement element in CheckBoxPanel.Children)
                {
                    if (element is CheckBox checkBox && checkBox.IsChecked == true)
                    {
                        selectedColumns.Add(checkBox.Tag.ToString());
                    }
                }

                // Обновляем видимость столбцов в DataGrid
                DataGrid AMDG = GetSelectedDataGrid(AdminTabControl);
                foreach (DataGridColumn column in AMDG.Columns)
                {
                    if (column.Header != null)
                    {
                        column.Visibility = selectedColumns.Contains(column.Header.ToString()) ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
            }
            else {
                // Получаем список выбранных столбцов
                List<string> selectedColumns = new List<string>();
                foreach (UIElement element in CheckBoxPanel.Children)
                {
                    if (element is CheckBox checkBox && checkBox.IsChecked == true)
                    {
                        selectedColumns.Add(checkBox.Tag.ToString());
                    }
                }

                // Обновляем видимость столбцов в DataGrid
                foreach (DataGridColumn column in MainDataGrid.Columns)
                {
                    if (column.Header != null)
                    {
                        column.Visibility = selectedColumns.Contains(column.Header.ToString()) ? Visibility.Visible : Visibility.Collapsed;
                    }
                }

            }
        }


        // ==================== ERROR CONTROL (RUNNING LINE) ====================
        public enum ErrorLevel { Info, Warning, Error }

        public void AddErrorMessage(string message, ErrorLevel level = ErrorLevel.Error)
        {
            if (ErrorMarquee == null || MarqueeTransform == null)
                return;

            // Префикс уровня
            string prefix = level switch
            {
                ErrorLevel.Info => "[ИНФО]",
                ErrorLevel.Warning => "[ВНИМАНИЕ]",
                ErrorLevel.Error => "[ОШИБКА]",
                _ => ""
            };

            string fullMessage = $"{prefix} {message}";

            // Добавление сообщения
            if (!string.IsNullOrWhiteSpace(ErrorMarquee.Text))
            {
                ErrorMarquee.Text += " • " + fullMessage;
            }
            else
            {
                ErrorMarquee.Text = fullMessage;
            }

            // Изменение цвета в зависимости от уровня
            ErrorMarquee.Foreground = level switch
            {
                ErrorLevel.Info => Brushes.LightGreen,
                ErrorLevel.Warning => Brushes.Gold,
                ErrorLevel.Error => Brushes.OrangeRed,
                _ => Brushes.White
            };

            RestartMarqueeAnimation();
        }

        public void ClearErrorMessages()
        {
            if (ErrorMarquee == null || MarqueeTransform == null)
                return;

            ErrorMarquee.Text = "Готов к работе...";
            ErrorMarquee.Foreground = Brushes.LightGray;
            RestartMarqueeAnimation();
        }

        private void RestartMarqueeAnimation()
        {
            if (ErrorMarquee == null || MarqueeTransform == null)
                return;

            var animation = new DoubleAnimation
            {
                From = ActualWidth,
                To = -ErrorMarquee.ActualWidth,
                Duration = TimeSpan.FromSeconds(60),
                RepeatBehavior = RepeatBehavior.Forever
            };

            MarqueeTransform.BeginAnimation(TranslateTransform.XProperty, animation);
        }

        // =============================== FILTER ===============================

        private double horizontalMargin = 10;       // Горизонтальный отступ
        private double verticalMargin = 5;         // Вертикальный отступ
        private double maxHeight = 1000;
        private void FilterScrollViewver_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double availableWidth = CheckBoxPanel.ActualWidth;
            int maxRows = Math.Max(1, (int)(maxHeight / ((verticalMargin * 2))));  // Максимальное количество строк
            int columnCountByWidth = Math.Max(1, (int)(availableWidth / (horizontalMargin * 2))); // Столбцов по ширине
            int columnCountByHeight = (int)Math.Ceiling((double)CheckBoxPanel.Children.Count / maxRows);  // Столбцов по высоте

            int columnCount = Math.Min(columnCountByWidth, columnCountByHeight); // Выбираем меньшее из двух значений

            CheckBoxPanel.Columns = columnCount;
        }

        private void FilterAllElements_Checked(object sender, RoutedEventArgs e)
        {
            LoadDataAndCreateCheckBoxes();
            ReLoadData(DBname);
        }

        private void LoadDataAndCreateCheckBoxes(bool AdminMode = false)
        {
            try
            {
                CheckBoxPanel.Children.Clear(); // Очистка CheckBox'ов

                using (var connection = new MySqlConnection(CH.GetConnectionString()))
                {
                    connection.Open();

                    string query;
                    DataTable allData = new DataTable();

                    if (AdminMode)
                    {
                        string tableName = GetTabName();
                        if (string.IsNullOrWhiteSpace(tableName) || tableName.ToLower() == "error")
                        {
                            //AddErrorMessage("Ошибка: имя таблицы не определено или содержит ошибку.");
                            return;
                        }

                        query = $"SELECT * FROM `{tableName}`";
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(DBname))
                        {
                            AddErrorMessage("Ошибка: имя базы данных не указано.");
                            return;
                        }

                        query = CH.ManagerData(DBname);
                    }

                    var adapter = new MySqlDataAdapter(query, connection);
                    adapter.Fill(allData);

                    // Создание чекбоксов по столбцам
                    foreach (DataColumn column in allData.Columns)
                    {
                        CheckBox checkBox = new CheckBox
                        {
                            Content = column.ColumnName,
                            IsChecked = true,
                            Margin = new Thickness(10, 5, 5, 5),
                            FontSize = 15,
                            Style = (Style)FindResource("RoundedCheckBoxStyle"),
                            Tag = column.ColumnName
                        };
                        CheckBoxPanel.Children.Add(checkBox);
                    }

                    if (AdminMode)
                    {
                        // Получение текущего DataGrid внутри активной вкладки
                        if (AdminTabControl.SelectedItem is TabItem selectedTab && selectedTab.Content is DataGrid dataGrid)
                        {
                            dataGrid.ItemsSource = allData.DefaultView;
                            ApplyColumnVisibility();
                        }
                        else
                        {
                            AddErrorMessage("Не удалось найти DataGrid во вкладке администратора.");
                        }
                    }
                    else
                    {
                        MainDataGrid.ItemsSource = allData.DefaultView;
                        MainDataGrid.AutoGenerateColumns = true;
                        ApplyColumnVisibility();
                    }
                }
            }
            catch (MySqlException ex)
            {
                AddErrorMessage($"Ошибка загрузки данных (MySQL): {ex.Message}");
            }
            catch (Exception ex)
            {
                AddErrorMessage($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private T FindChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T found)
                    return found;

                var result = FindChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }


        private void DataGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_isCtrlPressed && _isMouseOverDataGrid)
            {
                double newSize = MainDataGrid.FontSize + (e.Delta > 0 ? 1 : -1);

                if (newSize < 8 || newSize > 24)
                {
                    // Анимация "отскока" при достижении границ
                    DoubleAnimation anim = new DoubleAnimation
                    {
                        To = newSize < 8 ? 9 : 23,
                        Duration = TimeSpan.FromMilliseconds(200),
                        AutoReverse = true,
                        EasingFunction = new ElasticEase { Oscillations = 2 }
                    };
                    MainDataGrid.BeginAnimation(Control.FontSizeProperty, anim);
                    ReLoadData(DBname);

                }
                else
                {
                    // Обычная плавная анимация
                    DoubleAnimation anim = new DoubleAnimation
                    {
                        To = newSize,
                        Duration = TimeSpan.FromMilliseconds(300),
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                    };
                    MainDataGrid.BeginAnimation(Control.FontSizeProperty, anim);
                    ReLoadData(DBname);

                }

                e.Handled = true;
            }
        }

        private void DataGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            _isMouseOverDataGrid = true;
        }

        private void DataGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            _isMouseOverDataGrid = false;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                _isCtrlPressed = true;
            }
            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                _isCtrlPressed = false;
            }
            base.OnKeyUp(e);
        }

        private void MainDataGrid_Loaded(object sender, RoutedEventArgs e)
        {

            var dataGrid = (DataGrid)sender;

            // Создаем стиль программно
            var cellStyle = new Style(typeof(DataGridCell));
            cellStyle.Setters.Add(new Setter(Control.FontSizeProperty, 12.0));

            // Триггер при наведении
            var trigger = new Trigger()
            {
                Property = UIElement.IsMouseOverProperty,
                Value = true
            };
            trigger.Setters.Add(new Setter(Control.FontSizeProperty, 14.0));

            cellStyle.Triggers.Add(trigger);
            dataGrid.CellStyle = cellStyle;
        }

        private void AdminTabControl_Selected(object sender, SelectionChangedEventArgs e)
        {
            if (AdminTabControl.SelectedItem is TabItem selectedTab)
            {
                DBname = selectedTab.Header.ToString();

                ReLoadData(DBname, true);

            }
        }
        private string GetTabName()
        {
            if (AdminTabControl.SelectedItem is TabItem selectedTab)
            {
                var _currentTableName = selectedTab.Header.ToString();
                return _currentTableName;
            }
            else return "error";
        }
        private void AdminTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Проверяем, что изменение действительно связано с выбором вкладки
            if (e.Source is TabControl tabControl)
            {
                // Получаем выбранную вкладку
                if (tabControl.SelectedItem is TabItem selectedTab)
                {
                    LoadDataAndCreateCheckBoxes(true);
                    LoadDataToLabel(CH.GetRole(GetTabName()));
                }
            }

        }
        public DataGrid GetSelectedDataGrid(TabControl tabControl)
        {
            if (tabControl.SelectedItem is TabItem selectedTab &&
                selectedTab.Content is DataGrid dataGrid)
            {
                return dataGrid;
            }
            return null;
        }
        
        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child is T correctlyTyped)
                    return correctlyTyped;

                T descendent = FindVisualChild<T>(child);
                if (descendent != null)
                    return descendent;
            }
            return null;
        }

        private void TableRemoveBtn_Click(object sender, RoutedEventArgs e)
        {
            TableRemove TBR = new TableRemove();
            TBR.Show();
        }
    }
}