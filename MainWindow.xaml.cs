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
        public MainWindow()
        {
            InitializeComponent();
            RoleControl();
            MinimizeElements();
            LoadDataAndCreateCheckBoxes();
            LoadDataToLabel();
        }
        ConnectHelper CH = new ConnectHelper();

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
            string query = "SELECT MonthPlan FROM roles WHERE role = 'Manager'";

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    MySqlCommand command = new MySqlCommand(query, connection);
                    object result = command.ExecuteScalar();
                    int convResult = Convert.ToInt32(result);
                    var culture = new CultureInfo("en-US");

                    if (result != null)
                    {
                        MonthPlanLabel.Content = convResult.ToString("N0", culture);
                    }
                    else
                    {
                        MonthPlanLabel.Content = "Данные не найдены";
                    }
                }
                SoldControl();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void SoldControl()
        {
            string connectionString = CH.GetConnectionString();
            string query = "SELECT MonthPlan FROM roles WHERE role = 'Manager'";
            string query2 = "SELECT SUM(ShipmentPrice) from manager";
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
                    int ConvMonthPlan = Convert.ToInt32(MonthPlan);
                    int ConvResult = Convert.ToInt32(result);
                    if (ConvResult < ConvMonthPlan / 2)
                    {
                        var color = (Color)ColorConverter.ConvertFromString("#FFC00F0C");
                        SoldedLabel.Foreground = new SolidColorBrush(color);
                    }
                    else
                    {
                        if (ConvResult > ConvMonthPlan / 2&& ConvResult < ConvMonthPlan)
                        {
                            var color = (Color)ColorConverter.ConvertFromString("#FFD324");
                            SoldedLabel.Foreground = new SolidColorBrush(color);
                        }
                        else {
                            var color = (Color)ColorConverter.ConvertFromString("#14AE5C");
                            SoldedLabel.Foreground = new SolidColorBrush(color);
                        }
                    }

                    SoldedLabel.Content = ConvResult.ToString("N0", culture);
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);
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
            string labelText = RoleLabel.Content?.ToString(); // Получаем текст из Label, безопасно обрабатывая null

            if (labelText != "Dev" && labelText != "Admin")
            {
                ManagersButton.Visibility = Visibility.Collapsed;
                FilterAllElements.Visibility = Visibility.Collapsed;

            }
            else
            {
                if (labelText == "Dev") { MessageBox.Show("Welcome back Developer! All systems online", "Welcome back", MessageBoxButton.OK, MessageBoxImage.Information); }
                ManagersButton.Visibility = Visibility.Visible;
                FilterAllElements.Visibility = Visibility.Visible;

            }
        }

        private void MinimizeElements()
        {
            DatabaseMenu.Height = 0;
            ManagersMenu.Height = 0;
            ProfileMenu.Height = 0;
        }
        private void LoadDataAndCreateCheckBoxes()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(CH.GetConnectionString()))
                {
                    connection.Open();

                    string query = CH.ManagerData("manager"); // Замените YourTableName на имя вашей таблицы
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



        public void LoadData(string name = null)
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

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (((int)ProfileMenu.ActualHeight) > 0) {
                DoubleAnimation ProfileBtnAnimation = new DoubleAnimation();
                ProfileBtnAnimation.From = ProfileMenu.ActualHeight;
                ProfileBtnAnimation.To = 0;
                ProfileBtnAnimation.Duration = TimeSpan.FromSeconds(0.2);
                ProfileMenu.BeginAnimation(Grid.HeightProperty, ProfileBtnAnimation);
            } else
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
            LoadData("manager");
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

        private void CheckBoxGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
        }

        private void FilterAllElements_Checked(object sender, RoutedEventArgs e)
        {
            LoadData("manager0");
        }

        private void NewBtn1_Click(object sender, RoutedEventArgs e)
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
                        string deleteQuery = $"DELETE FROM `database`.`manager` WHERE (`id` = @id);\r\n";
                        MySqlCommand command = new MySqlCommand(deleteQuery, connection);
                        command.Parameters.AddWithValue("@id", id);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Запись успешно удалена");
                            LoadData();
                        }
                    }
                } catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            LoadData("manager");
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

        

        public void NewBtn_Click(object sender, RoutedEventArgs e)
        {
            
            
            AddnEdit addnEdit = new AddnEdit("Add");
            addnEdit.ShowDialog();
            LoadData("manager");
            ApplyColumnVisibility();
        }
        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            AddnEdit addnEdit = new AddnEdit("Edit");
            addnEdit.ShowDialog();
        }
    }
}