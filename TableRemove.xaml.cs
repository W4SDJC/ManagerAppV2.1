using MySql.Data.MySqlClient;
using System.Windows;
using System.Windows.Input;

namespace ManagerAppV4._0
{
    public partial class TableRemove : Window
    {
        ConnectHelper CH = new ConnectHelper();

        public TableRemove()
        {
            InitializeComponent();
            LoadTableNames();
        }
        private void LoadTableNames()
        {
            string query = "SHOW TABLES;";
            List<string> tables = new List<string>();

            try
            {
                using (MySqlConnection connection = new MySqlConnection(CH.GetConnectionString()))
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tables.Add(reader.GetString(0));
                        }
                    }
                }

                TablesComboBox.ItemsSource = tables;
                if (tables.Count > 0)
                    TablesComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки таблиц: " + ex.Message);
            }
        }
        private void DeleteTableButton_Click(object sender, RoutedEventArgs e)
        {
            if (TablesComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите таблицу для удаления.");
                return;
            }

            string selectedTable = TablesComboBox.SelectedItem.ToString();
            var result = MessageBox.Show($"Вы действительно хотите удалить таблицу '{selectedTable}'?","Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (MySqlConnection connection = new MySqlConnection(CH.GetConnectionString()))
                    {
                        connection.Open();
                        string query = $"DROP TABLE `{selectedTable}`;";
                        using (MySqlCommand cmd = new MySqlCommand(query, connection))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show($"Таблица '{selectedTable}' успешно удалена.", "Успех");
                    LoadTableNames(); // обновить список после удаления
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при удалении таблицы: " + ex.Message);
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
