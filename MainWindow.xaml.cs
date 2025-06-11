using ClosedXML.Excel;
using Microsoft.Win32;
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

namespace ManagerAppV3._5
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
        #region DATABASE MENU
        private void DatabaseMenuOpen(object sender, RoutedEventArgs e)
        {
            if (DatabaseMenu.ActualHeight > 0)
            {
                AnimateMenu(DatabaseMenu, 0);
                SetMenuIcon(DataBaseBtnimg, "/Icons/ArrowDown.png");
            }
            else
            {
                AnimateMenu(DatabaseMenu, 125);
                SetMenuIcon(DataBaseBtnimg, "/Icons/ArrowUp.png");
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
                    if (selectedTab != null)
                    {
                        var dataGrid1 = selectedTab.Content as DataGrid;
                        if (dataGrid1 != null && dataGrid1.SelectedItem != null)
                        {
                            // Приводим к DataRowView (если ItemsSource — DataView):
                            var rowView = dataGrid.SelectedItem as DataRowView;
                            if (rowView != null)
                            {
                                // Получаем значение столбца 'id':
                                string idValue = rowView["id"].ToString();
                                var selectedData = row.Row; // DataRow с доступом по именам колонок
                                var editWindow = new AddnEdit("Edit", GetTabName(), row.Row, idValue, true); // передаём DataRow
                                editWindow.ShowDialog();
                            }
                        }
                    }
                }
            }
            else
            {
                if (MainDataGrid.SelectedItem == null)
                {
                    MessageBox.Show("Выберите строку для изменения");
                    return;
                }
                DataRowView selectedRow = (DataRowView)MainDataGrid.SelectedItem;
                string id = selectedRow["id"].ToString();

                var editWindow = new AddnEdit("Edit",DBname, null, id); 
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
        // ============================ EXPORT DATA ============================
        #region Export Button
        private void ExportBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dataTable = null;
                string fileName = "Export.xlsx";

                if (AdminMode)
                {
                    // --- Экспорт из выбранной вкладки TabControl ---
                    var selectedTab = AdminTabControl.SelectedItem as TabItem;
                    if (selectedTab == null)
                    {
                        MessageBox.Show("Не выбрана вкладка.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var dataGrid = selectedTab.Content as DataGrid;
                    if (dataGrid == null || dataGrid.ItemsSource == null)
                    {
                        MessageBox.Show("В таблице нет данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var dataView = dataGrid.ItemsSource as DataView;
                    if (dataView == null)
                    {
                        MessageBox.Show("Источник данных некорректен.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    dataTable = dataView.ToTable();
                    fileName = selectedTab.Header.ToString() + ".xlsx";
                }
                else
                {
                    // --- Экспорт из MainDataGrid ---
                    if (MainDataGrid.ItemsSource == null)
                    {
                        MessageBox.Show("В таблице нет данных для экспорта.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var dataView = MainDataGrid.ItemsSource as DataView;
                    if (dataView == null)
                    {
                        MessageBox.Show("Источник данных некорректен.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    dataTable = dataView.ToTable();
                    fileName = $"{NameLabel.Content.ToString()}.xlsx";
                }

                // Диалог выбора пути сохранения
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel файл (*.xlsx)|*.xlsx",
                    Title = "Сохранить таблицу как Excel",
                    FileName = fileName
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    using (XLWorkbook workbook = new XLWorkbook())
                    {
                        workbook.Worksheets.Add(dataTable, "Данные");
                        workbook.SaveAs(saveFileDialog.FileName);
                    }

                    MessageBox.Show("Экспорт завершен успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        private void DatabaseMenuClose()
        {
            AnimateMenu(DatabaseMenu, 0);
            SetMenuIcon(DataBaseBtnimg, "/Icons/ArrowDown.png");
        }
        #endregion
        // =========================== UPDATE BUTTON ===========================
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            ReLoadData(DBname, AdminMode);
            ApplyColumnVisibility();
            LoadDataToLabel(DBname);
            GetMonth();
        }
        public void UpdateAll()
        {
            ReLoadData(DBname, AdminMode);
            ApplyColumnVisibility();
            LoadDataToLabel(DBname);
            GetMonth();
        }
        // ============================= EDIT MENU =============================
        #region EDIT MENU
        private void ManagersButton_Click(object sender, RoutedEventArgs e)
        {
            if (EditMenu.ActualHeight > 0)
            {
                AnimateMenu(EditMenu, 0);
                SetMenuIcon(EditBtnIMG, "/Icons/ArrowDown.png");
            }
            else
            {
                AnimateMenu(EditMenu, 155);
                SetMenuIcon(EditBtnIMG, "/Icons/ArrowUp.png");
            }
 
        }

        // ============================= USER MENU =============================
        #region USER MENU
        private void UsersMenu_Click(object sender, RoutedEventArgs e)
        {
            if (UserMenu.ActualHeight > 0)
            {
                AnimateMenu(UserMenu, 0);
            }
            else
            {
                ProductMenuClose();
                WarehouseMenuClose();
                AnimateMenu(UserMenu, 65);
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
        private void UserMenuClose()
        {
            AnimateMenu(UserMenu, 0);
        }
        #endregion

        // ============================ PRODUCT MENU ============================
        #region PRODUCT MENU
        private void ProductMenu_Click(object sender, RoutedEventArgs e)
        {
            if (ProductMenu.ActualHeight > 0)
            {
                AnimateMenu(ProductMenu, 0);
            }
            else
            {
                UserMenuClose();
                WarehouseMenuClose();
                AnimateMenu(ProductMenu, 65);
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
            AnimateMenu(ProductMenu, 0);
        }
        // ========================== PRODUCT MENU END ==========================
        #endregion
        // =========================== WAREHOUSE MENU ===========================

        #region WAREHOUSE MENU
        private void WarehouseMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            if (WarehouseMenu.ActualHeight> 0)
            {
                AnimateMenu(WarehouseMenu, 0);
            }
            else
            {
                UserMenuClose();
                ProductMenuClose();
                AnimateMenu(WarehouseMenu, 65);
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
            AnimateMenu(WarehouseMenu, 0);
        }
        // ========================= WAREHOUSE MENU END =========================
        #endregion
        // =========================== SET MONTH PLAN ===========================
        private void SetMonthPlan_Click(object sender, RoutedEventArgs e)
        {
            SetMonthPlan SMP = new SetMonthPlan();
            SMP.Show();
        }
        // ============================ REMOVE TABLE ============================
        private void TableRemoveBtn_Click(object sender, RoutedEventArgs e)
        {
            TableRemove TBR = new TableRemove();
            TBR.Show();
        }
        private void EditMenuClose()
        {
            AnimateMenu(EditMenu, 0);
            SetMenuIcon(EditBtnIMG, "/Icons/ArrowDown.png");
        }
        // =========================== EDIT MENU END ===========================
        #endregion
        // ============================ SEARCH FIELD ============================
        #region SEARCH FIELD
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
        #endregion
        // ============================ PROFILE MENU ============================
        #region PROFILE MENU
        private void ProfileMenu_Click(object sender, RoutedEventArgs e)
        {
            if (ProfileMenu.ActualHeight > 0)
            {
                AnimateMenu(ProfileMenu, 0);
            }
            else
            {
                AnimateMenu(ProfileMenu, 65);
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
            AnimateMenu(ProfileMenu, 0);
        }
        #endregion

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
            GetMonth();
        }
        // ============================ DATA CONTROL ============================
        #region DATA CONTROL
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

        private void LoadDataToLabel(string DBname = null)
        {
            string connectionString = CH.GetConnectionString();
            string query = $"SELECT monthPlan FROM users WHERE databasename = '{DBname}'";

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
                        MonthPlanLabel.Content = convResult.ToString("N0", culture);
                    }
                }
                SoldControl();
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
        #endregion
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
            GetMonth();
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
            //LoadTablesIntoTabControl();
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
                var specialTables = new List<string> { "product price", "roles", "users", "warehouses" };

                if (!specialTables.Contains(tableName))
                {
                    string query = CH.ManagerData(DBname);
                    var adapter = new MySqlDataAdapter(query, connection);
                    adapter.Fill(tableData);
                }
                else
                {
                    var adapter = new MySqlDataAdapter($"SELECT * FROM `{tableName}`", connection);
                    adapter.Fill(tableData);
                }

            }

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

                         var specialTables = new List<string> { "product price", "roles", "users", "warehouses" };

                        if (!specialTables.Contains(tableName))
                        {
                            query = CH.ManagerData(tableName);
                        }
                        else
                        {
                            query = $"SELECT * FROM `{tableName}`";
                        }

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
                    DataSource.DBname = GetTabName();
                    LoadDataToLabel(GetTabName());
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

        private void GetMonth()
        {
            string monthName = DateTime.Now.ToString("MMMM", new System.Globalization.CultureInfo("ru-RU"));

            MonthPLabel.Content = $"План ({monthName})";
            SoldedMLabel.Content = $"Продано ({monthName})";
        }
        // ======================= MANAGER DATAGRID STYLE =======================
        #region MANAGER DATAGRID STYLE
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
        #endregion
        #region Menu animations
        private void AnimateMenu(FrameworkElement menu, double toHeight)
        {
            DoubleAnimation animation = new DoubleAnimation
            {
                From = menu.ActualHeight,
                To = toHeight,
                Duration = TimeSpan.FromSeconds(0.2)
            };
            menu.BeginAnimation(FrameworkElement.HeightProperty, animation);
        }
        // Add the missing SetMenuIcon method to resolve the CS0103 error.
        private void SetMenuIcon(Image imageControl, string iconPath)
        {
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(iconPath, UriKind.RelativeOrAbsolute);
            bitmapImage.EndInit();
            imageControl.Source = bitmapImage;
        }
        #endregion

        
    }
}