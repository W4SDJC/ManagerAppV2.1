﻿using ManagerAppV2;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using Org.BouncyCastle.Utilities.Encoders;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Reflection.PortableExecutable;


namespace ManagerAppV2._1
{
    /// <summary>
    /// Логика взаимодействия для AddUser.xaml
    /// </summary>
    public partial class AddUser : Window
    {
        public AddUser()
        {
            InitializeComponent();
            LoadComboBoxData();
        }
        ConnectHelper CH = new ConnectHelper();
        private void LoadComboBoxData()
        {

            string query = "SELECT DISTINCT role FROM `roles` WHERE id != 0;"; // DISTINCT для уникальных значений

            List<string> items = new List<string>();

            try
            {
                using (MySqlConnection connection = new MySqlConnection(CH.GetConnectionString()))
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
                }

                // Привязка данных к ComboBox
                RoleComboBox.ItemsSource = items;

                RoleComboBox.SelectedIndex = 0;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Ошибка MySQL: {ex.Message}");
            }
        }
        private string ValidateTextBox(TextBox textBox, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.BorderBrush = Brushes.Red;
                return $"Please, fill the field {fieldName}.\n";
            }
            return "";
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string dbname;
            ValidateTextBox(LoginTextBox, "Login");
            ValidateTextBox(NameTextBox, "Name");
            ValidateTextBox(PasswordTextBox, "Password");
            ValidateTextBox(ConfirmPasswordTextBox, "Password confirmation");
            if (string.IsNullOrEmpty(DataBaseNameTextBox.Text))
            {
                dbname = LoginTextBox.Text + RoleComboBox.SelectedItem.ToString();
            }
            else
            { dbname = DataBaseNameTextBox.Text; }
                //MessageBox.Show(dbname);
            if (ConfirmingPassword(PasswordTextBox.Text, ConfirmPasswordTextBox.Text))
            {
                string query = $"CREATE TABLE if not exists `{dbname}` (" +
                $"`id` int NOT NULL AUTO_INCREMENT," +
                $"`ShipmentDate` date DEFAULT NULL," +
                $"`ShipmentWarehouse` varchar(60) DEFAULT NULL," +
                $"`ClientCity` varchar(45) DEFAULT NULL," +
                $"`ClientName` varchar(45) DEFAULT NULL," +
                $"`ProductName` varchar(45) DEFAULT NULL," +
                $"`ProductAmount` varchar(45) DEFAULT NULL," +
                $"`UnitOfMeasurement` varchar(45) DEFAULT NULL," +
                $"`Price` int DEFAULT NULL," +
                $"`MinimumPrice` int DEFAULT NULL," +
                $"`ShipmentValue` int DEFAULT NULL," +
                $"`ShipmentValue(Minimum_price)` int DEFAULT NULL," +
                $"`Reward` int DEFAULT NULL," +
                $"`UPDNumber` int DEFAULT NULL," +
                $"`ShipmentPrice` int DEFAULT NULL," +
                $"PRIMARY KEY (`id`)," +
                $"UNIQUE KEY `id_UNIQUE` (`id`)," +
                $"UNIQUE KEY `UPDNumber_UNIQUE` (`UPDNumber`)" +
                $") ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;" +
                $"INSERT INTO users(`name`, `login`, `password`, `role`, `databasename`) VALUES ('{NameTextBox.Text}','{LoginTextBox.Text}', '{PasswordHasher.HashPassword(ConfirmPasswordTextBox.Text)}', '{RoleComboBox.SelectedItem}', '{dbname}');";
                try
                {
                    using (MySqlConnection connection = new MySqlConnection(CH.GetConnectionString()))
                    {
                        connection.Open();

                        using (MySqlCommand command = new MySqlCommand(query, connection))
                        {
                            int result = command.ExecuteNonQuery();
                            MessageBox.Show($"Таблица {dbname} успешно создана!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show($"Ошибка при создании таблицы:\n{ex.Message}", "Ошибка",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
            else
            {
                MessageBox.Show("Passwords don't match", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }
            

        private bool ConfirmingPassword(string password, string confirmPassword)
        {
            return password == confirmPassword;
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string password = PasswordTextBox.Text;
            string confirmPassword = ConfirmPasswordTextBox.Text;

            if (password != confirmPassword)
            {
                MessageBox.Show("Пароли не совпадают");
                return;
            }

            string hashedPassword = PasswordHasher.HashPassword(password);

            // Сохраняем в БД
            SaveUserToDatabase(LoginTextBox.Text, hashedPassword);
        }

        private void SaveUserToDatabase(string username, string hashedPassword)
        {
            using (MySqlConnection connection = new MySqlConnection(CH.GetConnectionString()))
            {
                var cmd = new MySqlCommand("INSERT INTO Users (Username, PasswordHash) VALUES (@username, @passwordHash)",connection);

                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@passwordHash", hashedPassword);

                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private void AddRole_Click(object sender, RoutedEventArgs e)
        {
            AddRole AR = new AddRole();
            AR.ShowDialog();
        }
    }
}