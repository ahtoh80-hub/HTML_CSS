using OfficeOpenXml;

namespace PJT1.Tests;

/// <summary>
/// Creates a throwaway .xlsx file on disk and deletes it on dispose.
/// </summary>
public sealed class TempWorkbook : IDisposable
{
    public string Path { get; }

    public TempWorkbook(string[]? header, params (string? Tagname, string? Loop)[] rows)
        : this(header, rows.Select(r => new[] { r.Tagname, r.Loop }).ToArray())
    {
    }

    public TempWorkbook(string[]? header, string?[][] rows, string extension = ".xlsx")
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        Path = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            $"pjt1-tests-{Guid.NewGuid():N}{extension}");

        using var package = new ExcelPackage();
        var sheet = package.Workbook.Worksheets.Add("Sheet1");

        int row = 1;
        if (header != null)
        {
            for (int column = 0; column < header.Length; column++)
                sheet.Cells[row, column + 1].Value = header[column];
            row++;
        }

        foreach (var values in rows)
        {
            for (int column = 0; column < values.Length; column++)
            {
                if (values[column] != null)
                    sheet.Cells[row, column + 1].Value = values[column];
            }
            row++;
        }

        package.SaveAs(new FileInfo(Path));
    }

    public void Dispose()
    {
        if (File.Exists(Path))
            File.Delete(Path);
    }
}
