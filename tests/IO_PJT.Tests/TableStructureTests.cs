using System.Text.RegularExpressions;
using IO_PJT.Models;

namespace IO_PJT.Tests;

public class TableStructureTests
{
    private static readonly List<TableField> Fields = TableStructure.GetFields();

    [Fact]
    public void GetFields_ReturnsANewListOnEveryCall()
    {
        Assert.NotSame(TableStructure.GetFields(), TableStructure.GetFields());
    }

    [Fact]
    public void GetFields_HasNoDuplicateNames()
    {
        var duplicates = Fields
            .GroupBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);

        Assert.Empty(duplicates);
    }

    [Fact]
    public void GetFields_EveryFieldHasNameAndType()
    {
        Assert.All(Fields, field =>
        {
            Assert.False(string.IsNullOrWhiteSpace(field.Name));
            Assert.False(string.IsNullOrWhiteSpace(field.Type));
        });
    }

    [Fact]
    public void GetFields_DeclaresExactlyOnePrimaryKeyAsLastField()
    {
        var primaryKeys = Fields.Where(f => f.Type.Contains("PRIMARY KEY")).ToList();

        var primaryKey = Assert.Single(primaryKeys);
        Assert.Equal("ID", primaryKey.Name);
        Assert.Equal("COUNTER PRIMARY KEY", primaryKey.Type);
        Assert.Same(Fields[^1], primaryKey);
    }

    [Theory]
    [InlineData("Tag")]
    [InlineData("TagPc")]
    [InlineData("Sig_Type")]
    [InlineData("Range_Min")]
    [InlineData("Alarm_HH")]
    [InlineData("Cable_Id")]
    [InlineData("Ctrl_ModuleType")]
    [InlineData("Rev_No")]
    public void GetFields_ContainsKeyColumnOfEveryGroup(string expectedName)
    {
        Assert.Contains(expectedName, Fields.Select(f => f.Name));
    }

    [Theory]
    [InlineData("Code", "INTEGER")]
    [InlineData("Area", "INTEGER")]
    [InlineData("Cable_Len", "INTEGER")]
    [InlineData("Cable_Pair", "INTEGER")]
    [InlineData("Ctrl_Channel", "INTEGER")]
    [InlineData("Rev_No", "INTEGER")]
    public void GetFields_NumericColumnsUseIntegerType(string name, string expectedType)
    {
        Assert.Equal(expectedType, Fields.Single(f => f.Name == name).Type);
    }

    [Theory]
    [InlineData("Range_Min")]
    [InlineData("Range_Max")]
    [InlineData("Range2_Min")]
    [InlineData("Range2_Max")]
    [InlineData("Alarm_LL")]
    [InlineData("Alarm_L")]
    [InlineData("Alarm_H")]
    [InlineData("Alarm_HH")]
    public void GetFields_MeasurementColumnsUseDecimalType(string name)
    {
        Assert.Equal("DECIMAL(20,6)", Fields.Single(f => f.Name == name).Type);
    }

    [Fact]
    public void GetFields_TextColumnsDeclareALengthWithinAccessLimit()
    {
        var textFields = Fields.Where(f => f.Type.StartsWith("TEXT", StringComparison.Ordinal)).ToList();

        Assert.NotEmpty(textFields);
        Assert.All(textFields, field =>
        {
            var match = Regex.Match(field.Type, @"^TEXT\((\d+)\)$");
            Assert.True(match.Success, $"Unexpected TEXT declaration for {field.Name}: {field.Type}");
            Assert.InRange(int.Parse(match.Groups[1].Value), 1, 255);
        });
    }

    [Fact]
    public void GetFields_OnlyUsesKnownAccessTypes()
    {
        var knownTypes = new[] { "INTEGER", "DECIMAL(20,6)", "COUNTER PRIMARY KEY" };

        Assert.All(Fields, field =>
            Assert.True(
                knownTypes.Contains(field.Type) || field.Type.StartsWith("TEXT(", StringComparison.Ordinal),
                $"Unknown type for {field.Name}: {field.Type}"));
    }

    [Fact]
    public void GetFields_NamesAreValidAccessIdentifiers()
    {
        Assert.All(Fields, field => Assert.Matches(@"^[A-Za-z][A-Za-z0-9_]*$", field.Name));
    }

    [Fact]
    public void TableField_ConstructorStoresNameAndType()
    {
        var field = new TableField("Tag", "TEXT(50)");

        Assert.Equal("Tag", field.Name);
        Assert.Equal("TEXT(50)", field.Type);
    }

    [Fact]
    public void TableField_PropertiesAreMutable()
    {
        var field = new TableField("Tag", "TEXT(50)") { Name = "Tag2", Type = "TEXT(80)" };

        Assert.Equal("Tag2", field.Name);
        Assert.Equal("TEXT(80)", field.Type);
    }
}
