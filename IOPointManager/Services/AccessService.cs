using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Threading.Tasks;
using IOPointManager.Interfaces;
using IOPointManager.Models;

namespace IOPointManager.Services
{
    public class AccessService : IAccessService
    {
        private readonly IEventLogger _logger;
        private string? _connectionString;
        private bool _isConnected;

        public event EventHandler<LogEventArgs>? ProgressReport;

        public bool IsConnected => _isConnected;

        public AccessService(IEventLogger logger)
        {
            _logger = logger;
        }

        public bool Connect(string connectionString)
        {
            try
            {
                using var connection = new OleDbConnection(connectionString);
                connection.Open();
                connection.Close();

                _connectionString = connectionString;
                _isConnected = true;
                _logger.LogInfo("Подключение к Access установлено", "AccessService");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка подключения к Access: {ex.Message}", "AccessService");
                _isConnected = false;
                return false;
            }
        }

        public void Disconnect()
        {
            _isConnected = false;
            _connectionString = null;
            _logger.LogInfo("Отключение от Access", "AccessService");
        }

        public async Task<bool> CreateTableAsync(string tableName)
        {
            if (!_isConnected || string.IsNullOrEmpty(_connectionString))
                throw new InvalidOperationException("Нет подключения к Access");

            try
            {
                if (await TableExistsAsync(tableName))
                {
                    _logger.LogWarning($"Таблица '{tableName}' уже существует", "AccessService");
                    return true;
                }

                string createSql = $@"
                    CREATE TABLE [{tableName}] (
                        [Id] TEXT(36) NOT NULL,
                        [Tag] TEXT(50) NOT NULL,
                        [Area] LONG,
                        [Title] TEXT(255),
                        [Service] TEXT(255),
                        [ServiceEnglish] TEXT(255),
                        [InstrumentType] TEXT(255),
                        [InstrumentTypeEnglish] TEXT(255),
                        [System] TEXT(10),
                        [IoType] TEXT(50),
                        [Location] TEXT(50),
                        [Controller] TEXT(100),
                        [Pid] TEXT(50),
                        [SignalType] TEXT(50),
                        [ExProtection] TEXT(20),
                        [RangeMin] DOUBLE,
                        [RangeMax] DOUBLE,
                        [RangeUnit] TEXT(20),
                        [AlarmLL2] DOUBLE,
                        [AlarmLL] DOUBLE,
                        [AlarmL] DOUBLE,
                        [AlarmH] DOUBLE,
                        [AlarmHH] DOUBLE,
                        [AlarmHH2] DOUBLE,
                        [AlarmUnit] TEXT(20),
                        [CableId] TEXT(50),
                        [CableDescription] TEXT(255),
                        [CableType] TEXT(100),
                        [CableFrom] TEXT(100),
                        [CableTo] TEXT(100),
                        [CableLength] LONG,
                        [CableColor] TEXT(20),
                        [CablePair] LONG,
                        [Cpu] TEXT(50),
                        [ChassisMain] TEXT(50),
                        [ChassisRedundant] TEXT(50),
                        [ModuleSlot] TEXT(20),
                        [ModuleChannel] LONG,
                        [ModuleType] TEXT(50),
                        [Status] TEXT(20),
                        [CurrentValue] DOUBLE,
                        [LastUpdate] DATE,
                        [CreatedAt] DATE,
                        [ModifiedAt] DATE,
                        [IsDeleted] BIT,
                        [Version] LONG,
                        [DataQualityScore] LONG,
                        [Column1] TEXT(255),
                        [Column2] TEXT(255),
                        [Column3] TEXT(255),
                        [Column4] TEXT(255),
                        [Column5] TEXT(255),
                        [Column6] TEXT(255),
                        [Column7] TEXT(255),
                        [Column8] TEXT(255),
                        [Column9] TEXT(255),
                        [Column10] TEXT(255),
                        [ImportSource] TEXT(255),
                        [ImportRowNumber] LONG,
                        [IsImportValid] BIT,
                        [ImportError] TEXT(500)
                    )";

                await ExecuteNonQueryAsync(createSql);

                string indexSql = $"CREATE INDEX idx_Tag ON [{tableName}] ([Tag])";
                await ExecuteNonQueryAsync(indexSql);

                _logger.LogInfo($"Таблица '{tableName}' создана", "AccessService");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка создания таблицы: {ex.Message}", "AccessService");
                throw;
            }
        }

