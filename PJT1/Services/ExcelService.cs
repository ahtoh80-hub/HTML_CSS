// ================================================================
// ПРОСТРАНСТВА ИМЕН
// ================================================================
using System;
using System.Collections.Generic;
using System.IO;           // Для работы с файлами
using OfficeOpenXml;      // Библиотека EPPlus для Excel
using PJT1.Models;

namespace PJT1.Services
{
    /// <summary>
    /// КЛАСС ExcelService - СЕРВИС ДЛЯ РАБОТЫ С EXCEL
    /// 
    /// Статический класс (static class)
    /// 
    /// Что значит "статический"?
    /// 1. Нельзя создать объект этого класса
    /// 2. Все методы вызываются через имя класса
    /// 3. Хранит только статические методы
    /// 
    /// Зачем делать класс статическим?
    /// Когда у класса нет состояния (данных),
    /// а есть только функциональность (методы)
    /// </summary>
    public static class ExcelService
    {
        /// <summary>
        /// ЧТЕНИЕ ПЕРВЫХ ДВУХ ПОЛЕЙ ИЗ EXCEL
        /// 
        /// Это основной метод для импорта данных из Excel
        /// 
        /// Подробный алгоритм:
        /// 1. Проверяем существование файла
        /// 2. Проверяем расширение файла (.xlsx)
        /// 3. Настраиваем лицензию EPPlus
        /// 4. Открываем Excel файл
        /// 5. Получаем первый лист
        /// 6. Читаем все строки начиная со 2-й (пропускаем заголовки)
        /// 7. Для каждой строки читаем столбцы 1 и 2
        /// 8. Создаем объект DataBD
        /// 9. Добавляем в список результатов
        /// 10. Возвращаем список
        /// </summary>
        /// <param name="filePath">Полный путь к Excel файлу</param>
        /// <returns>Список объектов DataBD</returns>
        /// <exception cref="FileNotFoundException">Файл не найден</exception>
        /// <exception cref="ArgumentException">Неподдерживаемое расширение</exception>
        /// <exception cref="Exception">Ошибка чтения Excel</exception>
        public static List<DataBD> ReadFirstTwoFieldsFromExcel(string filePath)
        {
            // Создаем список для результатов
            var result = new List<DataBD>();

            // ============================================================
            // ШАГ 1: ПРОВЕРКА ФАЙЛА
            // ============================================================

            // Проверяем существование файла
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Файл не найден: {filePath}");
            }

            // Проверяем расширение файла
            if (!filePath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Поддерживаются только .xlsx файлы");
            }

            // ============================================================
            // ШАГ 2: НАСТРОЙКА ЛИЦЕНЗИИ EPPlus
            // ============================================================

            // EPPlus требует явной настройки лицензии
            // LicenseContext.NonCommercial - для некоммерческого использования
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // ============================================================
            // ШАГ 3: ЧТЕНИЕ ДАННЫХ
            // ============================================================

            try
            {
                // using - автоматически освобождает ресурсы после использования
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    // Получаем первый лист (индекс 0)
                    var worksheet = package.Workbook.Worksheets[0];
                    
                    // Проверяем, что лист существует и содержит данные
                    if (worksheet == null || worksheet.Dimension == null)
                    {
                        throw new Exception("Excel файл пуст или не содержит данных");
                    }

                    // Получаем количество строк с данными
                    int rowCount = worksheet.Dimension.Rows;

                    // Читаем данные, начиная со 2-й строки (пропускаем заголовки)
                    for (int row = 2; row <= rowCount; row++)
                    {
                        // Читаем первое поле (столбец A) - Tagname
                        var tagname = worksheet.Cells[row, 1].Text?.Trim() ?? string.Empty;
                        
                        // Читаем второе поле (столбец B) - Loop
                        var loop = worksheet.Cells[row, 2].Text?.Trim() ?? string.Empty;

                        // Пропускаем пустые строки
                        if (string.IsNullOrEmpty(tagname) && string.IsNullOrEmpty(loop))
                            continue;

                        // Создаем объект DataBD с двумя полями
                        var data = new DataBD(tagname, loop);
                        
                        // Добавляем в результат
                        result.Add(data);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при чтении Excel: {ex.Message}", ex);
            }

            return result;
        }
    }
}