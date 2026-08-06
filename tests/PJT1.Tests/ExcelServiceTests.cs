using OfficeOpenXml;
using PJT1.Services;

namespace PJT1.Tests;

public class ExcelServiceTests
{
    [Fact]
    public void ReadFirstTwoFieldsFromExcel_SkipsHeaderRowAndReadsBothColumns()
    {
        using var workbook = new TempWorkbook(
            new[] { "Tagname", "Loop" },
            ("2701-XZY-10101A", "L-101"),
            ("2701-XZY-10102B", "L-102"));

        var result = ExcelService.ReadFirstTwoFieldsFromExcel(workbook.Path);

        Assert.Equal(2, result.Count);
        Assert.Equal(new[] { "2701-XZY-10101A", "2701-XZY-10102B" }, result.Select(d => d.Tagname));
        Assert.Equal(new[] { "L-101", "L-102" }, result.Select(d => d.Loop));
        Assert.All(result, d => Assert.Equal(string.Empty, d.Comment));
    }

    [Fact]
    public void ReadFirstTwoFieldsFromExcel_TrimsWhitespace()
    {
        using var workbook = new TempWorkbook(
            new[] { "Tagname", "Loop" },
            ("  spaced tag  ", "\tL-101 "));

        var result = ExcelService.ReadFirstTwoFieldsFromExcel(workbook.Path);

        var only = Assert.Single(result);
        Assert.Equal("spaced tag", only.Tagname);
        Assert.Equal("L-101", only.Loop);
    }

    [Fact]
    public void ReadFirstTwoFieldsFromExcel_SkipsFullyEmptyRows()
    {
        using var workbook = new TempWorkbook(
            new[] { "Tagname", "Loop" },
            new string?[][]
            {
                new string?[] { "A", "L-1" },
                new string?[] { null, null },
                new string?[] { "B", "L-2" }
            });

        var result = ExcelService.ReadFirstTwoFieldsFromExcel(workbook.Path);

        Assert.Equal(new[] { "A", "B" }, result.Select(d => d.Tagname));
    }

    [Fact]
    public void ReadFirstTwoFieldsFromExcel_KeepsRowsWithOnlyOneFilledColumn()
    {
        using var workbook = new TempWorkbook(
            new[] { "Tagname", "Loop" },
            new string?[][]
            {
                new string?[] { "A", null },
                new string?[] { null, "L-2" }
            });

        var result = ExcelService.ReadFirstTwoFieldsFromExcel(workbook.Path);

        Assert.Equal(2, result.Count);
        Assert.Equal("A", result[0].Tagname);
        Assert.Equal(string.Empty, result[0].Loop);
        Assert.Equal(string.Empty, result[1].Tagname);
        Assert.Equal("L-2", result[1].Loop);
    }

    [Fact]
    public void ReadFirstTwoFieldsFromExcel_IgnoresColumnsBeyondTheSecond()
    {
        using var workbook = new TempWorkbook(
            new[] { "Tagname", "Loop", "Comment" },
            new string?[][] { new string?[] { "A", "L-1", "ignored" } });

        var result = ExcelService.ReadFirstTwoFieldsFromExcel(workbook.Path);

        Assert.Equal(string.Empty, Assert.Single(result).Comment);
    }

    [Fact]
    public void ReadFirstTwoFieldsFromExcel_HeaderOnlyWorkbook_ReturnsEmptyList()
    {
        using var workbook = new TempWorkbook(new[] { "Tagname", "Loop" }, Array.Empty<string?[]>());

        Assert.Empty(ExcelService.ReadFirstTwoFieldsFromExcel(workbook.Path));
    }

    [Fact]
    public void ReadFirstTwoFieldsFromExcel_MissingFile_ThrowsFileNotFoundException()
    {
        var path = Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.xlsx");

        var exception = Assert.Throws<FileNotFoundException>(
            () => ExcelService.ReadFirstTwoFieldsFromExcel(path));
        Assert.Contains(path, exception.Message);
    }

    [Fact]
    public void ReadFirstTwoFieldsFromExcel_UnsupportedExtension_ThrowsArgumentException()
    {
        var path = Path.Combine(Path.GetTempPath(), $"data-{Guid.NewGuid():N}.xls");
        File.WriteAllText(path, "not a workbook");

        try
        {
            var exception = Assert.Throws<ArgumentException>(
                () => ExcelService.ReadFirstTwoFieldsFromExcel(path));
            Assert.Contains(".xlsx", exception.Message);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void ReadFirstTwoFieldsFromExcel_AcceptsUppercaseExtension()
    {
        using var workbook = new TempWorkbook(
            new[] { "Tagname", "Loop" },
            new string?[][] { new string?[] { "A", "L-1" } },
            extension: ".XLSX");

        Assert.Single(ExcelService.ReadFirstTwoFieldsFromExcel(workbook.Path));
    }

    [Fact]
    public void ReadFirstTwoFieldsFromExcel_EmptyWorksheet_ThrowsWrappedException()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        var path = Path.Combine(Path.GetTempPath(), $"empty-{Guid.NewGuid():N}.xlsx");
        using (var package = new ExcelPackage())
        {
            package.Workbook.Worksheets.Add("Sheet1");
            package.SaveAs(new FileInfo(path));
        }

        try
        {
            var exception = Assert.Throws<Exception>(
                () => ExcelService.ReadFirstTwoFieldsFromExcel(path));
            Assert.Contains("Ошибка при чтении Excel", exception.Message);
            Assert.Contains("пуст", exception.Message);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
