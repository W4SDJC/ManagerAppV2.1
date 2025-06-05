using ManagerAppV2;
using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ManagerAppV2._1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
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
            AddnEdit addnEdit = new AddnEdit("Add", GetTabName());
            addnEdit.ShowDialog();
            ReLoadData(DBname);
            ApplyColumnVisibility();
        }

        // ============================= EDIT DATA =============================
        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            TabItem selectedTab = AdminTabControl.SelectedItem as TabItem;
            if (selectedTab?.Content is DataGrid dataGrid && dataGrid.SelectedItem is DataRowView row)
            {
                var selectedData = row.Row; // DataRow с доступом по именам колонок
                var editWindow = new AddnEdit("Edit", GetTabName(), row.Row); // передаём DataRow
                editWindow.ShowDialog();
            }
            //AddnEdit addnEdit = new AddnEdit("Edit", GetTabName());
            //addnEdit.ShowDialog();
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
                MessageBox.Show(RoleLabel.Content.ToString());
            }
        }
        // ================================= = =================================
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

        // =========================== UPDATE BUTTON ===========================
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            ReLoadData(DBname, AdminMode);
            ApplyColumnVisibility();
        }

        // =========================== MANAGERS MENU ===========================

        private void ManagersButton_Click(object sender, RoutedEventArgs e)
        {
            if (((int)ManagersMenu.ActualHeight) > 0)
            {
                DoubleAnimation ManagersBtnAnimation = new DoubleAnimation();
                ManagersBtnAnimation.From = ManagersMenu.ActualHeight;
                ManagersBtnAnimation.To = 0;
                ManagersBtnAnimation.Duration = TimeSpan.FromSeconds(0.2);
                ManagersMenu.BeginAnimation(Grid.HeightProperty, ManagersBtnAnimation);
            }
            else
            {
                DoubleAnimation ManagersBtnAnimation = new DoubleAnimation();
                ManagersBtnAnimation.From = ManagersMenu.ActualHeight;
                ManagersBtnAnimation.To = 155;
                ManagersBtnAnimation.Duration = TimeSpan.FromSeconds(0.2);
                ManagersMenu.BeginAnimation(Grid.HeightProperty, ManagersBtnAnimation);
            }
        }
        // ============================ CREATE USER ============================
        private void AddUserBtn_Click(object sender, RoutedEventArgs e)
        {
            AddUser ad = new AddUser();
            ad.ShowDialog();
        }
        // ================================= = =================================

        // ================================= = =================================
        private void ManagersMenuClose()
        {
            DoubleAnimation ManagersBtnAnimation = new DoubleAnimation();
            ManagersBtnAnimation.From = ManagersMenu.ActualHeight;
            ManagersBtnAnimation.To = 0;
            ManagersBtnAnimation.Duration = TimeSpan.FromSeconds(0.2);
            ManagersMenu.BeginAnimation(Grid.HeightProperty, ManagersBtnAnimation);
        }
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
        // ================================= = =================================
        private void ProfileMenuClose()
        {
            DoubleAnimation ProfileBtnAnimation = new DoubleAnimation();
            ProfileBtnAnimation.From = ProfileMenu.ActualHeight;
            ProfileBtnAnimation.To = 0;
            ProfileBtnAnimation.Duration = TimeSpan.FromSeconds(0.2);
            ProfileMenu.BeginAnimation(Grid.HeightProperty, ProfileBtnAnimation);
        }
        // ================================= = =================================

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Получаем элемент, по которому был сделан клик
            var clickedElement = e.OriginalSource as DependencyObject;

            // Проверяем, был ли клик вне всех меню
            if (!IsDescendantOf(clickedElement, DatabaseMenu) &&
                !IsDescendantOf(clickedElement, ManagersMenu) &&
                !IsDescendantOf(clickedElement, ProfileMenu))
            {
                DatabaseMenuClose();
                ManagersMenuClose();
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
                ManagerControls();
            }
            else
            {
                if (labelText == "Developer") { MessageBox.Show("Welcome back Developer! All systems online", "Welcome back", MessageBoxButton.OK, MessageBoxImage.Information); }
                AdminMode = true;
                AdminControls();
            }
        }
        private void SoldControl()
        {
            string connectionString = CH.GetConnectionString();
            string query = $"SELECT MonthPlan FROM roles WHERE role = '{Role}'";
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
                AddErrorMessage($"Ошибка загрузки данных: {ex.Message}");
                SoldedLabel.Content = "0"; // Устанавливаем 0 при ошибке
            }
        }

        // ============================ ADMIN LOGIN ============================

        private void AdminControls()
        {
            ManagersButton.Visibility = Visibility.Visible;
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
            ManagersButton.Visibility = Visibility.Collapsed;
            AdminTabControl.Visibility = Visibility.Collapsed;
            AdminDatabaseGrid.Visibility = Visibility.Collapsed;
            MainDataGrid.Visibility = Visibility.Visible;
            GetMySQLTables(CH.GetConnectionString());
            LoadTablesIntoTabControl();
            LoadDataAndCreateCheckBoxes();
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
        private void LoadDataToLabel()
        {
            string connectionString = CH.GetConnectionString();
            string query = $"SELECT MonthPlan FROM roles WHERE role = '{Role}'";

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
                AddErrorMessage($"Ошибка загрузки данных: {ex.Message}"); ;
                MonthPlanLabel.Content = "0"; // Устанавливаем 0 при ошибке
            }
        }
        // ========================= ADMIN TABLES LOAD =========================
        private DataGrid CreateDataGridForTable(string connectionString, string tableName)
        {
            var dataGrid = new DataGrid
            {
                AutoGenerateColumns = true,
                SelectionMode = DataGridSelectionMode.Single,
                SelectionUnit = DataGridSelectionUnit.FullRow,
                IsReadOnly = true,
                Margin = new Thickness(5)
            };

            LoadTableData(dataGrid, connectionString, tableName);
            return dataGrid;
        }

        // Метод загрузки данных в DataGrid (без изменений)
        private void LoadTableData(DataGrid dataGrid, string connectionString, string tableName)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = $"SELECT * FROM `{tableName}`";
                    MySqlCommand command = new MySqlCommand(query, connection);

                    MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    dataGrid.ItemsSource = dataTable.DefaultView;
                }
            }
            catch (Exception ex)
            {
                AddErrorMessage($"Ошибка загрузки данных: {ex.Message}");
            }
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
                AddErrorMessage($"Ошибка при удалении данных: {ex.Message}");
            }
        }
        // Метод для обновления данных


        // Модифицированный метод загрузки данных
        private void LoadTablesIntoTabControl()
        {
            string connectionString = CH.GetConnectionString();
            var tables = GetMySQLTables(connectionString);

            // Создаем новый список вкладок
            var newTabs = new List<TabItem>();

            foreach (string tableName in tables)
            {
                var newTab = new TabItem
                {
                    Header = tableName,
                    Content = CreateDataGridForTable(connectionString, tableName)
                };
                newTabs.Add(newTab);
            }

            // Добавляем все вкладки за одну операцию
            AdminTabControl.Items.Clear();
            foreach (var tab in newTabs)
            {
                AdminTabControl.Items.Add(tab);
            }

            if (AdminTabControl.Items.Count > 0)
            {
                AdminTabControl.SelectedIndex = 0;
            }
        }

        // Создание DataGrid для таблицы
        
        private void MinimizeElements()
        {
            DatabaseMenu.Height = 0;
            ManagersMenu.Height = 0;
            ProfileMenu.Height = 0;
        }

        public void ReLoadData(string name = null, bool AdminMode = false)
        {
            SoldControl();
            if (AdminMode)
            {
                // Сохраняем индекс текущей вкладки
                int currentIndex = AdminTabControl.SelectedIndex;

                // Полностью очищаем TabControl
                AdminTabControl.Items.Clear();

                // Перезагружаем данные
                LoadTablesIntoTabControl();

                // Восстанавливаем выбранную вкладку (если возможно)
                if (currentIndex >= 0 && currentIndex < AdminTabControl.Items.Count)
                {
                    AdminTabControl.SelectedIndex = currentIndex;
                }
            }
            else
            {
                MainDataGrid.ItemsSource = null;
                try
                {
                    using (MySqlConnection connection = new MySqlConnection(CH.GetConnectionString()))  // MySqlConnection
                    {
                        connection.Open();

                        string query = CH.ManagerData(DBname);
                        using (MySqlCommand command = new MySqlCommand(query, connection))  // MySqlCommand
                        {
                            MySqlDataAdapter adapter = new MySqlDataAdapter(command);  // MySqlDataAdapter
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);

                            MainDataGrid.ItemsSource = dataTable.DefaultView;
                        }
                    }
                }
                catch (Exception ex)
                {
                    AddErrorMessage($"Ошибка загрузки данных: {ex.Message}");
                }
            }
        }

        


        private void ApplyColumnVisibility()
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


        // ==================== ERROR CONTROL (RUNNING LINE) ====================
        public enum ErrorLevel { Info, Warning, Error }
        public void AddErrorMessage(string message)
        {
            if (!string.IsNullOrEmpty(ErrorMarquee.Text))
            {
                ErrorMarquee.Text += " • " + message;
            }
            else
            {
                ErrorMarquee.Text = message;
            }

            // Перезапуск анимации
            RestartMarqueeAnimation();
        }

        // Очистка сообщений
        public void ClearErrorMessages()
        {
            ErrorMarquee.Text = "Готов к работе...";
            RestartMarqueeAnimation();
        }

        // Перезапуск анимации
        private void RestartMarqueeAnimation()
        {
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
            if (AdminMode)
            {
                try
                {
                    while (CheckBoxPanel.Children.Count > 0)
                    {
                        CheckBoxPanel.Children.RemoveAt(0);
                    }
                    using (MySqlConnection connection = new MySqlConnection(CH.GetConnectionString()))
                    {
                        connection.Open();

                        string query = $"SELECT * FROM `{GetTabName()}`"; // Замените YourTableName на имя вашей таблицы
                        MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection);
                        var allData = new DataTable();
                        adapter.Fill(allData);

                        // Создаем CheckBox'ы для каждого столбца
                        foreach (DataColumn column in allData.Columns)
                        {
                            CheckBox checkBox = new CheckBox
                            {
                                Content = column.ColumnName,
                                IsChecked = true, // Изначально все столбцы выбраны
                                Margin = new Thickness(10, 5, 5, 5),
                                FontSize = 15,
                                Style = (Style)FindResource("RoundedCheckBoxStyle"),
                                Tag = column.ColumnName // Используем Tag для хранения имени столбца
                            };
                            CheckBoxPanel.Children.Add(checkBox);
                        }

                        // Отображаем все данные в DataGrid
                        AdminTabControl.ItemsSource = allData.DefaultView;

                        // Автоматически генерируем столбцы, а потом скрываем ненужные
                        ApplyColumnVisibility(); // Применяем видимость столбцов сразу после загрузки
                    }
                }
                catch (MySqlException ex)
                {
                    AddErrorMessage($"Ошибка загрузки данных: {ex.Message}");
                }
                catch (Exception ex)
                {
                    AddErrorMessage($"Ошибка загрузки данных: {ex.Message}");
                }
            }
            else
            {
                try
                {
                    while (CheckBoxPanel.Children.Count > 0)
                    {
                        CheckBoxPanel.Children.RemoveAt(0);
                    }
                    using (MySqlConnection connection = new MySqlConnection(CH.GetConnectionString()))
                    {
                        connection.Open();

                        string query = CH.ManagerData(DBname); // Замените YourTableName на имя вашей таблицы
                        MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection);
                        var allData = new DataTable();
                        adapter.Fill(allData);

                        // Создаем CheckBox'ы для каждого столбца
                        foreach (DataColumn column in allData.Columns)
                        {
                            CheckBox checkBox = new CheckBox
                            {
                                Content = column.ColumnName,
                                IsChecked = true, // Изначально все столбцы выбраны
                                Margin = new Thickness(10, 5, 5, 5),
                                FontSize = 15,
                                Style = (Style)FindResource("RoundedCheckBoxStyle"),
                                Tag = column.ColumnName // Используем Tag для хранения имени столбца
                            };
                            CheckBoxPanel.Children.Add(checkBox);
                        }

                        // Отображаем все данные в DataGrid
                        MainDataGrid.ItemsSource = allData.DefaultView;

                        // Автоматически генерируем столбцы, а потом скрываем ненужные
                        MainDataGrid.AutoGenerateColumns = true;
                        ApplyColumnVisibility(); // Применяем видимость столбцов сразу после загрузки
                    }
                }
                catch (MySqlException ex)
                {
                    AddErrorMessage($"Ошибка загрузки данных: {ex.Message}");
                }
                catch (Exception ex)
                {
                    AddErrorMessage($"Ошибка загрузки данных: {ex.Message}");
                }
            }
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
            // Запоминаем имя текущей таблицы при смене вкладки
            LoadDataAndCreateCheckBoxes(true);
            LoadDataToLabel();
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

        private void EditUserBtn_Click(object sender, RoutedEventArgs e)
        {
            EditUser EU = new EditUser();
            EU.Show();
        }
    }

}