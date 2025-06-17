public static class TabTranslations
{
    // Словарь для перевода технических названий в пользовательские
    private static readonly Dictionary<string, string> _techToUser = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        {"warehouses", "Склады"},
        {"users", "Пользователи"},
        {"product price", "Товары"},
        {"roles", "Роли"},
        // Добавьте другие таблицы по необходимости
    };

    // Обратный словарь для перевода пользовательских названий в технические
    private static readonly Dictionary<string, string> _userToTech;

    static TabTranslations()
    {
        // Инициализация обратного словаря
        _userToTech = _techToUser.ToDictionary(
            pair => pair.Value,
            pair => pair.Key,
            StringComparer.OrdinalIgnoreCase);
    }

    // Получение пользовательского названия по техническому
    public static string GetUserFriendlyName(string technicalName)
    {
        return _techToUser.TryGetValue(technicalName, out var userFriendlyName)
            ? userFriendlyName
            : technicalName;
    }

    // Получение технического названия по пользовательскому
    public static string GetTechnicalName(string userFriendlyName)
    {
        return _userToTech.TryGetValue(userFriendlyName, out var technicalName)
            ? technicalName
            : userFriendlyName;
    }

    // Добавление нового перевода
    public static void AddTranslation(string technicalName, string userFriendlyName)
    {
        _techToUser[technicalName] = userFriendlyName;
        _userToTech[userFriendlyName] = technicalName;
    }

    // Проверка наличия перевода для технического названия
    public static bool ContainsTechnicalName(string technicalName)
    {
        return _techToUser.ContainsKey(technicalName);
    }

    // Проверка наличия перевода для пользовательского названия
    public static bool ContainsUserFriendlyName(string userFriendlyName)
    {
        return _userToTech.ContainsKey(userFriendlyName);
    }
}