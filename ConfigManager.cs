using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;

namespace ManagerAppV2._1
{
    class ConfigManager
    {
        private static readonly string ConfigFilePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "server.config");

        // Сохраняем адрес сервера в файл
        public static void SaveServerAddress(string serverAddress)
        {
            try
            {
                File.WriteAllText(ConfigFilePath, serverAddress);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось сохранить настройки: {ex.Message}");
            }
        }

        // Загружаем адрес сервера из файла
        public static string LoadServerAddress()
        {
            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    return File.ReadAllText(ConfigFilePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось загрузить настройки: {ex.Message}");
            }

            return "localhost"; // Значение по умолчанию
        }
    }
}
