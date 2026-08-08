using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IOPointManager.Models;

namespace IOPointManager.Interfaces
{
    public class ExcelColumnInfo
    {
        public string Name { get; set; } = string.Empty;
        public int Index { get; set; }
        public Type DataType { get; set; } = typeof(string);
        public List<object> SampleValues { get; set; } = new();
        public bool IsNumeric { get; set; }
        public bool IsDateTime { get; set; }
        public int NullCount { get; set; }
    }

    public class ExcelStructureInfo
    {
        public int TotalRows { get; set; }
        public int TotalColumns { get; set; }
        public int HeaderRow { get; set; }
        public List<ExcelColumnInfo> Columns { get; set; } = new();
        public string? SheetName { get; set; }
    }

    public class ExcelMappingSuggestion
    {
        public string IOPointField { get; set; } = string.Empty;
        public string ExcelColumn { get; set; } = string.Empty;
        public int Confidence { get; set; }
        public string? Reason { get; set; }
    }

    public class ExcelMappingTemplate
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Dictionary<string, string> Mappings { get; set; } = new();
        public Dictionary<string, string> Converters { get; set; } = new();
        public int Version { get; set; } = 1;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ModifiedAt { get; set; }
    }

    public class ExcelMappingResult
    {
        public bool IsValid { get; set; }
        public Dictionary<string, string> AppliedMappings { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public int Confidence { get; set; }
        public int MappedRows { get; set; }
        public int ErrorRows { get; set; }
    }

    public interface IExcelMappingService
    {
        event EventHandler<LogEventArgs>? ProgressReport;
        Task<ExcelStructureInfo> AnalyzeExcelFileAsync(string filePath, int headerRow = 1);
        Task<List<ExcelMappingSuggestion>> AutoDetectMappingAsync(string filePath, int headerRow = 1);
        Task<List<ExcelMappingSuggestion>> SuggestMappingAsync(string filePath, int headerRow = 1);
        Task<(List<IOPoint> points, ExcelMappingResult result)> ApplyMappingAsync(
            string filePath, ExcelMappingTemplate template, int startRow = 1, int? endRow = null);
        Task SaveMappingTemplateAsync(ExcelMappingTemplate template);
        Task<ExcelMappingTemplate?> LoadMappingTemplateAsync(string name);
        Task<IEnumerable<ExcelMappingTemplate>> GetMappingTemplatesAsync();
        Task DeleteMappingTemplateAsync(string name);
        ExcelMappingResult ValidateMapping(ExcelMappingTemplate template);
        Dictionary<string, string> GetFieldDescriptions();
        List<string> GetFieldNames();
    }
}