using System;
using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;
using PJT1.Models;

namespace PJT1.Services
{
    /// <summary>
    /// СЕРВИС ДЛЯ РАБОТЫ С EXCEL
    /// 
    /// Статический класс для чтения данных из Excel файлов
    /// </summary>
    public static class ExcelService
    {
        /// <summary>
        /// ЧТЕНИЕ ПЕРВЫХ ДВУХ ПОЛЕЙ ИЗ EXCEL
        /// 
        /// Читает файл Excel и возвращает список объектов DataBD
        /// </summary>
        /// <param name="filePath">Путь к Excel файлу</param>
        /// <returns>Список объектов DataBD</returns>
        public static List<DataBD> ReadFirstTwoFieldsFromExcel(string filePath)
        {
            var result = new List<DataBD>();

            // Проверяем существование файла
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Файл не найден: {filePath}");

            // Проверяем расширение
            if (!filePath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Поддерживаются только .xlsx файлы");

            // Настраиваем лицензию EPPlus
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            try
            {
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    // Получаем первый лист
                    var worksheet = package.Workbook.Worksheets[0];
                    
                    if (worksheet == null || worksheet.Dimension == null)
                        throw new Exception("Excel файл пуст или не содержит данных");

                    int rowCount = worksheet.Dimension.Rows;

                    // Читаем данные, начиная со 2-й строки (пропускаем заголовки)
                    for (int row = 2; row <= rowCount; row++)
                    {
                        var tagname = worksheet.Cells[row, 1].Text?.Trim() ?? string.Empty;
                        var loop = worksheet.Cells[row, 2].Text?.Trim() ?? string.Empty;

                        // Пропускаем пустые строки
                        if (string.IsNullOrEmpty(tagname) && string.IsNullOrEmpty(loop))
                            continue;

                        var data = new DataBD(tagname, loop);
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