using IO_PJT.Models;

namespace IO_PJT.Tests;

public class IoPointTests
{
    [Fact]
    public void NewIoPoint_HasAllSectionsUnset()
    {
        var point = new IoPoint();

        Assert.Null(point.Id);
        Assert.Null(point.Signal);
        Assert.Null(point.Ranges);
        Assert.Null(point.Cable);
        Assert.Null(point.Hardware);
        Assert.Null(point.Rev);
    }

    [Fact]
    public void Identification_TagPcDefaultsToEmptyStringWhileOtherFieldsAreNull()
    {
        var id = new Identification();

        Assert.Equal(string.Empty, id.TagPc);
        Assert.Null(id.Tag);
        Assert.Null(id.Code);
        Assert.Null(id.Area);
        Assert.Null(id.Sys);
    }

    [Fact]
    public void Ranges_PrimaryIsInitializedWithZeroToHundredPercentRange()
    {
        var ranges = new Ranges();

        Assert.NotNull(ranges.Primary);
        Assert.Equal(0m, ranges.Primary.Min);
        Assert.Equal(100m, ranges.Primary.Max);
        Assert.Null(ranges.Primary.Unit);
        Assert.Null(ranges.Secondary);
        Assert.Null(ranges.Alarms);
    }

    [Fact]
    public void Ranges_AlarmSetpointsAreOptional()
    {
        var alarm = new Ranges.Alarm { LL = -1.5m, HH = 120m };

        Assert.Equal(-1.5m, alarm.LL);
        Assert.Null(alarm.L);
        Assert.Null(alarm.H);
        Assert.Equal(120m, alarm.HH);
    }

    [Fact]
    public void IoPoint_SectionsCanBeComposed()
    {
        var point = new IoPoint
        {
            Id = new Identification { Tag = "2701-XZY-10101A", TagPc = "2701_XZY_10101A", Sys = SystemType.SIS },
            Signal = new Signal { SigType = "DOR-P", SysType = SystemType.SIS },
            Ranges = new Ranges
            {
                Primary = new Ranges.Range { Min = 0m, Max = 250m, Unit = "kPa" },
                Alarms = new Ranges.Alarm { L = 10m, H = 200m },
                AlarmUnit = "kPa"
            },
            Cable = new Cable { Id = "C-1", Len = 120, Pair = 2 },
            Hardware = new Controller
            {
                Cpu = "CPU-01",
                Chassis = new ChassisInfo { Main = "1", Red = "2" },
                Module = new ModuleInfo { Slot = "3", Ch = 8, Type = "DO" }
            },
            Rev = new Revision { No = 2, Desc = "Issued for construction" }
        };

        Assert.Equal(SystemType.SIS, point.Id!.Sys);
        Assert.Equal("2701_XZY_10101A", point.Id.TagPc);
        Assert.Equal(250m, point.Ranges!.Primary.Max);
        Assert.Equal("kPa", point.Ranges.AlarmUnit);
        Assert.Equal(10m, point.Ranges.Alarms!.L);
        Assert.Equal(120, point.Cable!.Len);
        Assert.Equal("1", point.Hardware!.Chassis!.Main);
        Assert.Equal(8, point.Hardware.Module!.Ch);
        Assert.Equal(2, point.Rev!.No);
    }

    [Fact]
    public void SystemType_DeclaresDcsSisAndGds()
    {
        Assert.Equal(new[] { SystemType.DCS, SystemType.SIS, SystemType.GDS }, Enum.GetValues<SystemType>());
    }

    [Theory]
    [InlineData(SystemType.DCS, "DCS")]
    [InlineData(SystemType.SIS, "SIS")]
    [InlineData(SystemType.GDS, "GDS")]
    public void SystemType_NamesMatchTheValuesStoredInTheDatabase(SystemType value, string expected)
    {
        Assert.Equal(expected, value.ToString());
    }
}
