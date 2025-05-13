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
            CreateCheckBoxesForColumns();
            MinimizeElements();
            LoadData();
            
        }

        private void MinimizeElements()
        {
            DatabaseMenu.Height = 0;
            ProfileMenu.Height = 0;
        }
        private void LoadData()
        {
            MainDataGrid.ItemsSource = null;

            try
            {
                using (MySqlConnection connection = new MySqlConnection("server=localhost;port=3306;user=root;password=password;database=database;"))  // MySqlConnection
                {
                    connection.Open();

                    string query = "SELECT id, Shipment_date as \"Дата отправки\", Shipment_warehouse as \"Склад отправки\", \"Client's_city\" as \"Город покупателя\", \"Client's_name\" as \"Покупатель\", Product_name as \"Товар\", \"Product's_amount\" as \"Количество\", Unit_of_measurement as \"Ед. изм.\", Price as \"Цена менеджера\", Minimum_price as \"Мин. цена\", Shipment_value as \"Итого менеджера\", \"Shipment_value_(Minimum_price)\" as \"Итого (Мин)\", UPD_number as \"Номер УПД\", Shipment_price as \"Стоимость доставки\", Reward as Премия FROM `database`.`manager`;";
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
            if (((int)ProfileMenu.Height) > 0) {
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

        

    }
}