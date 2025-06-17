namespace ManagerAppV4._0
{
    public static class DataSource
    {
        public static string DataBase { get; set; }
        public static string UserName { get; set; }
        public static string Role { get; set; }
        public static string Login { get; set; }
        public static string DBname { get; set; }

        // Метод для очистки сессии
        public static void Clear()
        {
            DataBase = null;
            UserName = null;
            Role = null;
            Login = null;
            DBname = null;
        }
    }
}
