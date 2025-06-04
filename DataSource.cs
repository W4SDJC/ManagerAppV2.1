using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagerAppV2._1
{
    public static class DataSource
    {
        public static string UserName { get; set; }
        public static string Role { get; set; }
        public static string Login { get; set; }
        public static string DBname { get; set; }

        // Метод для очистки сессии
        public static void Clear()
        {
            UserName = null;
            Role = null;
            Login = null;
            DBname = null;
        }
    }
}
