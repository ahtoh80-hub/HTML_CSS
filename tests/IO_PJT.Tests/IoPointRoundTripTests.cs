using IO_PJT.Models;

namespace IO_PJT.Tests;

/// <summary>
/// The model classes mirror the Access table columns one to one, so every property has to survive
/// a set/get round trip: a mismatched or dropped property silently corrupts an exported I/O list.
/// </summary>
public class IoPointRoundTripTests
{
    [Fact]
    public void Identification_KeepsEveryColumn()
    {
        var id = new Identification
        {
            Code = 1,
            Area = 1401,
            Title = "Title",
            Loop = "Loop",
            ILoop = "ILoop",
            LoopPc = "LoopPc",
            Tag = "Tag",
            TagPc = "TagPc",
            Service = "Service",
            InstrumentType = "InstrumentType",
            ServiceEng = "ServiceEng",
            InstrumentTypeEng = "InstrumentTypeEng",
            Sys = SystemType.GDS,
            IoType = "DOR-P",
            Controller = "2000-S-SC-B01",
            Location = "Field",
            Pid = "Pid"
        };

        Assert.Equal(1, id.Code);
        Assert.Equal(1401, id.Area);
        Assert.Equal("Title", id.Title);
        Assert.Equal("Loop", id.Loop);
        Assert.Equal("ILoop", id.ILoop);
        Assert.Equal("LoopPc", id.LoopPc);
        Assert.Equal("Tag", id.Tag);
        Assert.Equal("TagPc", id.TagPc);
        Assert.Equal("Service", id.Service);
        Assert.Equal("InstrumentType", id.InstrumentType);
        Assert.Equal("ServiceEng", id.ServiceEng);
        Assert.Equal("InstrumentTypeEng", id.InstrumentTypeEng);
        Assert.Equal(SystemType.GDS, id.Sys);
        Assert.Equal("DOR-P", id.IoType);
        Assert.Equal("2000-S-SC-B01", id.Controller);
        Assert.Equal("Field", id.Location);
        Assert.Equal("Pid", id.Pid);
    }

    [Fact]
    public void Signal_KeepsEveryColumn()
    {
        var signal = new Signal
        {
            Sys = "Sys",
            SysType = SystemType.DCS,
            Pid = "Pid",
            Loc = "Loc",
            SigType = "AI",
            Ex = "Ex ia",
            SubSys = "SubSys",
            Aux = "Aux"
        };

        Assert.Equal("Sys", signal.Sys);
        Assert.Equal(SystemType.DCS, signal.SysType);
        Assert.Equal("Pid", signal.Pid);
        Assert.Equal("Loc", signal.Loc);
        Assert.Equal("AI", signal.SigType);
        Assert.Equal("Ex ia", signal.Ex);
        Assert.Equal("SubSys", signal.SubSys);
        Assert.Equal("Aux", signal.Aux);
    }

    [Fact]
    public void Ranges_KeepsBothRangesAndAlarms()
    {
        var ranges = new Ranges
        {
            Primary = new Ranges.Range { Min = -50m, Max = 150m, Unit = "degC" },
            Secondary = new Ranges.Range { Min = 4m, Max = 20m, Unit = "mA" },
            Alarms = new Ranges.Alarm { LL = -40m, L = -20m, H = 100m, HH = 140m },
            AlarmUnit = "degC"
        };

        Assert.Equal(-50m, ranges.Primary.Min);
        Assert.Equal(150m, ranges.Primary.Max);
        Assert.Equal("degC", ranges.Primary.Unit);
        Assert.Equal(4m, ranges.Secondary!.Min);
        Assert.Equal(20m, ranges.Secondary.Max);
        Assert.Equal("mA", ranges.Secondary.Unit);
        Assert.Equal(-40m, ranges.Alarms!.LL);
        Assert.Equal(-20m, ranges.Alarms.L);
        Assert.Equal(100m, ranges.Alarms.H);
        Assert.Equal(140m, ranges.Alarms.HH);
        Assert.Equal("degC", ranges.AlarmUnit);
    }

    [Fact]
    public void Cable_KeepsEveryColumn()
    {
        var cable = new Cable
        {
            Id = "C-1",
            Desc = "Desc",
            Type = "2x1.5",
            Desig = "Desig",
            From = "JB-1",
            To = "MC-1",
            Len = 250,
            Color = "Blue",
            Pair = 4,
            Note = "Note",
            TitleAker = "TitleAker",
            Drum = "Drum-7",
            Volt = "300V"
        };

        Assert.Equal("C-1", cable.Id);
        Assert.Equal("Desc", cable.Desc);
        Assert.Equal("2x1.5", cable.Type);
        Assert.Equal("Desig", cable.Desig);
        Assert.Equal("JB-1", cable.From);
        Assert.Equal("MC-1", cable.To);
        Assert.Equal(250, cable.Len);
        Assert.Equal("Blue", cable.Color);
        Assert.Equal(4, cable.Pair);
        Assert.Equal("Note", cable.Note);
        Assert.Equal("TitleAker", cable.TitleAker);
        Assert.Equal("Drum-7", cable.Drum);
        Assert.Equal("300V", cable.Volt);
    }

    [Fact]
    public void Controller_KeepsCabinetsChassisAndModule()
    {
        var controller = new Controller
        {
            CableTecon = "CableTecon",
            Mcc = "Mcc",
            CtrlCab = "CtrlCab",
            MarshCab = "MarshCab",
            Cpu = "Cpu",
            Chassis = new ChassisInfo { Main = "0", Red = "1" },
            Module = new ModuleInfo
            {
                Slot = "3",
                SlotRed = "4",
                Main1 = "M1",
                Main2 = "M2",
                Red1 = "R1",
                Red2 = "R2",
                Ch = 16,
                Type = "AI"
            }
        };

        Assert.Equal("CableTecon", controller.CableTecon);
        Assert.Equal("Mcc", controller.Mcc);
        Assert.Equal("CtrlCab", controller.CtrlCab);
        Assert.Equal("MarshCab", controller.MarshCab);
        Assert.Equal("Cpu", controller.Cpu);
        Assert.Equal("0", controller.Chassis!.Main);
        Assert.Equal("1", controller.Chassis.Red);
        Assert.Equal("3", controller.Module!.Slot);
        Assert.Equal("4", controller.Module.SlotRed);
        Assert.Equal("M1", controller.Module.Main1);
        Assert.Equal("M2", controller.Module.Main2);
        Assert.Equal("R1", controller.Module.Red1);
        Assert.Equal("R2", controller.Module.Red2);
        Assert.Equal(16, controller.Module.Ch);
        Assert.Equal("AI", controller.Module.Type);
    }

    [Fact]
    public void Revision_KeepsEveryColumn()
    {
        var revision = new Revision
        {
            No = 3,
            Desc = "Desc",
            Package = "Package",
            Field1 = "Field1",
            Field2 = "Field2",
            Doc = "Doc",
            Aker = "Aker"
        };

        Assert.Equal(3, revision.No);
        Assert.Equal("Desc", revision.Desc);
        Assert.Equal("Package", revision.Package);
        Assert.Equal("Field1", revision.Field1);
        Assert.Equal("Field2", revision.Field2);
        Assert.Equal("Doc", revision.Doc);
        Assert.Equal("Aker", revision.Aker);
    }
}
