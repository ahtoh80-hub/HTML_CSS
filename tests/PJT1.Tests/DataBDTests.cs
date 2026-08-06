using PJT1.Models;

namespace PJT1.Tests;

public class DataBDTests
{
    [Fact]
    public void DefaultConstructor_InitializesStringsToEmptyAndSetsCreatedDate()
    {
        var before = DateTime.Now;

        var data = new DataBD();

        Assert.Equal(0, data.Id);
        Assert.Equal(string.Empty, data.Tagname);
        Assert.Equal(string.Empty, data.Loop);
        Assert.Equal(string.Empty, data.Comment);
        Assert.InRange(data.CreatedDate, before.AddSeconds(-5), DateTime.Now.AddSeconds(5));
    }

    [Fact]
    public void ImportConstructor_SetsTagnameAndLoopAndLeavesCommentEmpty()
    {
        var data = new DataBD("2701-XZY-10101A", "LoopA");

        Assert.Equal("2701-XZY-10101A", data.Tagname);
        Assert.Equal("LoopA", data.Loop);
        Assert.Equal(string.Empty, data.Comment);
    }

    [Fact]
    public void ImportConstructor_ConvertsNullsToEmptyStrings()
    {
        var data = new DataBD(null!, null!);

        Assert.Equal(string.Empty, data.Tagname);
        Assert.Equal(string.Empty, data.Loop);
        Assert.Equal(string.Empty, data.Comment);
    }

    [Fact]
    public void FullConstructor_SetsAllThreeFields()
    {
        var data = new DataBD("Motor1", "LoopA", "Двигатель 1");

        Assert.Equal("Motor1", data.Tagname);
        Assert.Equal("LoopA", data.Loop);
        Assert.Equal("Двигатель 1", data.Comment);
    }

    [Fact]
    public void FullConstructor_ConvertsNullsToEmptyStrings()
    {
        var data = new DataBD(null!, null!, null!);

        Assert.Equal(string.Empty, data.Tagname);
        Assert.Equal(string.Empty, data.Loop);
        Assert.Equal(string.Empty, data.Comment);
    }

    [Fact]
    public void ToString_JoinsFieldsWithDashes()
    {
        var data = new DataBD("Motor1", "LoopA", "Двигатель 1");

        Assert.Equal("Motor1 - LoopA - Двигатель 1", data.ToString());
    }

    [Fact]
    public void ToString_OnEmptyRecord_ContainsOnlySeparators()
    {
        Assert.Equal(" -  - ", new DataBD().ToString());
    }

    [Fact]
    public void Properties_AreMutable()
    {
        var created = new DateTime(2024, 5, 17, 8, 30, 0);
        var data = new DataBD { Id = 42, Tagname = "T", Loop = "L", Comment = "C", CreatedDate = created };

        Assert.Equal(42, data.Id);
        Assert.Equal("T", data.Tagname);
        Assert.Equal("L", data.Loop);
        Assert.Equal("C", data.Comment);
        Assert.Equal(created, data.CreatedDate);
    }
}
