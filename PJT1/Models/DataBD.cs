// ================================================================
// ПРОСТРАНСТВО ИМЕН
// ================================================================
using System;  // Для работы с DateTime

namespace PJT1.Models
{
    /// <summary>
    /// КЛАСС DataBD
    /// 
    /// Модель данных для хранения информации из Excel
    /// Каждый объект этого класса представляет одну запись
    /// </summary>
    public class DataBD
    {
        // ============================================================
        // СВОЙСТВА (данные объекта)
        // ============================================================

        /// <summary>
        /// Уникальный идентификатор записи
        /// Автоматически присваивается при добавлении в репозиторий
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Имя тега (первое поле из Excel)
        /// </summary>
        public string Tagname { get; set; }

        /// <summary>
        /// Цикл/контур (второе поле из Excel)
        /// </summary>
        public string Loop { get; set; }

        /// <summary>
        /// Комментарий (третье поле из Excel)
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Дата создания записи
        /// </summary>
        public DateTime CreatedDate { get; set; }

        // ============================================================
        // КОНСТРУКТОРЫ
        // ============================================================

        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public DataBD()
        {
            Tagname = string.Empty;
            Loop = string.Empty;
            Comment = string.Empty;
            CreatedDate = DateTime.Now;
        }

        /// <summary>
        /// Конструктор для импорта из Excel (первые два поля)
        /// </summary>
        public DataBD(string tagname, string loop) : this()
        {
            Tagname = tagname ?? string.Empty;
            Loop = loop ?? string.Empty;
        }

        /// <summary>
        /// Конструктор для ручного добавления (все поля)
        /// </summary>
        public DataBD(string tagname, string loop, string comment) : this()
        {
            Tagname = tagname ?? string.Empty;
            Loop = loop ?? string.Empty;
            Comment = comment ?? string.Empty;
        }

        /// <summary>
        /// Переопределение ToString() для отображения в списках
        /// </summary>
        public override string ToString()
        {
            return $"{Tagname} - {Loop} - {Comment}";
        }
    }
}