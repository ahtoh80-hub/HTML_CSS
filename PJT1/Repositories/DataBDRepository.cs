using System;
using System.Collections.Generic;
using System.Linq;
using PJT1.Models;
using PJT1.Services;

namespace PJT1.Repositories
{
    /// <summary>
    /// РЕАЛИЗАЦИЯ РЕПОЗИТОРИЯ
    /// 
    /// Хранит данные в памяти (List)
    /// В реальном проекте здесь могла бы быть работа с БД
    /// </summary>
    public class DataBDRepository : IDataBDRepository
    {
        // Хранилище данных
        private readonly List<DataBD> _dataList;
        private int _nextId;

        /// <summary>
        /// Конструктор репозитория
        /// </summary>
        public DataBDRepository()
        {
            _dataList = new List<DataBD>();
            _nextId = 1;
        }

        /// <summary>
        /// Получить все записи (возвращаем копию)
        /// </summary>
        public IEnumerable<DataBD> GetAll()
        {
            return _dataList.ToList();
        }

        /// <summary>
        /// Получить запись по ID
        /// </summary>
        public DataBD GetById(int id)
        {
            return _dataList.FirstOrDefault(d => d.Id == id);
        }

        /// <summary>
        /// Добавить запись
        /// </summary>
        public void Add(DataBD data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            data.Id = _nextId++;
            _dataList.Add(data);
        }

        /// <summary>
        /// Добавить несколько записей
        /// </summary>
        public void AddRange(IEnumerable<DataBD> dataList)
        {
            if (dataList == null)
                throw new ArgumentNullException(nameof(dataList));

            foreach (var data in dataList)
            {
                data.Id = _nextId++;
                _dataList.Add(data);
            }
        }

        /// <summary>
        /// Удалить запись по ID
        /// </summary>
        public void Delete(int id)
        {
            var data = GetById(id);
            if (data != null)
                _dataList.Remove(data);
        }

        /// <summary>
        /// Очистить все записи
        /// </summary>
        public void Clear()
        {
            _dataList.Clear();
            _nextId = 1;
        }

        /// <summary>
        /// Получить количество записей
        /// </summary>
        public int Count()
        {
            return _dataList.Count;
        }

        /// <summary>
        /// Импорт данных из Excel
        /// </summary>
        public void ImportFromExcel(string filePath)
        {
            try
            {
                var importedData = ExcelService.ReadFirstTwoFieldsFromExcel(filePath);
                AddRange(importedData);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при импорте из Excel: {ex.Message}", ex);
            }
        }
    }
}