        public async Task<bool> ClearTableAsync(string tableName)
        {
            if (!_isConnected || string.IsNullOrEmpty(_connectionString))
                throw new InvalidOperationException("Нет подключения к Access");

            if (!await TableExistsAsync(tableName))
                throw new InvalidOperationException($"Таблица '{tableName}' не существует");

            try
            {
                string sql = $"DELETE FROM [{tableName}]";
                await ExecuteNonQueryAsync(sql);
                _logger.LogInfo($"Таблица '{tableName}' очищена", "AccessService");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка очистки таблицы: {ex.Message}", "AccessService");
                return false;
            }
        }

        public async Task<bool> BackupTableAsync(string tableName)
        {
            if (!_isConnected || string.IsNullOrEmpty(_connectionString))
                throw new InvalidOperationException("Нет подключения к Access");

            if (!await TableExistsAsync(tableName))
                throw new InvalidOperationException($"Таблица '{tableName}' не существует");

            try
            {
                string backupName = $"{tableName}_{DateTime.Now:yyyyMMdd_HHmmss}";
                string sql = $"SELECT * INTO [{backupName}] FROM [{tableName}]";
                await ExecuteNonQueryAsync(sql);
                _logger.LogInfo($"Резервная копия '{backupName}' создана", "AccessService");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка создания резервной копии: {ex.Message}", "AccessService");
                return false;
            }
        }

