using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace IOPointManager.Models
{
    public enum SystemType { DCS, SCS, GDS }
    public enum LocationType { Field, SIS, MCC, PLC, AUX, GDS }
    public enum ExProtectionType { None, ExD, ExI, ExE, ExN, ExP, ExO, ExQ, ExM }
    public enum IOPointStatus { Active, Inactive, Commissioning, Maintenance, Fault, Decommissioned }

    public class IOPoint : INotifyPropertyChanged, ICloneable
    {
        private string? _tag;
        private string? _service;
        private IOPointStatus _status = IOPointStatus.Active;
        private int _dataQualityScore = 100;
        private decimal? _currentValue;
        private DateTime? _lastUpdate;

        public event PropertyChangedEventHandler? PropertyChanged;

        // Идентификационные данные
        public int? Code { get; set; }
        public int? Area { get; set; }
        public string? Title { get; set; }
        public string? MainLoop { get; set; }
        public string? SubLoop { get; set; }
        public string? ProcessLoop { get; set; }
        
        public string? Tag
        {
            get => _tag;
            set
            {
                if (_tag != value)
                {
                    _tag = value;
                    OnPropertyChanged();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        try { ParsedTag = InstrumentTag.Parse(value); }
                        catch { ParsedTag = null; }
                    }
                    else ParsedTag = null;
                }
            }
        }
        
        public InstrumentTag? ParsedTag { get; private set; }
        public string? ProcessTag { get; set; }
        public SystemType? System { get; set; }
        public string? IoType { get; set; }
        public string? Controller { get; set; }
        public LocationType? Location { get; set; }

        // Сигнал
        public string? Service
        {
            get => _service;
            set { _service = value; OnPropertyChanged(); }
        }
        public string? ServiceEnglish { get; set; }
        public string? InstrumentType { get; set; }
        public string? InstrumentTypeEnglish { get; set; }
        public string? Pid { get; set; }
        public string? SignalType { get; set; }
        public ExProtectionType? ExProtection { get; set; }
        public string? Subsystem { get; set; }
        public string? Auxiliary { get; set; }

        // Диапазоны и уставки
        public decimal? RangeMin { get; set; }
        public decimal? RangeMax { get; set; }
        public string? RangeUnit { get; set; }
        public decimal? RangeMinSecondary { get; set; }
        public decimal? RangeMaxSecondary { get; set; }
        public string? RangeUnitSecondary { get; set; }
        public decimal? AlarmLL2 { get; set; }
        public decimal? AlarmLL { get; set; }
        public decimal? AlarmL { get; set; }
        public decimal? AlarmH { get; set; }
        public decimal? AlarmHH { get; set; }
        public decimal? AlarmHH2 { get; set; }
        public string? AlarmUnit { get; set; }

        // Кабель
        public string? CableId { get; set; }
        public string? CableDescription { get; set; }
        public string? CableType { get; set; }
        public string? CableDesignation { get; set; }
        public string? CableFrom { get; set; }
        public string? CableTo { get; set; }
        public int? CableLength { get; set; }
        public string? CableColor { get; set; }
        public int? CablePair { get; set; }
        public string? CableNote { get; set; }
        public string? VendorDesignation { get; set; }
        public string? Drum { get; set; }
        public string? Voltage { get; set; }

        // Оборудование
        public string? CableTecon { get; set; }
        public string? Mcc { get; set; }
        public string? ControlCabinet { get; set; }
        public string? MarshallingCabinet { get; set; }
        public string? Cpu { get; set; }
        public string? ChassisMain { get; set; }
        public string? ChassisRedundant { get; set; }
        public string? ModuleSlot { get; set; }
        public string? ModuleSlotRedundant { get; set; }
        public int? ModuleChannel { get; set; }
        public string? ModuleType { get; set; }

        // Ревизия
        public int? RevisionNumber { get; set; }
        public string? RevisionDescription { get; set; }
        public string? Package { get; set; }
        public string? VendorField1 { get; set; }
        public string? VendorField2 { get; set; }
        public string? Document { get; set; }
        public string? AkerRevision { get; set; }
        public string? FileName { get; set; }
        public string? Author { get; set; }
        public DateTime? DateEntered { get; set; }
        public int? RowNumber { get; set; }

        // Системные
        public Guid Id { get; set; } = Guid.NewGuid();
        public IOPointStatus Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }
        public decimal? CurrentValue
        {
            get => _currentValue;
            set { _currentValue = value; OnPropertyChanged(); LastUpdate = DateTime.Now; }
        }
        public DateTime? LastUpdate
        {
            get => _lastUpdate;
            set { _lastUpdate = value; OnPropertyChanged(); }
        }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ModifiedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted { get; set; }
        public int Version { get; set; } = 1;
        public bool IsValid { get; set; }
        public bool IsTagUnique { get; set; }
        public string? TagValidationMessage { get; set; }
        public int DataQualityScore
        {
            get => _dataQualityScore;
            set { _dataQualityScore = Math.Clamp(value, 0, 100); OnPropertyChanged(); }
        }
        public DateTime? ValidationTimestamp { get; set; }
        public int ValidationErrorCount { get; set; }
        public int ValidationWarningCount { get; set; }

        // Дополнительные поля для импорта (10 текстовых полей)
        public string? Column1 { get; set; }
        public string? Column2 { get; set; }
        public string? Column3 { get; set; }
        public string? Column4 { get; set; }
        public string? Column5 { get; set; }
        public string? Column6 { get; set; }
        public string? Column7 { get; set; }
        public string? Column8 { get; set; }
        public string? Column9 { get; set; }
        public string? Column10 { get; set; }

        // Поля импорта
        public string? ImportSource { get; set; }
        public int? ImportRowNumber { get; set; }
        public bool IsImportValid { get; set; }
        public string? ImportError { get; set; }

        public string GetFullIdentifier() => Tag ?? Id.ToString();
        public string GetDisplayName() => string.IsNullOrWhiteSpace(Service) ? Tag ?? "Без имени" : Service;
        public bool IsActive() => Status == IOPointStatus.Active && !IsDeleted;
        public void MarkAsDeleted() { IsDeleted = true; DeletedAt = DateTime.Now; Status = IOPointStatus.Decommissioned; }
        public void Restore() { IsDeleted = false; DeletedAt = null; Status = IOPointStatus.Active; }

        public AlarmValidationResult ValidateAlarmHierarchy()
        {
            var result = new AlarmValidationResult();
            var alarms = new List<(string Name, decimal? Value)>
            {
                ("LL2", AlarmLL2), ("LL", AlarmLL), ("L", AlarmL),
                ("H", AlarmH), ("HH", AlarmHH), ("HH2", AlarmHH2)
            };

            decimal? prevValue = null;
            foreach (var (name, value) in alarms)
            {
                if (value.HasValue)
                {
                    if (prevValue.HasValue && value.Value <= prevValue.Value)
                    {
                        result.Errors.Add($"Нарушение иерархии: {name} = {value} должно быть > {prevValue}");
                        result.IsValid = false;
                    }
                    prevValue = value;
                }
            }

            if (string.IsNullOrWhiteSpace(AlarmUnit) && 
                (AlarmLL2.HasValue || AlarmLL.HasValue || AlarmL.HasValue || AlarmH.HasValue || AlarmHH.HasValue || AlarmHH2.HasValue))
            {
                result.Warnings.Add("Для уставок не указана единица измерения");
            }

            return result;
        }

        public ValidationResult Validate()
        {
            var result = new ValidationResult();
            ValidationTimestamp = DateTime.Now;
            ValidationErrorCount = 0;
            ValidationWarningCount = 0;

            if (string.IsNullOrWhiteSpace(Tag))
            {
                result.Errors.Add("Tag является обязательным полем");
                ValidationErrorCount++;
            }
            else if (Tag.Length < 3)
            {
                result.Errors.Add("Tag слишком короткий (минимум 3 символа)");
                ValidationErrorCount++;
            }

            var alarmResult = ValidateAlarmHierarchy();
            if (!alarmResult.IsValid)
            {
                result.Errors.AddRange(alarmResult.Errors);
                ValidationErrorCount += alarmResult.Errors.Count;
            }
            result.Warnings.AddRange(alarmResult.Warnings);
            ValidationWarningCount += alarmResult.Warnings.Count;

            if (RangeMin.HasValue && RangeMax.HasValue && RangeMin >= RangeMax)
            {
                result.Errors.Add("RangeMin должен быть меньше RangeMax");
                ValidationErrorCount++;
            }

            IsValid = result.Errors.Count == 0;
            result.IsValid = IsValid;
            return result;
        }

        public object Clone() => MemberwiseClone();
        public IOPoint DeepClone() => (IOPoint)Clone();

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // Операторы
        public static bool operator ==(IOPoint? left, IOPoint? right) => Equals(left?.Tag, right?.Tag);
        public static bool operator !=(IOPoint? left, IOPoint? right) => !(left == right);
        public static bool operator ==(IOPoint? point, string? tag) => Equals(point?.Tag, tag);
        public static bool operator !=(IOPoint? point, string? tag) => !(point == tag);
        public static bool operator ==(string? tag, IOPoint? point) => point == tag;
        public static bool operator !=(string? tag, IOPoint? point) => !(tag == point);
        public static implicit operator string?(IOPoint? point) => point?.Tag;
        public static explicit operator IOPoint(string tag) => new IOPoint { Tag = tag };

        public override bool Equals(object? obj) => obj is IOPoint other && Tag == other.Tag;
        public override int GetHashCode() => Tag?.GetHashCode() ?? 0;
        public override string ToString() => $"{Tag} - {Service}";
    }

    public class AlarmValidationResult
    {
        public bool IsValid { get; set; } = true;
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public List<string> SuggestedFixes { get; set; } = new();
        public DateTime ValidatedAt { get; set; } = DateTime.Now;
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; } = true;
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public DateTime ValidatedAt { get; set; } = DateTime.Now;
    }
}