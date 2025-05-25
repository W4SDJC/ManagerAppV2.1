using MySql.Data.MySqlClient;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

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
            MinimizeElements();
            LoadDataAndCreateCheckBoxes();
            
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
                using (MySqlConnection connection = new MySqlConnection("server=localhost;port=3306;user=root;password=password;database=database;"))
                {
                    connection.Open();

                    string query = "SELECT " +
                        "id," +
                        "Shipment_date as \"Дата отправки\"," +
                        "Shipment_warehouse as \"Склад отправки\"," +
                        "\"Client's_city\" as \"Город покупателя\"," +
                        "\"Client's_name\" as \"Покупатель\"," +
                        "Product_name as \"Товар\"," +
                        "\"Product's_amount\" as \"Количество\"," +
                        "Unit_of_measurement as \"Ед. изм.\"," +
                        "Price as \"Цена менеджера\"," +
                        "Minimum_price as \"Мин. цена\"," +
                        "Shipment_value as \"Итого менеджера\"," +
                        "\"Shipment_value_(Minimum_price)\" as \"Итого (Мин)\"," +
                        "UPD_number as \"Номер УПД\"," +
                        "Shipment_price as \"Стоимость доставки\"," +
                        "Reward as Премия FROM `database`.`manager`;"; // Замените YourTableName на имя вашей таблицы
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
                            Margin = new Thickness(10, 5, 5, 0),
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
    


        private void LoadData()
        {
            MainDataGrid.ItemsSource = null;

            try
            {
                using (MySqlConnection connection = new MySqlConnection("server=localhost;port=3306;user=root;password=password;database=database;"))  // MySqlConnection
                {
                    connection.Open();

                    string query = "SELECT " +
                        "id," +
                        "Shipment_date as \"Дата отправки\"," +
                        "Shipment_warehouse as \"Склад отправки\"," +
                        "\"Client's_city\" as \"Город покупателя\"," +
                        "\"Client's_name\" as \"Покупатель\"," +
                        "Product_name as \"Товар\"," +
                        "\"Product's_amount\" as \"Количество\"," +
                        "Unit_of_measurement as \"Ед. изм.\"," +
                        "Price as \"Цена менеджера\"," +
                        "Minimum_price as \"Мин. цена\"," +
                        "Shipment_value as \"Итого менеджера\"," +
                        "\"Shipment_value_(Minimum_price)\" as \"Итого (Мин)\"," +
                        "UPD_number as \"Номер УПД\"," +
                        "Shipment_price as \"Стоимость доставки\"," +
                        "Reward as Премия FROM `database`.`manager`;";
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

        private void DatabaseButton_Click(object sender, RoutedEventArgs e)
        {
            if (((int)DatabaseMenu.ActualHeight) > 0)
            {
                DoubleAnimation DatabaseBtnAnimation = new DoubleAnimation();
                DatabaseBtnAnimation.From = DatabaseMenu.ActualHeight;
                DatabaseBtnAnimation.To = 0;
                DatabaseBtnAnimation.Duration = TimeSpan.FromSeconds(0.5);
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

        private void DatabaseButton_LostFocus(object sender, RoutedEventArgs e)
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

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (((int)ProfileMenu.ActualHeight) > 0) {
                DoubleAnimation ProfileBtnAnimation = new DoubleAnimation();
                ProfileBtnAnimation.From = ProfileMenu.ActualHeight;
                ProfileBtnAnimation.To = 0;
                ProfileBtnAnimation.Duration = TimeSpan.FromSeconds(0.2);
                ProfileMenu.BeginAnimation(Grid.HeightProperty, ProfileBtnAnimation);
            }else
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
                ManagersBtnAnimation.To = 65;
                ManagersBtnAnimation.Duration = TimeSpan.FromSeconds(0.2);
                ManagersMenu.BeginAnimation(Grid.HeightProperty, ManagersBtnAnimation);
            }
        }

        private void ApplyFilterButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyColumnVisibility();
        }

        private void GridSplitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            //  Сохраните новую ширину столбцов, если это необходимо
            //  Например, в настройках приложения:

            // double column1Width = ((Grid)this.Content).ColumnDefinitions[0].ActualWidth;
            // Properties.Settings.Default.Column1Width = column1Width;
            // Properties.Settings.Default.Save();
        }
    }
}