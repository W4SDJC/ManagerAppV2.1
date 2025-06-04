using Kursovaya2;
using MySql.Data.MySqlClient;
using System.Data;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
        private bool _isCtrlPressed = false;

        public MainWindow()
        {
            InitializeComponent();
            RoleControl();
            MinimizeElements();
            LoadDataAndCreateCheckBoxes();
            LoadDataToLabel();

        }

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
                MessageBox.Show(ex.Message);
                MonthPlanLabel.Content = "0"; // Устанавливаем 0 при ошибке
            }
        }

        private void SoldControl()
        {
            string connectionString = CH.GetConnectionString();
            string query = $"SELECT MonthPlan FROM roles WHERE role = '{Role}'";
            string query2 = $"SELECT SUM(ShipmentPrice) from {DBname}";
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
                MessageBox.Show(ex.Message);
                SoldedLabel.Content = "0"; // Устанавливаем 0 при ошибке
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
        private void RoleControl()
        {
            NameLabel.Content = Name;
            string labelText = RoleLabel.Content?.ToString(); // Получаем текст из Label, безопасно обрабатывая null

            if (labelText != "Dev" && labelText != "Admin")
            {
                ManagersButton.Visibility = Visibility.Collapsed;
                DevElements.Visibility = Visibility.Collapsed;

            }
            else
            {
                if (labelText == "Dev") { MessageBox.Show("Welcome back Developer! All systems online", "Welcome back", MessageBoxButton.OK, MessageBoxImage.Information); }
                ManagersButton.Visibility = Visibility.Visible;
                DevElements.Visibility = Visibility.Visible;

            }
        }

        private void MinimizeElements()
        {
            DatabaseMenu.Height = 0;
            ManagersMenu.Height = 0;
            ProfileMenu.Height = 0;
        }
        private void LoadDataAndCreateCheckBoxes(int mode = 0)
        {

            try
            {
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
                MessageBox.Show($"Ошибка при подключении к базе данных: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}");
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
            foreach (DataGridColumn column in MainDataGrid.Columns)
            {
                if (column.Header != null)
                {
                    column.Visibility = selectedColumns.Contains(column.Header.ToString()) ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }



        public void ReLoadData(string name = null)
        {
            SoldControl();
            if (name != null)
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
                    MessageBox.Show($"Error loading data: {ex.Message}");
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

                        string query = CH.ManagerData("manager");
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
                    MessageBox.Show($"Error loading data: {ex.Message}");
                }
            }
        }

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

        private void ProfileMenuClose()
        {
            DoubleAnimation ProfileBtnAnimation = new DoubleAnimation();
            ProfileBtnAnimation.From = ProfileMenu.ActualHeight;
            ProfileBtnAnimation.To = 0;
            ProfileBtnAnimation.Duration = TimeSpan.FromSeconds(0.2);
            ProfileMenu.BeginAnimation(Grid.HeightProperty, ProfileBtnAnimation);
        }

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
        private void ProfileBtn_Click(object sender, RoutedEventArgs e)
        {
            ProfilePage ProfilePageWindow = new ProfilePage();
            ProfilePageWindow.ShowDialog();
        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {

            LoginWindow LoginWindow = new LoginWindow();
            LoginWindow.Show();
        }

        private void ManagersMenuClose()
        {
            DoubleAnimation ManagersBtnAnimation = new DoubleAnimation();
            ManagersBtnAnimation.From = ManagersMenu.ActualHeight;
            ManagersBtnAnimation.To = 0;
            ManagersBtnAnimation.Duration = TimeSpan.FromSeconds(0.2);
            ManagersMenu.BeginAnimation(Grid.HeightProperty, ManagersBtnAnimation);
        }

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

        private void ApplyFilterButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyColumnVisibility();
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            ReLoadData(DBname);
            ApplyColumnVisibility();
        }

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

        private void AddManagerBtn_Click(object sender, RoutedEventArgs e)
        {
            AddUser ad = new AddUser();
            ad.ShowDialog();
        }



        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
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
                        string deleteQuery = $"DELETE FROM {DBname} WHERE (`id` = @id);\r\n";
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
        private void UpdateUser(int id, string name, string email)
        {
            using (MySqlConnection conn = new MySqlConnection(CH.GetConnectionString()))
            {
                conn.Open();
                string query = "UPDATE users SET name = @name, email = @email WHERE id = @id";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@id", id);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Данные успешно обновлены");
                    }
                    else
                    {
                        MessageBox.Show("Ошибка обновления данных");
                    }
                }
            }
        }



        public void AddData_Click(object sender, RoutedEventArgs e)
        {
            AddnEdit addnEdit = new AddnEdit("Add");
            addnEdit.ShowDialog();
            ReLoadData(DBname);
            ApplyColumnVisibility();
        }
        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            AddnEdit addnEdit = new AddnEdit("Edit");
            addnEdit.ShowDialog();
        }


        private bool _isMouseOverDataGrid = false;

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




    }
}