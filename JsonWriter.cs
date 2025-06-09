using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace ManagerAppV2._1
{
    public static class JsonConfigManager

    {

        private static string configPath = "consettings.json";

        // Сохранение настроек в JSON
        public static void SaveConfig(ConnectHelper config)
        {
            string json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(configPath, json);
        }

        // Загрузка настроек из JSON
        public static ConnectHelper LoadConfig()
        {
            if (File.Exists(configPath))
            {
                string json = File.ReadAllText(configPath);
                return JsonConvert.DeserializeObject<ConnectHelper>(json);
            }
            else
            {
                return null; // или вернуть значение по умолчанию
            }
        }
    }

}
