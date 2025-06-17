using ClosedXML.Excel;
using Microsoft.Win32;
using MySql.Data.MySqlClient;
using System.Collections;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace ManagerAppV4._0
{

    public partial class MainWindow : Window
    {
        ConnectHelper CH = new ConnectHelper();
        private string Name = DataSource.UserName;
        private string Login = DataSource.Login;
        private string Role = DataSource.Role;
        public string Tablename = DataSource.DBname;
        private bool AdminMode = false;
        private int FSize = 16;
        private bool _isMouseOverDataGrid = false;
        private bool _isCtrlPressed = false;

        public MainWindow()
        {
            InitializeComponent();
            MainLoad();
        }

        // =========================== DATABASE MENU ===========================
        #region DATABASE MENU
        private void DatabaseMenuOpen(object sender, RoutedEventArgs e)
        {
            if (AdminMode)
            {
                if (DatabaseMenu.ActualHeight > 0)
                {
                    AnimateMenu(DatabaseMenu, 0);
                    SetMenuIcon(DataBaseBtnimg, "/Icons/ArrowDown.png");
                }
                else
                {
                    AnimateMenu(DatabaseMenu, 125);
                    SetMenuIcon(DataBaseBtnimg, "/Icons/ArrowUp.png");
                }
            }
            else
            {
                if (DatabaseMenu.ActualHeight > 0)
                {
                    AnimateMenu(DatabaseMenu, 0);
                    SetMenuIcon(DataBaseBtnimg, "/Icons/ArrowDown.png");
                }
                else
                {
                    AnimateMenu(DatabaseMenu, 95);
                    SetMenuIcon(DataBaseBtnimg, "/Icons/ArrowUp.png");
                }
            }
        }

        // ======================== ADD DATA TO DATABASE ========================
        public void AddData_Click(object sender, RoutedEventArgs e)
        {
            if (AdminMode)
            {
                AddnEdit addnEdit = new AddnEdit("Add", TabTranslations.GetTechnicalName(GetTabName()));
                addnEdit.ShowDialog();
                LoadDataToLabel(TabTranslations.GetTechnicalName(GetTabName()));
                UpdateAll();
            }
            else
            {
                AddnEdit addnEdit = new AddnEdit("Add", Tablename);
                addnEdit.ShowDialog();
                UpdateAll();
            }
            ReLoadData(Tablename);
            ApplyColumnVisibility();
        }

        // ============================= EDIT DATA =============================
        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            if (AdminMode)
            {
                TabItem selectedTab = AdminTabControl.SelectedItem as TabItem;
                if (selectedTab?.Content is DataGrid dataGrid && dataGrid.SelectedItem is DataRowView row)
                {
                    var specialTables = new List<string> { "product price", "roles", "users", "warehouses" };

                    if (selectedTab != null && !specialTables.Contains(TabTranslations.GetTechnicalName(GetTabName())))
                    {
                        var dataGrid1 = selectedTab.Content as DataGrid;
                        if (dataGrid1 != null && dataGrid1.SelectedItem != null)
                        {
                            // Приводим к DataRowView (если ItemsSource — DataView):
                            var rowView = dataGrid.SelectedItem as DataRowView;
                            if (rowView != null)
                            {
                                // Получаем значение столбца 'id':
                                string idValue = rowView["id"].ToString();
                                var selectedData = row.Row; // DataRow с доступом по именам колонок
                                AddnEdit editWindow = new AddnEdit("Edit", TabTranslations.GetTechnicalName(GetTabName()), row.Row, idValue, true); // передаём DataRow
                                editWindow.ShowDialog();
                                UpdateAll();
                            }
                        }
                    }
                    else { MessageBox.Show("Выберите таблицу пользователя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
                }
                else { MessageBox.Show("Выберите строку для изменения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Information); }

            }
            else
            {
                if (MainDataGrid.SelectedItem == null)
                {
                    MessageBox.Show("Выберите строку для изменения");
                    return;
                }
                DataRowView selectedRow = (DataRowView)MainDataGrid.SelectedItem;
                string id = selectedRow["id"].ToString();

                var editWindow = new AddnEdit("Edit",Tablename, null, id); 
                editWindow.ShowDialog();
                UpdateAll();
            }
        }
        // ============================ DELETE DATA ============================
        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Role == "Admin" || Role == "Developer")
            {
                AdminDelete();
                // Получаем текущую вкладку
                var currentTab = AdminTabControl.SelectedItem as TabItem;

                // Ищем DataGrid внутри вкладки
                if (currentTab?.Content is StackPanel panel)
                {
                    var dataGrid = panel.Children.OfType<DataGrid>().FirstOrDefault();

                    if (dataGrid?.SelectedItem != null && dataGrid.ItemsSource is IList list)
                    {
                        list.Remove(dataGrid.SelectedItem);
                    }
                }
                UpdateAll();
            }
            else
            {
                DeleteData();
                UpdateAll();
            }
        }
        // ============================ EXPORT DATA ============================
        #region Export Button
        private void ExportBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dataTable = null;
                string fileName = "Export.xlsx";
                var selectedTab = AdminTabControl.SelectedItem as TabItem;
                var specialTables = new List<string> { "product price", "roles", "users", "warehouses" };

                if (!specialTables.Contains(TabTranslations.GetTechnicalName(GetTabName())))
                {
                    if (selectedTab == null)
                    {
                        MessageBox.Show("Не выбрана вкладка.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var dataGrid = selectedTab.Content as DataGrid;
                    if (dataGrid == null || dataGrid.ItemsSource == null)
                    {
                        MessageBox.Show("В таблице нет данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var dataView = dataGrid.ItemsSource as DataView;
                    if (dataView == null)
                    {
                        MessageBox.Show("Источник данных некорректен.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    dataTable = dataView.ToTable();
                    fileName = selectedTab.Header.ToString() + ".xlsx";


                    SaveFileDialog saveFileDialog = new SaveFileDialog
                    {
                        Filter = "Excel файл (*.xlsx)|*.xlsx",
                        Title = "Сохранить таблицу как Excel",
                        FileName = fileName
                    };

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        using (XLWorkbook workbook = new XLWorkbook())
                        {
                            // 1. Добавляем основной лист с данными
                            var wsData = workbook.Worksheets.Add(dataTable, "Отчет");

                            // Добавляем итоговые строки в конец таблицы
                            int lastRow = wsData.LastRowUsed().RowNumber();
                            int firstDataRow = 2; // предполагая, что строка 1 - это заголовки

                            // Находим индексы столбцов по названиям
                            int totalCol = -1, totalMinCol = -1, deliveryCol = -1;
                            for (int i = 1; i <= wsData.ColumnCount(); i++)
                            {
                                string header = wsData.Cell(1, i).Value.ToString();
                                if (header.Contains("Итого менеджера")) totalCol = i;
                                if (header.Contains("Итого (Мин)")) totalMinCol = i;
                                if (header.Contains("Стоимость доставки")) deliveryCol = i;
                            }

                            // Добавляем итоговые строки
                            if (totalCol > 0 || totalMinCol > 0 || deliveryCol > 0)
                            {
                                lastRow++;
                                wsData.Cell(lastRow, 1).Value = "ИТОГО:";

                                if (totalCol > 0)
                                {
                                    wsData.Cell(lastRow, totalCol).FormulaA1 = $"SUM({wsData.Column(totalCol).Cell(firstDataRow).Address}:{wsData.Column(totalCol).Cell(lastRow - 1).Address})";
                                }

                                if (totalMinCol > 0)
                                {
                                    wsData.Cell(lastRow, totalMinCol).FormulaA1 = $"SUM({wsData.Column(totalMinCol).Cell(firstDataRow).Address}:{wsData.Column(totalMinCol).Cell(lastRow - 1).Address})";
                                }

                                if (deliveryCol > 0)
                                {
                                    wsData.Cell(lastRow, deliveryCol).FormulaA1 = $"SUM({wsData.Column(deliveryCol).Cell(firstDataRow).Address}:{wsData.Column(deliveryCol).Cell(lastRow - 1).Address})";
                                }

                                // Форматируем итоговую строку
                                var totalRange = wsData.Range(lastRow, 1, lastRow, wsData.ColumnCount());
                                totalRange.Style.Font.Bold = true;
                                totalRange.Style.Fill.BackgroundColor = XLColor.LightGreen;
                                totalRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                            }

                            // 2. Создаем лист с расчетом премии с точным форматированием
                            var wsBonus = workbook.Worksheets.Add("Расчет премии");


                            // Установка ширины столбца E (5) в 3 см 
                            wsBonus.Column(5).Width = 12.6;
                            // Цвета для форматирования
                            var lightGreen = XLColor.FromArgb(51, 191, 86);
                            var lightGray = XLColor.FromArgb(217, 217, 217);
                            var lightYellow = XLColor.FromHtml("#ffe699");


                            wsBonus.Cell(2, 7).Value = $"План продаж на {GetMonth().ToString()}";
                            SetNumberCell(wsBonus, 3, 7, NormalizeNumberStringToDecimal(MonthPlanLabel.Content.ToString()));
                            wsBonus.Cell(3, 7).Style.NumberFormat.Format = "#,##0.00";
                            wsBonus.Range("G2:I2").Merge();
                            SetNumberCell(wsBonus, 7, 7, NormalizeNumberStringToDecimal(MonthPlanLabel.Content.ToString()));
                            wsBonus.Cell(7, 7).Style.NumberFormat.Format = "#,##0.00";
                            SetNumberCell(wsBonus, 7, 8, NormalizeNumberStringToDecimal(SoldedLabel.Content.ToString()));
                            wsBonus.Cell(7, 8).Style.NumberFormat.Format = "#,##0.00";
                            wsBonus.Cell(7, 9).FormulaA1 = "H7/G7*100";
                            var range3 = wsBonus.Range("G2:I3");
                            range3.Style.Border.OutsideBorder = XLBorderStyleValues.Medium; // внешняя рамка
                            range3.Style.Border.InsideBorder = XLBorderStyleValues.Medium;

                            // Заголовки таблиц

                            wsBonus.Cell(2, 4).Value = "Условие соблюдения ценовой политики (УСЦП)";
                            wsBonus.Range("D2:E2").Merge();
                            wsBonus.Cell(3, 4).Value = "Условие";
                            wsBonus.Cell(3, 5).Value = "% вознаграждения от выручки";
                            wsBonus.Cell(4, 4).Value = "равно минимальной стоимости товара за вычетом ЦР";
                            wsBonus.Range("D4:D5").Merge();
                            wsBonus.Cell(4, 5).Value = 2;
                            wsBonus.Range("E4:E5").Merge();
                            wsBonus.Cell(6, 4).Value = "больше минимальной стоимости товара за вычетом ЦР";
                            wsBonus.Range("D6:D7").Merge();
                            wsBonus.Cell(6, 5).Value = 3;
                            wsBonus.Range("E6:E7").Merge();
                            var range2 = wsBonus.Range("D2:E7");
                            range2.Style.Border.OutsideBorder = XLBorderStyleValues.Medium; // внешняя рамка
                            range2.Style.Border.InsideBorder = XLBorderStyleValues.Medium;

                            wsBonus.Range("G3:I3").Merge();

                            // Форматирование заголовков
                            wsBonus.Range("A2:K2").Style.Font.Bold = true;
                            wsBonus.Range("A2:K2").Style.Font.FontSize = 12;
                            wsBonus.Range("A2:K2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            wsBonus.Range("A2:K2").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                            wsBonus.Range("A3:B3").Style.Fill.BackgroundColor = lightGreen;
                            wsBonus.Range("D2:E2").Style.Fill.BackgroundColor = lightGreen;
                            wsBonus.Range("D3:E3").Style.Fill.BackgroundColor = lightGreen;
                            wsBonus.Range("B4:B9").Style.Fill.BackgroundColor = lightGreen;
                            wsBonus.Range("A2:B2").Style.Fill.BackgroundColor = lightGreen;
                            wsBonus.Range("E4:E7").Style.Fill.BackgroundColor = lightGreen;
                            wsBonus.Range("G2:I2").Style.Fill.BackgroundColor = lightGreen;
                            wsBonus.Range("G3:I3").Style.Fill.BackgroundColor = lightYellow;
                            wsBonus.Range("A3:K3").Style.Font.Bold = true;

                            // Устанавливаем автоперенос слов + центрирование по вертикали и горизонтали для всего листа "Расчет премии"
                            wsBonus.Cells().Style.Alignment.WrapText = true;
                            wsBonus.Cells().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            wsBonus.Cells().Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                            // Таблица выполнения плана
                            wsBonus.Cell(2, 1).Value = "Выполнение плана продаж";
                            wsBonus.Range("A2:B2").Merge();
                            wsBonus.Cell(3, 1).Value = "% выполнения плана продаж";
                            wsBonus.Cell(3, 2).Value = "Коэффициент вознаграждения (кВ)";
                            wsBonus.Cell(4, 1).Value = "От 50% до 70%";
                            wsBonus.Cell(4, 2).Value = 0.00;
                            wsBonus.Cell(5, 1).Value = "70-80%";
                            wsBonus.Cell(5, 2).Value = 0.80;
                            wsBonus.Cell(6, 1).Value = "80-90%";
                            wsBonus.Cell(6, 2).Value = 0.90;
                            wsBonus.Cell(7, 1).Value = "90-100%";
                            wsBonus.Cell(7, 2).Value = 1.00; 
                            wsBonus.Cell(8, 1).Value = "110-125%";
                            wsBonus.Cell(8, 2).Value = 1.20; 
                            wsBonus.Cell(9, 1).Value = "Более 125%";
                            wsBonus.Cell(9, 2).Value = 1.30;
                            var range1 = wsBonus.Range("A2:B9");
                            range1.Style.Border.OutsideBorder = XLBorderStyleValues.Medium; // внешняя рамка
                            range1.Style.Border.InsideBorder = XLBorderStyleValues.Medium;


                            wsBonus.Cell(5, 7).Value = "Коэффициент вознаграждения";
                            wsBonus.Range("G5:I5").Merge();
                            wsBonus.Cell(6, 7).Value = "план";
                            wsBonus.Cell(6, 8).Value = "факт";
                            wsBonus.Cell(6, 9).Value = "% выполнения";
                            wsBonus.Range("G5:I5").Style.Fill.BackgroundColor = lightGreen;
                            wsBonus.Range("G6:I6").Style.Fill.BackgroundColor = lightGreen;
                            wsBonus.Range("G7:I7").Style.Fill.BackgroundColor = lightYellow;
                            var range = wsBonus.Range("G5:I7");
                            range.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
                            range.Style.Border.InsideBorder = XLBorderStyleValues.Medium;


                            string[] summaryHeaders = {
                            "Сумма отгрузки за месяц",
                            "Сумма транспортных расходов за месяц",
                            "Сумма отгрузки за вычетом транспорта",
                            "Сумма отгрузки в мин.ценах за вычетом транспорта",
                            "расчет по УСЦП",
                            "Расчет премии с учетом кВ без НДФЛ",
                            "Оклад без НДФЛ",
                            "Итого без НДФЛ",
                            "Выплачен аванс без НДФЛ",
                            "Подрасчет без НДФЛ"
                        };

                            // Заголовки таблицы (обязательно перенос слов + выравнивание)
                            for (int i = 0; i < summaryHeaders.Length; i++)
                            {
                                var cell = wsBonus.Cell(12, i + 1);
                                cell.Value = summaryHeaders[i];
                                cell.Style.Alignment.WrapText = true; // Перенос слов для каждого заголовка
                                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            }

                            // Форматирование заголовков таблицы
                            wsBonus.Range("A12:J12").Style.Fill.BackgroundColor = lightGreen;
                            wsBonus.Range("A12:J12").Style.Font.Bold = true;
                            wsBonus.Range("A12:J12").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            // Данные таблицы
                            if (totalCol > 0)
                                wsBonus.Cell(13, 1).FormulaA1 = "Отчет!" + wsData.Column(totalCol).Cell(lastRow).Address;
                            else
                                wsBonus.Cell(13, 1).Value = 0;

                            if (deliveryCol > 0)
                                wsBonus.Cell(13, 2).FormulaA1 = "Отчет!" + wsData.Column(deliveryCol).Cell(lastRow).Address;
                            else
                                wsBonus.Cell(13, 2).Value = 0;

                            wsBonus.Cell(13, 3).FormulaA1 = "A13-B13";

                            if (totalMinCol > 0 && deliveryCol > 0)
                                wsBonus.Cell(13, 4).FormulaA1 = $"Отчет!{wsData.Column(totalMinCol).Cell(lastRow).Address}-Отчет!{wsData.Column(deliveryCol).Cell(lastRow).Address}";
                            else
                                wsBonus.Cell(13, 4).Value = 0;

                            wsBonus.Cell(13, 5).FormulaA1 = "D13*IF(C13=D13,$E$4,$E$6)/100";
                            wsBonus.Cell(13, 6).FormulaA1 = "E13*VLOOKUP(H6,$A$4:$B$9,2,TRUE)";
                            wsBonus.Cell(13, 7).Value = $"{CH.GetOklad(TabTranslations.GetTechnicalName(GetTabName()))}";
                            wsBonus.Cell(13, 8).FormulaA1 = "G13+F13";

                            wsBonus.Cell(13, 9).Value = 16000;
                            wsBonus.Cell(13, 10).FormulaA1 = "H13-I13";

                            // Границы таблицы итогов
                            var calculationTable = wsBonus.Range("A12:J13");
                            calculationTable.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
                            calculationTable.Style.Border.InsideBorder = XLBorderStyleValues.Medium;

                            // Числовые форматы
                            wsBonus.Range("B4:B9").Style.NumberFormat.Format = "0.00";
                            wsBonus.Range("E4:E6").Style.NumberFormat.Format = "0";
                            wsBonus.Range("F6:H6").Style.NumberFormat.Format = "#,##0.00";
                            wsBonus.Cell(6, 8).Style.NumberFormat.Format = "0.00";
                            wsBonus.Range("A13:J13").Style.NumberFormat.Format = "#,##0.00";

                            // Выравнивание
                            wsBonus.Range("G5:I7").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            wsBonus.Range("A2:B9").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            wsBonus.Range("F4:H6").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            wsBonus.Range("D2:E7").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            wsBonus.Range("A12:J13").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            // Автоподбор ширины столбцов
                            wsBonus.Columns().AdjustToContents();
                            wsBonus.Rows().AdjustToContents();
                            // Сохраняем файл
                            workbook.SaveAs(saveFileDialog.FileName);

                        }
                    MessageBox.Show("Экспорт завершен успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    
                }
                else
                {
                    // --- Экспорт из выбранной вкладки TabControl ---
                    selectedTab = AdminTabControl.SelectedItem as TabItem;
                    if (selectedTab == null)
                    {
                        MessageBox.Show("Не выбрана вкладка.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var dataGrid = selectedTab.Content as DataGrid;
                    if (dataGrid == null || dataGrid.ItemsSource == null)
                    {
                        MessageBox.Show("В таблице нет данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var dataView = dataGrid.ItemsSource as DataView;
                    if (dataView == null)
                    {
                        MessageBox.Show("Источник данных некорректен.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    dataTable = dataView.ToTable();
                    fileName = TabTranslations.GetTechnicalName(GetTabName()) + ".xlsx";
                    // Диалог выбора пути сохранения
                    SaveFileDialog saveFileDialog = new SaveFileDialog
                    {
                        Filter = "Excel файл (*.xlsx)|*.xlsx",
                        Title = "Сохранить таблицу как Excel",
                        FileName = fileName
                    };
                    if (saveFileDialog.ShowDialog() == true)
                    {
                        using (XLWorkbook workbook = new XLWorkbook())
                        {
                            workbook.Worksheets.Add(dataTable, "Данные");
                            workbook.SaveAs(saveFileDialog.FileName);
                        }

                        MessageBox.Show("Экспорт завершен успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public decimal NormalizeNumberStringToDecimal(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                MessageBox.Show("Пустая или null строка. Возвращено 0.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return 0m;
            }

            string noDots = input.Replace(".", "");
            string normalized = noDots.Replace(",", ".");

            if (decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
            {
                return result;
            }
            else
            {
                MessageBox.Show($"Ошибка преобразования строки в decimal:\nОригинал: \"{input}\"\nПосле обработки: \"{normalized}\"", "Ошибка парсинга", MessageBoxButton.OK, MessageBoxImage.Error);
                return 0m;
            }
        }
        public void SetNumberCell(IXLWorksheet sheet, int row, int column, decimal number)
        {
            try
            {
                IXLCell cell = sheet.Cell(row, column);
                cell.Value = Convert.ToDouble(number); // Устанавливаем как число
                cell.Style.NumberFormat.Format = "0.00"; // Формат с двумя знаками после запятой
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при установке числа в ячейку [{row},{column}]: {ex.Message}");
            }
        }

        #endregion

        private void DatabaseMenuClose()
        {
            AnimateMenu(DatabaseMenu, 0);
            SetMenuIcon(DataBaseBtnimg, "/Icons/ArrowDown.png");
        }
        #endregion
        // =========================== UPDATE BUTTON ===========================
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateAll();
        }
        public void UpdateAll()
        {
            ClearErrorMessages();
            ReLoadData(Tablename, AdminMode);
            ApplyColumnVisibility();
            LoadDataToLabel(Tablename);
            GetMonthIntoLabel();
            LoadDataToLabel(TabTranslations.GetTechnicalName(GetTabName()));
        }
        // ============================= EDIT MENU =============================
        #region EDIT MENU
        private void ManagersButton_Click(object sender, RoutedEventArgs e)
        {
           if ((int)EditMenu.ActualHeight > 0)
            {
                AnimateMenu(EditMenu, 0);
                SetMenuIcon(EditBtnIMG, "/Icons/ArrowDown.png");
            }
            else
            {
                AnimateMenu(EditMenu, 155);
                SetMenuIcon(EditBtnIMG, "/Icons/ArrowUp.png");
            }
 
        }

        // ============================= USER MENU =============================
        #region USER MENU
        private void UsersMenu_Click(object sender, RoutedEventArgs e)
        {
            if (UserMenu.ActualHeight > 0)
            {
                AnimateMenu(UserMenu, 0);
            }
            else
            {
                ProductMenuClose();
                WarehouseMenuClose();
                AnimateMenu(UserMenu, 65);
            }
        }
        // ============================ CREATE USER ============================
        
        private void AddUserBtn_Click(object sender, RoutedEventArgs e)
        {
            AddUser ad = new AddUser();
            ad.ShowDialog();
        }
        // ============================= EDIT USER =============================
        private void EditUserBtn_Click(object sender, RoutedEventArgs e)
        {
            EditUser EU = new EditUser();
            EU.Show();
        }
        private void UserMenuClose()
        {
            AnimateMenu(UserMenu, 0);
        }
        #endregion

        // ============================ PRODUCT MENU ============================
        #region PRODUCT MENU
        private void ProductMenu_Click(object sender, RoutedEventArgs e)
        {
            if (ProductMenu.ActualHeight > 0)
            {
                AnimateMenu(ProductMenu, 0);
            }
            else
            {
                UserMenuClose();
                WarehouseMenuClose();
                AnimateMenu(ProductMenu, 65);
            }
        }
        // ============================ ADD PRODUCT ============================
        private void CreateProduct_Click(object sender, RoutedEventArgs e)
        {
            AddnEditProduct AEP = new AddnEditProduct("Add");
            AEP.ShowDialog();
        }
        // ============================ EDIT PRODUCT ============================
        private void EditProductButton_Click(object sender, RoutedEventArgs e)
        {
            AddnEditProduct AEP = new AddnEditProduct("Edit");
            AEP.ShowDialog();
        }

        private void ProductMenuClose()
        {
            AnimateMenu(ProductMenu, 0);
        }
        // ========================== PRODUCT MENU END ==========================
        #endregion
        // =========================== WAREHOUSE MENU ===========================

        #region WAREHOUSE MENU
        private void WarehouseMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            if (WarehouseMenu.ActualHeight> 0)
            {
                AnimateMenu(WarehouseMenu, 0);
            }
            else
            {
                UserMenuClose();
                ProductMenuClose();
                AnimateMenu(WarehouseMenu, 65);
            }
        }
        // =========================== ADD WAREHOUSE ===========================
        private void AddWarehouse_Click(object sender, RoutedEventArgs e)
        {
            AddWarehouse addWarehouse = new AddWarehouse();
            addWarehouse.Show();
        }
        // =========================== EDIT WAREHOUSE ===========================
        private void EditWarehouse_Click(object sender, RoutedEventArgs e)
        {
            EditWarehouse EW = new EditWarehouse();
            EW.Show();
        }
        private void WarehouseMenuClose()
        {
            AnimateMenu(WarehouseMenu, 0);
        }
        // ========================= WAREHOUSE MENU END =========================
        #endregion
        // =========================== SET MONTH PLAN ===========================
        private void SetMonthPlan_Click(object sender, RoutedEventArgs e)
        {
            SetMonthPlan SMP = new SetMonthPlan();
            SMP.Show();
        }
        // ============================ REMOVE TABLE ============================
        private void TableRemoveBtn_Click(object sender, RoutedEventArgs e)
        {
            TableRemove TBR = new TableRemove();
            TBR.Show();
        }
        private void EditMenuClose()
        {
            AnimateMenu(EditMenu, 0);
            SetMenuIcon(EditBtnIMG, "/Icons/ArrowDown.png");
        }
        // =========================== EDIT MENU END ===========================
        #endregion
        // ============================ SEARCH FIELD ============================
        #region SEARCH FIELD
        private void SearchField_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (AdminMode)
            {
                string searchText = SearchTextBox.Text.Trim().ToLower();

                // Получаем выбранную вкладку
                if (AdminTabControl.SelectedItem is TabItem selectedTab)
                {
                    // Получаем DataGrid из содержимого вкладки
                    if (selectedTab.Content is DataGrid dataGrid)
                    {
                        // Источник должен быть DataView
                        if (dataGrid.ItemsSource is DataView dataView)
                        {
                            List<string> filterConditions = new List<string>();

                            foreach (DataColumn column in dataView.Table.Columns)
                            {
                                // Строим условие для фильтрации всех столбцов
                                filterConditions.Add($"CONVERT([{column.ColumnName}], 'System.String') LIKE '%{searchText}%'");
                            }

                            // Устанавливаем фильтр
                            dataView.RowFilter = string.Join(" OR ", filterConditions);
                        }
                    }
                }
            }
            else
            {
                string searchText = SearchTextBox.Text.Trim().ToLower();

                if (MainDataGrid.ItemsSource is DataView dataView)
                {
                    List<string> filterConditions = new List<string>();

                    foreach (DataColumn column in dataView.Table.Columns)
                    {
                        // Фильтрация абсолютно всех типов данных через строковое преобразование
                        filterConditions.Add($"CONVERT([{column.ColumnName}], 'System.String') LIKE '%{searchText}%'");
                    }

                    dataView.RowFilter = string.Join(" OR ", filterConditions);
                }
            }
        }
        #endregion
        // ============================ PROFILE MENU ============================
        #region PROFILE MENU
        private void ProfileMenu_Click(object sender, RoutedEventArgs e)
        {
            if (ProfileMenu.ActualHeight > 0)
            {
                AnimateMenu(ProfileMenu, 0);
            }
            else
            {
                AnimateMenu(ProfileMenu, 65);
            }
        }
        // ============================ EDIT PROFILE ============================
        private void ProfileBtn_Click(object sender, RoutedEventArgs e)
        {
            ProfilePage ProfilePageWindow = new ProfilePage();
            ProfilePageWindow.ShowDialog();
        }
        // ============================== LOG OUT ==============================
        private void LogOut_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow LoginWindow = new LoginWindow();
            LoginWindow.Show();
            this.Close();
            DataSource.Clear();

        }
        private void ProfileMenuClose()
        {
            AnimateMenu(ProfileMenu, 0);
        }
        #endregion

        // =========================== FILTER BUTTON ===========================
        private void ApplyFilterButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyColumnVisibility();
        }
        // ================================ LOAD ================================
        private void MainLoad()
        {
            Tablename = DataSource.DBname;
            RoleControl();
            MinimizeElements();
            LoadDataToLabel(Tablename);
            ReLoadData(Tablename);
            GetMonthIntoLabel();
        }
        // ============================ DATA CONTROL ============================
        #region DATA CONTROL
        private void RoleControl()
        {
            NameLabel.Content = Name;
            RoleLabel.Content = Role;
            string labelText = RoleLabel.Content?.ToString(); // Получаем текст из Label, безопасно обрабатывая null

            if (labelText != "Developer" && labelText != "Admin")
            {
                this.Title = "Главное окно";
                ManagerControls();
            }
            else
            {
                this.Title = "Главное окно (ADMIN)";
                if (labelText == "Developer") { MessageBox.Show("Welcome back Developer! All systems online", "Welcome back", MessageBoxButton.OK, MessageBoxImage.Information); }
                AdminMode = true;
                AdminControls();
            }
        }

        private void LoadDataToLabel(string DBname = null)
        {
            string connectionString = CH.GetConnectionString();
            string query;
            if (AdminMode)
            {
                query = $"SELECT monthPlan FROM users WHERE databasename = '{DBname}'";
            }
            else { query = $"SELECT monthPlan FROM users WHERE databasename = '{DataSource.DBname}'"; }
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    MySqlCommand command = new MySqlCommand(query, connection);
                    object result = command.ExecuteScalar();
                    var culture = new CultureInfo("de-DE");

                    // Модифицированная проверка на null и DBNull
                    if (result == null || result == DBNull.Value)
                    {
                        MonthPlanLabel.Content = "0,00";
                    }
                    else
                    {
                        int convResult = Convert.ToInt32(result);
                        MonthPlanLabel.Content = convResult.ToString("N2", culture);
                    }
                }
                SoldControl();
            }
            catch (MySqlException ex)
            {
                AddErrorMessage($"Ошибка загрузки данных (LOADDATATOLABEL): {ex.Message}"); ;
                MonthPlanLabel.Content = "0,00"; // Устанавливаем 0 при ошибке
            }
        }

        private void SoldControl(string role = null)
        {
            if (AdminMode)
            {
                string connectionString = CH.GetConnectionString();
                string query = $"SELECT monthplan FROM users WHERE databasename = '{TabTranslations.GetTechnicalName(GetTabName())}'";
                string query2 = $"SELECT SUM(ShipmentValue) - SUM(ShipmentPrice) from `{TabTranslations.GetTechnicalName(GetTabName())}`";
                string query3 = $"SELECT SUM(`ShipmentValue(Minimum_price)`) from `{TabTranslations.GetTechnicalName(GetTabName())}`";
                var culture = new CultureInfo("de-DE");

                try
                {
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        MySqlCommand command = new MySqlCommand(query, connection);
                        MySqlCommand command2 = new MySqlCommand(query2, connection);
                        MySqlCommand command3 = new MySqlCommand(query3, connection);

                        object MonthPlan = command.ExecuteScalar();
                        object result = command2.ExecuteScalar();
                        object resultMinimum = command3.ExecuteScalar();

                        // Обработка пустых значений
                        double ConvMonthPlan = (MonthPlan == null || MonthPlan == DBNull.Value) ? 0 : Convert.ToInt32(MonthPlan);
                        double ConvResult = (result == null || result == DBNull.Value) ? 0 : Convert.ToInt32(result);
                        double ConvResultMinimum = (resultMinimum == null || resultMinimum == DBNull.Value) ? 0 : Convert.ToInt32(resultMinimum);

                        // Логика изменения цвета
                        if (ConvResult < ConvMonthPlan / 2)
                        {
                            SoldedLabel.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFC00F0C"));
                        }
                        else if (ConvResult > ConvMonthPlan / 2 && ConvResult < ConvMonthPlan)
                        {
                            SoldedLabel.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFD324"));
                        }
                        else
                        {
                            SoldedLabel.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#14AE5C"));
                        }

                        SoldedLabel.Content = ConvResult.ToString("N2", culture);
                        SoldedMinimumLabel.Content = ConvResultMinimum.ToString("N2", culture);

                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message != "Unknown column 'ShipmentValue' in 'field list'")
                    {
                        AddErrorMessage($"Ошибка загрузки данных (КОНТРОЛЬ ПРОДАЖ) ADMIN: {ex.Message}");
                    }
                    SoldedLabel.Content = "0,00"; // Устанавливаем 0 при ошибке
                    SoldedMinimumLabel.Content = "0,00";
                }
            }
            else
            {
                string connectionString = CH.GetConnectionString();
                string query = $"SELECT monthplan FROM users WHERE login = '{Login}'";
                string query2 = $"SELECT SUM(ShipmentValue) - SUM(ShipmentPrice) from `{Tablename}`";
                string query3 = $"SELECT SUM(`ShipmentValue(Minimum_price)`) from `{Tablename}`";
                var culture = new CultureInfo("de-DE");

                try
                {
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        MySqlCommand command = new MySqlCommand(query, connection);
                        MySqlCommand command2 = new MySqlCommand(query2, connection);
                        MySqlCommand command3 = new MySqlCommand(query3, connection);

                        object MonthPlan = command.ExecuteScalar();
                        object result = command2.ExecuteScalar();
                        object resultMinimum = command3.ExecuteScalar();

                        // Обработка пустых значений
                        int ConvMonthPlan = (MonthPlan == null || MonthPlan == DBNull.Value) ? 0 : Convert.ToInt32(MonthPlan);
                        int ConvResult = (result == null || result == DBNull.Value) ? 0 : Convert.ToInt32(result);
                        int ConvResultMinimum = (resultMinimum == null || resultMinimum == DBNull.Value) ? 0 : Convert.ToInt32(resultMinimum);

                        // Логика изменения цвета
                        if (ConvResult < ConvMonthPlan / 2)
                        {
                            SoldedLabel.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFC00F0C"));
                        }
                        else if (ConvResult > ConvMonthPlan / 2 && ConvResult < ConvMonthPlan)
                        {
                            SoldedLabel.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFD324"));
                        }
                        else
                        {
                            SoldedLabel.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#14AE5C"));
                        }

                        SoldedLabel.Content = ConvResult.ToString("N2", culture);
                        SoldedMinimumLabel.Content = ConvResultMinimum.ToString("N2", culture);
                    }
                }
                catch (Exception ex)
                {
                    AddErrorMessage($"Ошибка загрузки данных (КОНТРОЛЬ ПРОДАЖ): {ex.Message}");
                    SoldedLabel.Content = "0,00"; // Устанавливаем 0 при ошибке
                    SoldedMinimumLabel.Content = "0,00";
                }
            }
        }
        #endregion
        // ============================ ADMIN LOGIN ============================

        private void AdminControls()
        {
            AdminEditButton.Visibility = Visibility.Visible;
            AdminTabControl.Visibility = Visibility.Visible;
            MainDataGrid.Visibility = Visibility.Collapsed;
            FontResizeGrid.Visibility = Visibility.Visible;
            GetMySQLTables(CH.GetConnectionString());
            LoadTablesIntoTabControl();
            AdminTabControl.SelectedIndex = 0;
            LoadDataAndCreateCheckBoxes(true);
            GetMonthIntoLabel();
        }

        private void AdminDelete()
        {
            // Получаем текущую активную вкладку

            if (AdminTabControl.SelectedItem is TabItem currentTab)
            {
                // Получаем DataGrid из содержимого вкладки
                if (currentTab.Content is DataGrid dataGrid)
                {
                    DataRowView selectedRow = (DataRowView)dataGrid.SelectedItem;
                    int id = Convert.ToInt32(selectedRow["id"]);
                    // Проверяем, есть ли выбранная строка
                    if (dataGrid.SelectedItem == null)
                    {
                        MessageBox.Show("Выберите строку для удаления!");
                        return;
                    }
                    //MessageBoxResult DialogResult = MessageBox.Show("Удалить выбранную запись?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (MessageBox.Show("Удалить выбранную запись?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        try
                        {
                            using (MySqlConnection connection = new MySqlConnection(CH.GetConnectionString()))
                            {
                                connection.Open();
                                string deleteQuery = $"DELETE FROM {TabTranslations.GetTechnicalName(GetTabName())} WHERE (`id` = @id);";
                                MySqlCommand command = new MySqlCommand(deleteQuery, connection);
                                command.Parameters.AddWithValue("@id", id);

                                int rowsAffected = command.ExecuteNonQuery();

                                if (rowsAffected > 0)
                                {
                                    MessageBox.Show("Запись успешно удалена");
                                    UpdateAll();
                                    ReLoadData();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }

                    }

                }
                ReLoadData(Tablename, true);
                ApplyColumnVisibility();
            }
        }

        // =========================== MANAGER LOGIN ===========================
        private void ManagerControls()
        {
            AdminEditButton.Visibility = Visibility.Collapsed;
            AdminTabControl.Visibility = Visibility.Collapsed;
            AdminDatabaseGrid.Visibility = Visibility.Collapsed;
            MainDataGrid.Visibility = Visibility.Visible;
            FontResizeGrid.Visibility = Visibility.Collapsed;
            LoadDataAndCreateCheckBoxes();
            LoadDataToLabel();
        }
        private void DeleteData()
        {
            if (MainDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите строку для удаления");
                return;
            }
            DataRowView selectedRow = (DataRowView)MainDataGrid.SelectedItem;
            int id = Convert.ToInt32(selectedRow["id"]);
            if (MessageBox.Show("Вы уверены, что хотите удалить эту запись?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    using (MySqlConnection connection = new MySqlConnection(CH.GetConnectionString()))
                    {
                        connection.Open();
                        string deleteQuery = $"DELETE FROM {Tablename} WHERE (`id` = @id);";
                        MySqlCommand command = new MySqlCommand(deleteQuery, connection);
                        command.Parameters.AddWithValue("@id", id);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Запись успешно удалена");
                            UpdateAll();
                            ReLoadData();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            ReLoadData(Tablename);
            ApplyColumnVisibility();
        }

        // ========================== SYSTEM FUNCTIONS ==========================

        // ========================= ADMIN TABLES LOAD =========================
        private DataGrid CreateDataGridForTable(string connectionString, string tableName)
        {
            var dataGrid = new DataGrid
            {
                AutoGenerateColumns = false,
                SelectionMode = DataGridSelectionMode.Single,
                FontSize = FSize,
                FontFamily = new FontFamily("Segoe UI Symbol"),
                SelectionUnit = DataGridSelectionUnit.FullRow,
                CanUserSortColumns = true,
                IsReadOnly = true, // Основное свойство для запрета редактирования
                HorizontalScrollBarVisibility = ScrollBarVisibility.Visible,
                VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
                CanUserAddRows = false, // Запретить добавление новых строк
                CanUserDeleteRows = false, // Запретить удаление строк
                CanUserResizeRows = false,
                CanUserResizeColumns = true
            };

            // Добавление столбцов
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new Binding("Id") });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "Имя", Binding = new Binding("Name") });

            var tableData = new DataTable();
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                var specialTables = new List<string> { "product price", "roles", "users", "warehouses" };

                if (!specialTables.Contains(tableName))
                {
                    try
                    {
                        string query = CH.ManagerData(tableName);
                        var adapter = new MySqlDataAdapter(query, connection);
                        adapter.Fill(tableData);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка загрузки таблиц " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    var adapter = new MySqlDataAdapter(CH.SystemQuery(TabTranslations.GetTechnicalName(tableName)), connection);
                    adapter.Fill(tableData);
                }

            }
            dataGrid.Columns.Clear();

            foreach (DataColumn column in tableData.Columns)
            {
                // Создаем правильную привязку для DataTable
                var binding = new Binding(column.ColumnName);

                var gridColumn = new DataGridTextColumn
                {
                    Header = column.ColumnName,
                    Binding = binding,
                    CanUserSort = true // Разрешаем сортировку для столбца
                };

                // Форматирование для даты
                if (column.DataType == typeof(DateTime) ||
                    column.ColumnName.Equals("ShipmentDate", StringComparison.OrdinalIgnoreCase))
                {
                    gridColumn.Binding = new Binding(column.ColumnName)
                    {
                        StringFormat = "dd.MM.yyyy"
                    };
                }

                dataGrid.Columns.Add(gridColumn);
            }

            // Устанавливаем источник данных
            dataGrid.ItemsSource = tableData.DefaultView;

            return dataGrid;
        }

        // Вспомогательная функция для проверки принадлежности элемента
        private bool IsDescendantOf(DependencyObject element, DependencyObject parent)
        {
            if (element == null || parent == null)
                return false;

            // Проверяем все родительские элементы
            while (element != null)
            {
                if (element == parent)
                    return true;

                // Для Popup элементов нужна особая проверка
                if (element is Visual || element is Visual3D)
                    element = VisualTreeHelper.GetParent(element);
                else
                    element = LogicalTreeHelper.GetParent(element);
            }

            return false;
        }

        public List<string> GetMySQLTables(string connectionString)
        {
            List<string> tables = new List<string>();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {

                connection.Open();

                // Запрос для получения списка таблиц
                string query = "SHOW TABLES";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tables.Add(reader.GetString(0));
                        }
                    }
                }
            }
            return tables;
        }

        // Модифицированный метод загрузки данных
        private void LoadTablesIntoTabControl()
        {
            string connectionString = CH.GetConnectionString();
            var tables = GetMySQLTables(connectionString);

            AdminTabControl.Items.Clear();

            foreach (string tableName in tables)
            {
                var newTab = new TabItem
                {
                    Header = TabTranslations.GetUserFriendlyName(tableName),
                    Content = CreateDataGridForTable(connectionString, tableName),
                    Tag = tableName // Сохраняем техническое название
                };
                newTab.Background = GetTabColor(tableName);

                AdminTabControl.Items.Add(newTab);
            }
        }
        private Brush GetTabColor(string tableName)
        {
            return tableName.ToLower() switch
            {
                "warehouses" => Brushes.LightGreen,
                "users" => Brushes.LightGreen,
                "product price" => Brushes.LightGreen,
                "roles" => Brushes.LightGreen,
                _ => Brushes.LightBlue
            };
        }
        private void MinimizeElements()
        {
            DatabaseMenu.Height = 0;
            EditMenu.Height = 0;
            UserMenu.Height = 0;
            ProductMenu.Height = 0;
            WarehouseMenu.Height = 0;
            ProfileMenu.Height = 0;
        }

        public void ReLoadData(string name = null, bool AdminMode1 = false)
        {
            if (AdminMode1)
            {
                // Сохраняем индекс текущей вкладки
                int currentIndex = AdminTabControl.SelectedIndex;

                // Полностью очищаем TabControl
                AdminTabControl.ItemsSource = null;

                // Перезагружаем данные
                LoadTablesIntoTabControl();

                // Восстанавливаем выбранную вкладку (если возможно)
                if (currentIndex >= 0 && currentIndex < AdminTabControl.Items.Count)
                {
                    AdminTabControl.SelectedIndex = currentIndex;
                }

                string tableName = TabTranslations.GetTechnicalName(GetTabName());
                if (!string.IsNullOrWhiteSpace(tableName) && tableName != "error")
                {
                    SoldControl();
                }
                else
                {
                    AddErrorMessage("Ошибка: не удалось определить имя таблицы для вкладки.");
                }
            }
            else
            {
                SoldControl();
                MainDataGrid.ItemsSource = null;
                try
                {
                    using (MySqlConnection connection = new MySqlConnection(CH.GetConnectionString()))
                    {
                        connection.Open();

                        string query = CH.ManagerData(Tablename);
                        using (MySqlCommand command = new MySqlCommand(query, connection))
                        {
                            MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);

                            MainDataGrid.ItemsSource = dataTable.DefaultView;
                        }
                    }
                }
                catch (Exception ex)
                {
                    AddErrorMessage($"Ошибка загрузки данных (RELOADDATA): {ex.Message}");
                }
            }
        }
        private void ApplyColumnVisibility()
        {
            if (AdminMode)
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
                DataGrid AMDG = GetSelectedDataGrid(AdminTabControl);
                foreach (DataGridColumn column in AMDG.Columns)
                {
                    if (column.Header != null)
                    {
                        column.Visibility = selectedColumns.Contains(column.Header.ToString()) ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
            }
            else {
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
        }


        // ==================== ERROR CONTROL (RUNNING LINE) ====================
        public enum ErrorLevel { Info, Warning, Error }

        public void AddErrorMessage(string message, ErrorLevel level = ErrorLevel.Error)
        {
            if (ErrorMarquee == null || MarqueeTransform == null)
                return;

            // Префикс уровня
            string prefix = level switch
            {
                ErrorLevel.Info => "[ИНФО]",
                ErrorLevel.Warning => "[ВНИМАНИЕ]",
                ErrorLevel.Error => "[ОШИБКА]",
                _ => ""
            };

            string fullMessage = $"{prefix} {message}";

            // Добавление сообщения
            if (!string.IsNullOrWhiteSpace(ErrorMarquee.Text))
            {
                ErrorMarquee.Text += " • " + fullMessage;
            }
            else
            {
                ErrorMarquee.Text = fullMessage;
            }

            // Изменение цвета в зависимости от уровня
            ErrorMarquee.Foreground = level switch
            {
                ErrorLevel.Info => Brushes.LightGreen,
                ErrorLevel.Warning => Brushes.Gold,
                ErrorLevel.Error => Brushes.OrangeRed,
                _ => Brushes.White
            };

            RestartMarqueeAnimation();
        }

        public void ClearErrorMessages()
        {
            if (ErrorMarquee == null || MarqueeTransform == null)
                return;

            ErrorMarquee.Text = "Готов к работе... ";
            ErrorMarquee.Foreground = Brushes.LightGray;
            RestartMarqueeAnimation();
        }

        private void RestartMarqueeAnimation()
        {
            if (ErrorMarquee == null || MarqueeTransform == null)
                return;

            var animation = new DoubleAnimation
            {
                From = ActualWidth,
                To = -ErrorMarquee.ActualWidth,
                Duration = TimeSpan.FromSeconds(60),
                RepeatBehavior = RepeatBehavior.Forever
            };

            MarqueeTransform.BeginAnimation(TranslateTransform.XProperty, animation);
        }

        // =============================== FILTER ===============================

        private double horizontalMargin = 10;       // Горизонтальный отступ
        private double verticalMargin = 5;         // Вертикальный отступ
        private double maxHeight = 1000;
        private void FilterScrollViewver_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double availableWidth = CheckBoxPanel.ActualWidth;
            int maxRows = Math.Max(1, (int)(maxHeight / ((verticalMargin * 2))));  // Максимальное количество строк
            int columnCountByWidth = Math.Max(1, (int)(availableWidth / (horizontalMargin * 2))); // Столбцов по ширине
            int columnCountByHeight = (int)Math.Ceiling((double)CheckBoxPanel.Children.Count / maxRows);  // Столбцов по высоте

            int columnCount = Math.Min(columnCountByWidth, columnCountByHeight); // Выбираем меньшее из двух значений

            CheckBoxPanel.Columns = columnCount;
        }

        private void FilterAllElements_Checked(object sender, RoutedEventArgs e)
        {
            LoadDataAndCreateCheckBoxes();
            ReLoadData(Tablename);
        }

        private void LoadDataAndCreateCheckBoxes(bool AdminMode = false)
        {
            try
            {
                CheckBoxPanel.Children.Clear(); // Очистка CheckBox'ов

                using (var connection = new MySqlConnection(CH.GetConnectionString()))
                {
                    connection.Open();

                    string query;
                    DataTable allData = new DataTable();

                    if (AdminMode)
                    {
                        string tableName = TabTranslations.GetTechnicalName(GetTabName());
                        if (string.IsNullOrWhiteSpace(tableName) || tableName.ToLower() == "error")
                        {
                            //AddErrorMessage("Ошибка: имя таблицы не определено или содержит ошибку.");
                            return;
                        }

                         var specialTables = new List<string> { "product price", "roles", "users", "warehouses" };

                        if (!specialTables.Contains(tableName))
                        {
                            query = CH.ManagerData(tableName);
                        }
                        else
                        {
                            query = $"{CH.SystemQuery(TabTranslations.GetTechnicalName(tableName))}";
                        }
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(Tablename))
                        {
                            AddErrorMessage("Ошибка: имя базы данных не указано.");
                            return;
                        }

                        query = CH.ManagerData(Tablename);
                    }

                    var adapter = new MySqlDataAdapter(query, connection);
                    adapter.Fill(allData);

                    // Создание чекбоксов по столбцам
                    foreach (DataColumn column in allData.Columns)
                    {
                        CheckBox checkBox = new CheckBox
                        {
                            Content = column.ColumnName,
                            IsChecked = true,
                            Margin = new Thickness(10, 5, 5, 5),
                            FontSize = 15,
                            Style = (Style)FindResource("RoundedCheckBoxStyle"),
                            Tag = column.ColumnName
                        };
                        CheckBoxPanel.Children.Add(checkBox);
                    }

                    if (AdminMode)
                    {
                        // Получение текущего DataGrid внутри активной вкладки
                        if (AdminTabControl.SelectedItem is TabItem selectedTab && selectedTab.Content is DataGrid dataGrid)
                        {
                            dataGrid.ItemsSource = allData.DefaultView;
                            ApplyColumnVisibility();
                        }
                        else
                        {
                            AddErrorMessage("Не удалось найти DataGrid во вкладке администратора.");
                        }
                    }
                    else
                    {
                        MainDataGrid.ItemsSource = allData.DefaultView;
                        MainDataGrid.AutoGenerateColumns = true;
                        ApplyColumnVisibility();
                    }
                }
            }
            catch (MySqlException ex)
            {
                AddErrorMessage($"Ошибка загрузки данных (MySQL): {ex.Message}");
            }
            catch (Exception ex)
            {
                AddErrorMessage($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private T FindChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T found)
                    return found;

                var result = FindChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }
       

        private string GetTabName()
        {
            if (AdminTabControl.SelectedItem is TabItem selectedTab)
            {
                var _currentTableName = selectedTab.Header.ToString();
                return _currentTableName;
            }
            else return "error";
        }
        private void AdminTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Проверяем, что изменение действительно связано с выбором вкладки
            if (e.Source is TabControl tabControl)
            {
                // Получаем выбранную вкладку
                if (tabControl.SelectedItem is TabItem selectedTab)
                {
                    LoadDataAndCreateCheckBoxes(true);
                    DataSource.DBname = TabTranslations.GetTechnicalName(GetTabName());
                    LoadDataToLabel(TabTranslations.GetTechnicalName(GetTabName()));
                }
            }
        }
        public DataGrid GetSelectedDataGrid(TabControl tabControl)
        {
            if (tabControl.SelectedItem is TabItem selectedTab &&
                selectedTab.Content is DataGrid dataGrid)
            {
                return dataGrid;
            }
            return null;
        }
        
        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child is T correctlyTyped)
                    return correctlyTyped;

                T descendent = FindVisualChild<T>(child);
                if (descendent != null)
                    return descendent;
            }
            return null;
        }

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Получаем элемент, по которому был сделан клик
            var clickedElement = e.OriginalSource as DependencyObject;

            // Проверяем, был ли клик вне всех меню
            if (!IsDescendantOf(clickedElement, DatabaseMenu) &&
                !IsDescendantOf(clickedElement, EditMenu) &&
                !IsDescendantOf(clickedElement, ProfileMenu))
            {
                DatabaseMenuClose();
                EditMenuClose();
                UserMenuClose();
                ProductMenuClose();
                WarehouseMenuClose();
                ProfileMenuClose();
            }
        }
        private string GetMonth()
        {
            string monthName = DateTime.Now.ToString("MMMM", new System.Globalization.CultureInfo("ru-RU"));

            return monthName;
        }
        private void GetMonthIntoLabel()
        {
            string monthName = DateTime.Now.ToString("MMMM", new System.Globalization.CultureInfo("ru-RU"));

            MonthPLabel.Content = $"План ({monthName})";
            SoldedMLabel.Content = $"Продано ({monthName})";
        }
        // ======================= MANAGER DATAGRID STYLE =======================
        #region MANAGER DATAGRID STYLE
        private void DataGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_isCtrlPressed && _isMouseOverDataGrid)
            {
                double newSize = MainDataGrid.FontSize + (e.Delta > 0 ? 1 : -1);

                if (newSize < 8 || newSize > 24)
                {
                    MainDataGrid.FontSize = newSize;
                    // Анимация "отскока" при достижении границ
                    DoubleAnimation anim = new DoubleAnimation
                    {
                        To = newSize < 8 ? 9 : 23,
                        Duration = TimeSpan.FromMilliseconds(200),
                        AutoReverse = true,
                        EasingFunction = new ElasticEase { Oscillations = 2 }
                    };
                    MainDataGrid.BeginAnimation(Control.FontSizeProperty, anim);
                    ReLoadData(Tablename);

                }
                else
                {
                    MainDataGrid.FontSize = newSize;
                    // Обычная плавная анимация
                    DoubleAnimation anim = new DoubleAnimation
                    {
                        To = newSize,
                        Duration = TimeSpan.FromMilliseconds(300),
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                    };
                    MainDataGrid.BeginAnimation(Control.FontSizeProperty, anim);
                    ReLoadData(Tablename);

                }

                e.Handled = true;
            }
        }

        private void DataGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            _isMouseOverDataGrid = true;
        }

        private void DataGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            _isMouseOverDataGrid = false;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                _isCtrlPressed = true;
            }
            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                _isCtrlPressed = false;
            }
            base.OnKeyUp(e);
        }

        private void MainDataGrid_Loaded(object sender, RoutedEventArgs e)
        {

            var dataGrid = (DataGrid)sender;

            // Создаем стиль программно
            var cellStyle = new Style(typeof(DataGridCell));
            cellStyle.Setters.Add(new Setter(Control.FontSizeProperty, 12.0));

            // Триггер при наведении
            var trigger = new Trigger()
            {
                Property = UIElement.IsMouseOverProperty,
                Value = true
            };
            trigger.Setters.Add(new Setter(Control.FontSizeProperty, 14.0));

            cellStyle.Triggers.Add(trigger);
            dataGrid.CellStyle = cellStyle;
        }
        #endregion
        #region Menu animations
        private void AnimateMenu(FrameworkElement menu, double toHeight)
        {
            DoubleAnimation animation = new DoubleAnimation
            {
                From = menu.ActualHeight,
                To = toHeight,
                Duration = TimeSpan.FromSeconds(0.2)
            };
            menu.BeginAnimation(FrameworkElement.HeightProperty, animation);
        }
        // Add the missing SetMenuIcon method to resolve the CS0103 error.
        private void SetMenuIcon(Image imageControl, string iconPath)
        {
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(iconPath, UriKind.RelativeOrAbsolute);
            bitmapImage.EndInit();
            imageControl.Source = bitmapImage;
        }

        #endregion

        private void FontPlus_Click(object sender, RoutedEventArgs e)
        {
            if (FSize <= 72)
            {
                FSize += 1;
                ReLoadData(Tablename, AdminMode);
            }
        }

        private void FontMinus_Click(object sender, RoutedEventArgs e)
        {
            if (FSize >= 8)
            {
                FSize -= 1;
                ReLoadData(Tablename, AdminMode);
            }
        }
    }
}