        public async Task<int> WritePointsAsync(string tableName, IEnumerable<IOPoint> points)
        {
            if (!_isConnected || string.IsNullOrEmpty(_connectionString))
                throw new InvalidOperationException("Нет подключения к Access");

            if (!await TableExistsAsync(tableName))
                await CreateTableAsync(tableName);

            int count = 0;
            var pointList = points.ToList();

            try
            {
                using var connection = new OleDbConnection(_connectionString);
                await connection.OpenAsync();

                using var transaction = connection.BeginTransaction();

                foreach (var point in pointList)
                {
                    try
                    {
                        string sql = @"
                            INSERT INTO [" + tableName + @"] (
                                Id, Tag, Area, Title, Service, ServiceEnglish, InstrumentType, InstrumentTypeEnglish,
                                System, IoType, Location, Controller, Pid, SignalType, ExProtection,
                                RangeMin, RangeMax, RangeUnit,
                                AlarmLL2, AlarmLL, AlarmL, AlarmH, AlarmHH, AlarmHH2, AlarmUnit,
                                CableId, CableDescription, CableType, CableFrom, CableTo, CableLength, CableColor, CablePair,
                                Cpu, ChassisMain, ChassisRedundant, ModuleSlot, ModuleChannel, ModuleType,
                                Status, CurrentValue, LastUpdate, CreatedAt, ModifiedAt, IsDeleted, Version, DataQualityScore,
                                Column1, Column2, Column3, Column4, Column5, Column6, Column7, Column8, Column9, Column10,
                                ImportSource, ImportRowNumber, IsImportValid, ImportError
                            ) VALUES (
                                @Id, @Tag, @Area, @Title, @Service, @ServiceEnglish, @InstrumentType, @InstrumentTypeEnglish,
                                @System, @IoType, @Location, @Controller, @Pid, @SignalType, @ExProtection,
                                @RangeMin, @RangeMax, @RangeUnit,
                                @AlarmLL2, @AlarmLL, @AlarmL, @AlarmH, @AlarmHH, @AlarmHH2, @AlarmUnit,
                                @CableId, @CableDescription, @CableType, @CableFrom, @CableTo, @CableLength, @CableColor, @CablePair,
                                @Cpu, @ChassisMain, @ChassisRedundant, @ModuleSlot, @ModuleChannel, @ModuleType,
                                @Status, @CurrentValue, @LastUpdate, @CreatedAt, @ModifiedAt, @IsDeleted, @Version, @DataQualityScore,
                                @Column1, @Column2, @Column3, @Column4, @Column5, @Column6, @Column7, @Column8, @Column9, @Column10,
                                @ImportSource, @ImportRowNumber, @IsImportValid, @ImportError
                            )";

                        using var cmd = new OleDbCommand(sql, connection, transaction);

                        cmd.Parameters.AddWithValue("@Id", point.Id.ToString());
                        cmd.Parameters.AddWithValue("@Tag", (object?)point.Tag ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Area", (object?)point.Area ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Title", (object?)point.Title ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Service", (object?)point.Service ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ServiceEnglish", (object?)point.ServiceEnglish ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@InstrumentType", (object?)point.InstrumentType ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@InstrumentTypeEnglish", (object?)point.InstrumentTypeEnglish ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@System", (object?)point.System?.ToString() ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@IoType", (object?)point.IoType ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Location", (object?)point.Location?.ToString() ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Controller", (object?)point.Controller ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Pid", (object?)point.Pid ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@SignalType", (object?)point.SignalType ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ExProtection", (object?)point.ExProtection?.ToString() ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@RangeMin", (object?)point.RangeMin ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@RangeMax", (object?)point.RangeMax ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@RangeUnit", (object?)point.RangeUnit ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@AlarmLL2", (object?)point.AlarmLL2 ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@AlarmLL", (object?)point.AlarmLL ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@AlarmL", (object?)point.AlarmL ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@AlarmH", (object?)point.AlarmH ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@AlarmHH", (object?)point.AlarmHH ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@AlarmHH2", (object?)point.AlarmHH2 ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@AlarmUnit", (object?)point.AlarmUnit ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@CableId", (object?)point.CableId ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@CableDescription", (object?)point.CableDescription ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@CableType", (object?)point.CableType ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@CableFrom", (object?)point.CableFrom ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@CableTo", (object?)point.CableTo ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@CableLength", (object?)point.CableLength ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@CableColor", (object?)point.CableColor ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@CablePair", (object?)point.CablePair ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Cpu", (object?)point.Cpu ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ChassisMain", (object?)point.ChassisMain ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ChassisRedundant", (object?)point.ChassisRedundant ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ModuleSlot", (object?)point.ModuleSlot ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ModuleChannel", (object?)point.ModuleChannel ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ModuleType", (object?)point.ModuleType ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Status", point.Status.ToString());
                        cmd.Parameters.AddWithValue("@CurrentValue", (object?)point.CurrentValue ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@LastUpdate", (object?)point.LastUpdate ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@CreatedAt", point.CreatedAt);
                        cmd.Parameters.AddWithValue("@ModifiedAt", (object?)point.ModifiedAt ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@IsDeleted", point.IsDeleted);
                        cmd.Parameters.AddWithValue("@Version", point.Version);
                        cmd.Parameters.AddWithValue("@DataQualityScore", point.DataQualityScore);
                        cmd.Parameters.AddWithValue("@Column1", (object?)point.Column1 ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Column2", (object?)point.Column2 ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Column3", (object?)point.Column3 ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Column4", (object?)point.Column4 ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Column5", (object?)point.Column5 ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Column6", (object?)point.Column6 ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Column7", (object?)point.Column7 ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Column8", (object?)point.Column8 ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Column9", (object?)point.Column9 ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Column10", (object?)point.Column10 ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ImportSource", (object?)point.ImportSource ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ImportRowNumber", (object?)point.ImportRowNumber ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@IsImportValid", point.IsImportValid);
                        cmd.Parameters.AddWithValue("@ImportError", (object?)point.ImportError ?? DBNull.Value);

                        await cmd.ExecuteNonQueryAsync();
                        count++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Ошибка записи точки {point.Tag}: {ex.Message}", "AccessService");
                    }
                }

                await transaction.CommitAsync();
                _logger.LogInfo($"Записано {count} из {pointList.Count} точек", "AccessService");
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка записи в Access: {ex.Message}", "AccessService");
                throw;
            }
        }

        public async Task<IEnumerable<IOPoint>> ReadPointsAsync(string tableName)
        {
            if (!_isConnected || string.IsNullOrEmpty(_connectionString))
                throw new InvalidOperationException("Нет подключения к Access");

            if (!await TableExistsAsync(tableName))
                throw new InvalidOperationException($"Таблица '{tableName}' не существует");

            var points = new List<IOPoint>();

            try
            {
                string sql = $"SELECT * FROM [{tableName}] ORDER BY Tag";

                using var connection = new OleDbConnection(_connectionString);
                await connection.OpenAsync();

                using var cmd = new OleDbCommand(sql, connection);
                using OleDbDataReader reader = await cmd.ExecuteReaderAsync() as OleDbDataReader 
                    ?? throw new InvalidOperationException("Не удалось получить OleDbDataReader");

                while (await reader.ReadAsync())
                {
                    var point = new IOPoint();

                    point.Id = Guid.TryParse(GetString(reader, "Id"), out var id) ? id : Guid.NewGuid();
                    point.Tag = GetString(reader, "Tag");
                    point.Area = GetInt(reader, "Area");
                    point.Title = GetString(reader, "Title");
                    point.Service = GetString(reader, "Service");
                    point.ServiceEnglish = GetString(reader, "ServiceEnglish");
                    point.InstrumentType = GetString(reader, "InstrumentType");
                    point.InstrumentTypeEnglish = GetString(reader, "InstrumentTypeEnglish");
                    
                    string systemStr = GetString(reader, "System") ?? string.Empty;
                    point.System = Enum.TryParse<SystemType>(systemStr, true, out var sys) ? sys : null;
                    
                    point.IoType = GetString(reader, "IoType");
                    
                    string locationStr = GetString(reader, "Location") ?? string.Empty;
                    point.Location = Enum.TryParse<LocationType>(locationStr, true, out var loc) ? loc : null;
                    
                    point.Controller = GetString(reader, "Controller");
                    point.Pid = GetString(reader, "Pid");
                    point.SignalType = GetString(reader, "SignalType");
                    
                    string exStr = GetString(reader, "ExProtection") ?? string.Empty;
                    point.ExProtection = Enum.TryParse<ExProtectionType>(exStr, true, out var ex) ? ex : null;
                    
                    point.RangeMin = GetDecimal(reader, "RangeMin");
                    point.RangeMax = GetDecimal(reader, "RangeMax");
                    point.RangeUnit = GetString(reader, "RangeUnit");
                    point.AlarmLL2 = GetDecimal(reader, "AlarmLL2");
                    point.AlarmLL = GetDecimal(reader, "AlarmLL");
                    point.AlarmL = GetDecimal(reader, "AlarmL");
                    point.AlarmH = GetDecimal(reader, "AlarmH");
                    point.AlarmHH = GetDecimal(reader, "AlarmHH");
                    point.AlarmHH2 = GetDecimal(reader, "AlarmHH2");
                    point.AlarmUnit = GetString(reader, "AlarmUnit");
                    point.CableId = GetString(reader, "CableId");
                    point.CableDescription = GetString(reader, "CableDescription");
                    point.CableType = GetString(reader, "CableType");
                    point.CableFrom = GetString(reader, "CableFrom");
                    point.CableTo = GetString(reader, "CableTo");
                    point.CableLength = GetInt(reader, "CableLength");
                    point.CableColor = GetString(reader, "CableColor");
                    point.CablePair = GetInt(reader, "CablePair");
                    point.Cpu = GetString(reader, "Cpu");
                    point.ChassisMain = GetString(reader, "ChassisMain");
                    point.ChassisRedundant = GetString(reader, "ChassisRedundant");
                    point.ModuleSlot = GetString(reader, "ModuleSlot");
                    point.ModuleChannel = GetInt(reader, "ModuleChannel");
                    point.ModuleType = GetString(reader, "ModuleType");
                    
                    string statusStr = GetString(reader, "Status") ?? "Active";
                    point.Status = Enum.TryParse<IOPointStatus>(statusStr, true, out var status) ? status : IOPointStatus.Active;
                    
                    point.CurrentValue = GetDecimal(reader, "CurrentValue");
                    point.LastUpdate = GetDateTime(reader, "LastUpdate");
                    point.CreatedAt = GetDateTime(reader, "CreatedAt") ?? DateTime.Now;
                    point.ModifiedAt = GetDateTime(reader, "ModifiedAt");
                    point.IsDeleted = GetBool(reader, "IsDeleted");
                    point.Version = GetInt(reader, "Version") ?? 1;
                    point.DataQualityScore = GetInt(reader, "DataQualityScore") ?? 100;
                    point.Column1 = GetString(reader, "Column1");
                    point.Column2 = GetString(reader, "Column2");
                    point.Column3 = GetString(reader, "Column3");
                    point.Column4 = GetString(reader, "Column4");
                    point.Column5 = GetString(reader, "Column5");
                    point.Column6 = GetString(reader, "Column6");
                    point.Column7 = GetString(reader, "Column7");
                    point.Column8 = GetString(reader, "Column8");
                    point.Column9 = GetString(reader, "Column9");
                    point.Column10 = GetString(reader, "Column10");
                    point.ImportSource = GetString(reader, "ImportSource");
                    point.ImportRowNumber = GetInt(reader, "ImportRowNumber");
                    point.IsImportValid = GetBool(reader, "IsImportValid");
                    point.ImportError = GetString(reader, "ImportError");

                    points.Add(point);
                }

                _logger.LogInfo($"Прочитано {points.Count} точек из Access", "AccessService");
                return points;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка чтения из Access: {ex.Message}", "AccessService");
                throw;
            }
        }

        #region Вспомогательные методы чтения из OleDbDataReader

        private string? GetString(OleDbDataReader reader, string columnName)
        {
            try
            {
                int idx = reader.GetOrdinal(columnName);
                return reader.IsDBNull(idx) ? null : reader.GetString(idx);
            }
            catch { return null; }
        }

        private int? GetInt(OleDbDataReader reader, string columnName)
        {
            try
            {
                int idx = reader.GetOrdinal(columnName);
                return reader.IsDBNull(idx) ? null : reader.GetInt32(idx);
            }
            catch { return null; }
        }

        private decimal? GetDecimal(OleDbDataReader reader, string columnName)
        {
            try
            {
                int idx = reader.GetOrdinal(columnName);
                return reader.IsDBNull(idx) ? null : Convert.ToDecimal(reader.GetValue(idx));
            }
            catch { return null; }
        }

        private DateTime? GetDateTime(OleDbDataReader reader, string columnName)
        {
            try
            {
                int idx = reader.GetOrdinal(columnName);
                return reader.IsDBNull(idx) ? null : reader.GetDateTime(idx);
            }
            catch { return null; }
        }

        private bool GetBool(OleDbDataReader reader, string columnName)
        {
            try
            {
                int idx = reader.GetOrdinal(columnName);
                return !reader.IsDBNull(idx) && Convert.ToBoolean(reader.GetValue(idx));
            }
            catch { return false; }
        }

        #endregion

        public async Task<IEnumerable<string>> GetTableNamesAsync()
        {
            if (!_isConnected || string.IsNullOrEmpty(_connectionString))
                throw new InvalidOperationException("Нет подключения к Access");

            var tables = new List<string>();

            try
            {
                using var connection = new OleDbConnection(_connectionString);
                await connection.OpenAsync();

                DataTable schema = connection.GetSchema("Tables", new[] { null, null, null, "TABLE" });
                foreach (DataRow row in schema.Rows)
                {
                    string? name = row["TABLE_NAME"]?.ToString();
                    if (!string.IsNullOrEmpty(name))
                        tables.Add(name);
                }

                return tables;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка получения списка таблиц: {ex.Message}", "AccessService");
                throw;
            }
        }

        public async Task<bool> TableExistsAsync(string tableName)
        {
            IEnumerable<string> tables = await GetTableNamesAsync();
            return tables.Any(t => string.Equals(t, tableName, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<bool> DropTableAsync(string tableName)
        {
            if (!_isConnected || string.IsNullOrEmpty(_connectionString))
                throw new InvalidOperationException("Нет подключения к Access");

            if (!await TableExistsAsync(tableName))
                return true;

            try
            {
                string sql = $"DROP TABLE [{tableName}]";
                await ExecuteNonQueryAsync(sql);
                _logger.LogInfo($"Таблица '{tableName}' удалена", "AccessService");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка удаления таблицы: {ex.Message}", "AccessService");
                return false;
            }
        }

        private async Task ExecuteNonQueryAsync(string sql)
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new InvalidOperationException("Строка подключения не задана");

            using var connection = new OleDbConnection(_connectionString);
            await connection.OpenAsync();
            using var cmd = new OleDbCommand(sql, connection);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}