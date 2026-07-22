using System.Collections.Generic;
using PJT1.Models;

namespace PJT1.Repositories
{
    /// <summary>
    /// ИНТЕРФЕЙС РЕПОЗИТОРИЯ
    /// 
    /// Определяет контракт для работы с данными
    /// Позволяет легко менять способ хранения данных
    /// </summary>
    public interface IDataBDRepository
    {
        // Получить все записи
        IEnumerable<DataBD> GetAll();

        // Получить запись по ID
        DataBD GetById(int id);

        // Добавить запись
        void Add(DataBD data);

        // Добавить несколько записей
        void AddRange(IEnumerable<DataBD> dataList);

        // Удалить запись
        void Delete(int id);

        // Очистить все записи
        void Clear();

        // Получить количество записей
        int Count();

        // Импорт из Excel
        void ImportFromExcel(string filePath);
    }
}