// ================================================================
// ПРОСТРАНСТВА ИМЕН
// ================================================================
using System;
using System.Collections.Generic;
using System.Linq;      // Для LINQ запросов
using PJT1.Models;
using PJT1.Services;

namespace PJT1.Repositories
{
    /// <summary>
    /// КЛАСС DataBDRepository - РЕАЛИЗАЦИЯ РЕПОЗИТОРИЯ
    /// 
    /// Это РЕАЛИЗАЦИЯ интерфейса IDataBDRepository
    /// 
    /// Синтаксис: class Имя : Интерфейс
    /// Двоеточие означает "наследует/реализует"
    /// 
    /// В данный момент данные хранятся в памяти (List)
    /// В реальном проекте здесь могла бы быть работа с:
    /// - SQL базой данных (MSSQL, PostgreSQL)
    /// - JSON или XML файлом
    /// - Облачным хранилищем
    /// 
    /// Прелесть интерфейса: код, использующий репозиторий,
    /// НЕ ЗНАЕТ, где хранятся данные. Для него это просто
    /// "место, где можно получить данные"
    /// </summary>
    public class DataBDRepository : IDataBDRepository
    {
        // ============================================================
        // ПОЛЯ КЛАССА
        // ============================================================

        /// <summary>
        /// ХРАНИЛИЩЕ ДАННЫХ
        /// 
        /// List<DataBD> - список объектов DataBD
        /// readonly - поле только для чтения
        /// private - доступно только внутри этого класса
        /// </summary>
        private readonly List<DataBD> _dataList;

        /// <summary>
        /// СЧЕТЧИК ID
        /// 
        /// Хранит следующий доступный идентификатор
        /// При добавлении новой записи используем это значение
        /// и увеличиваем на 1
        /// </summary>
        private int _nextId;

        // ============================================================
        // КОНСТРУКТОР
        // ============================================================

        /// <summary>
        /// КОНСТРУКТОР РЕПОЗИТОРИЯ
        /// 
        /// Создает новый экземпляр репозитория
        /// Инициализирует пустой список и счетчик
        /// </summary>
        public DataBDRepository()
        {
            // Создаем новый пустой список
            _dataList = new List<DataBD>();
            
            // Начинаем нумерацию с 1
            _nextId = 1;
        }

        // ============================================================
        // РЕАЛИЗАЦИЯ МЕТОДОВ ИНТЕРФЕЙСА
        // ============================================================

        /// <summary>
        /// ПОЛУЧИТЬ ВСЕ ЗАПИСИ
        /// 
        /// Возвращает копию списка, а не сам список
        /// Защита от изменения данных извне
        /// 
        /// .ToList() - создает новый список с теми же элементами
        /// </summary>
        public IEnumerable<DataBD> GetAll()
        {
            // ToList() - метод расширения LINQ
            return _dataList.ToList();
        }

        /// <summary>
        /// ПОЛУЧИТЬ ЗАПИСЬ ПО ID
        /// 
        /// .FirstOrDefault() - находит первый подходящий элемент
        /// или возвращает null, если ничего не найдено
        /// 
        /// d => d.Id == id - это ЛЯМБДА-ВЫРАЖЕНИЕ
        /// Читается как: "для каждого d проверяем, что d.Id равен id"
        /// </summary>
        public DataBD? GetById(int id)
        {
            return _dataList.FirstOrDefault(d => d.Id == id);
        }

        /// <summary>
        /// ДОБАВИТЬ ЗАПИСЬ
        /// 
        /// Алгоритм:
        /// 1. Проверяем, что объект не null
        /// 2. Присваиваем уникальный ID
        /// 3. Добавляем в список
        /// </summary>
        public void Add(DataBD data)
        {
            // Проверяем, что объект существует
            if (data == null)
            {
                // nameof(data) - возвращает имя параметра "data"
                throw new ArgumentNullException(
                    nameof(data), 
                    "Объект не может быть null"
                );
            }

            // Присваиваем уникальный ID и увеличиваем счетчик
            data.Id = _nextId++;
            
            // Добавляем объект в список
            _dataList.Add(data);
        }

        /// <summary>
        /// ДОБАВИТЬ НЕСКОЛЬКО ЗАПИСЕЙ
        /// 
        /// Используется при массовом импорте из Excel
        /// 
        /// foreach - цикл для перебора коллекции
        /// Проходит по каждому элементу и выполняет действия
        /// </summary>
        public void AddRange(IEnumerable<DataBD> dataList)
        {
            if (dataList == null)
            {
                throw new ArgumentNullException(
                    nameof(dataList), 
                    "Коллекция не может быть null"
                );
            }

            // Проходим по каждому объекту в коллекции
            foreach (var data in dataList)
            {
                if (data == null)
                {
                    throw new ArgumentException(
                        "Коллекция не может содержать null-элементы",
                        nameof(dataList)
                    );
                }

                // Присваиваем ID
                data.Id = _nextId++;
                // Добавляем в список
                _dataList.Add(data);
            }
        }

        /// <summary>
        /// УДАЛИТЬ ЗАПИСЬ ПО ID
        /// 
        /// Ищет запись и удаляет ее из списка
        /// Если запись не найдена - ничего не делает
        /// </summary>
        public void Delete(int id)
        {
            var data = GetById(id);
            if (data != null)
            {
                _dataList.Remove(data);
            }
        }

        /// <summary>
        /// ОЧИСТИТЬ ВСЕ ЗАПИСИ
        /// 
        /// Удаляет все элементы из списка
        /// Сбрасывает счетчик ID
        /// </summary>
        public void Clear()
        {
            _dataList.Clear();
            _nextId = 1;
        }

        /// <summary>
        /// ПОЛУЧИТЬ КОЛИЧЕСТВО ЗАПИСЕЙ
        /// 
        /// Count - свойство списка, возвращает количество элементов
        /// </summary>
        public int Count()
        {
            return _dataList.Count;
        }

        /// <summary>
        /// ИМПОРТ ИЗ EXCEL
        /// 
        /// Основной метод для импорта данных из Excel
        /// 
        /// Алгоритм:
        /// 1. Вызываем ExcelService для чтения данных
        /// 2. Получаем список объектов DataBD
        /// 3. Добавляем их в репозиторий
        /// 
        /// Исключения ExcelService (FileNotFoundException, ArgumentException,
        /// InvalidDataException) намеренно не перехватываются: вызывающий код
        /// должен видеть исходный тип ошибки, чтобы отреагировать на нее.
        /// </summary>
        public void ImportFromExcel(string filePath)
        {
            // Читаем данные из Excel через сервис
            var importedData = ExcelService.ReadFirstTwoFieldsFromExcel(filePath);

            // Добавляем данные в репозиторий
            AddRange(importedData);
        }
    }
}