using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
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
    /// Логика взаимодействия для AddRole.xaml
    /// </summary>
    public partial class AddRole : Window
    {
        ConnectHelper CH = new ConnectHelper();
        public AddRole()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string query = $"INSERT INTO roles(role) value(@role);";
                using (MySqlConnection connection = new MySqlConnection(CH.GetConnectionString()))
                {
                    var cmd = new MySqlCommand(query, connection);

                    cmd.Parameters.AddWithValue("@role", RoleTextBox.Text);

                    connection.Open();
                    cmd.ExecuteNonQuery();
                }
                MessageBox.Show($"Роль {RoleTextBox.Text} успешно добавлена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (MySqlException ex) { MessageBox.Show("Ошибка при добавлении роли: " + ex.Message); }
        }
    }
}
