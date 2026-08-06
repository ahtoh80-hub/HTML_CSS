using PJT1.Models;
using PJT1.Repositories;

namespace PJT1.Tests;

public class DataBDRepositoryTests
{
    private static DataBDRepository RepositoryWith(params string[] tagnames)
    {
        var repository = new DataBDRepository();
        foreach (var tagname in tagnames)
            repository.Add(new DataBD(tagname, "Loop"));
        return repository;
    }

    [Fact]
    public void NewRepository_IsEmpty()
    {
        var repository = new DataBDRepository();

        Assert.Equal(0, repository.Count());
        Assert.Empty(repository.GetAll());
    }

    [Fact]
    public void Add_AssignsSequentialIdsStartingAtOne()
    {
        var repository = new DataBDRepository();
        var first = new DataBD("A", "L");
        var second = new DataBD("B", "L");

        repository.Add(first);
        repository.Add(second);

        Assert.Equal(1, first.Id);
        Assert.Equal(2, second.Id);
        Assert.Equal(2, repository.Count());
    }

    [Fact]
    public void Add_Null_ThrowsArgumentNullException()
    {
        var repository = new DataBDRepository();

        var exception = Assert.Throws<ArgumentNullException>(() => repository.Add(null!));
        Assert.Equal("data", exception.ParamName);
    }

    [Fact]
    public void AddRange_AssignsIdsToEveryItem()
    {
        var repository = RepositoryWith("A");
        var batch = new[] { new DataBD("B", "L"), new DataBD("C", "L") };

        repository.AddRange(batch);

        Assert.Equal(new[] { 2, 3 }, batch.Select(d => d.Id));
        Assert.Equal(3, repository.Count());
    }

    [Fact]
    public void AddRange_EmptyCollection_DoesNotConsumeIds()
    {
        var repository = new DataBDRepository();

        repository.AddRange(Array.Empty<DataBD>());
        var added = new DataBD("A", "L");
        repository.Add(added);

        Assert.Equal(1, added.Id);
    }

    [Fact]
    public void AddRange_Null_ThrowsArgumentNullException()
    {
        var repository = new DataBDRepository();

        var exception = Assert.Throws<ArgumentNullException>(() => repository.AddRange(null!));
        Assert.Equal("dataList", exception.ParamName);
    }

    [Fact]
    public void GetById_ReturnsMatchingRecord()
    {
        var repository = RepositoryWith("A", "B");

        var found = repository.GetById(2);

        Assert.NotNull(found);
        Assert.Equal("B", found!.Tagname);
    }

    [Fact]
    public void GetById_UnknownId_ReturnsNull()
    {
        var repository = RepositoryWith("A");

        Assert.Null(repository.GetById(99));
    }

    [Fact]
    public void GetAll_ReturnsSnapshotThatDoesNotAffectRepository()
    {
        var repository = RepositoryWith("A", "B");

        var snapshot = repository.GetAll().ToList();
        snapshot.Clear();

        Assert.Equal(2, repository.Count());
    }

    [Fact]
    public void GetAll_PreservesInsertionOrder()
    {
        var repository = RepositoryWith("A", "B", "C");

        Assert.Equal(new[] { "A", "B", "C" }, repository.GetAll().Select(d => d.Tagname));
    }

    [Fact]
    public void Delete_RemovesOnlyTheMatchingRecord()
    {
        var repository = RepositoryWith("A", "B");

        repository.Delete(1);

        Assert.Equal(1, repository.Count());
        Assert.Null(repository.GetById(1));
        Assert.NotNull(repository.GetById(2));
    }

    [Fact]
    public void Delete_UnknownId_LeavesRepositoryUnchanged()
    {
        var repository = RepositoryWith("A");

        repository.Delete(42);

        Assert.Equal(1, repository.Count());
    }

    [Fact]
    public void Delete_DoesNotReuseIdOfRemovedRecord()
    {
        var repository = RepositoryWith("A", "B");

        repository.Delete(2);
        var added = new DataBD("C", "L");
        repository.Add(added);

        Assert.Equal(3, added.Id);
    }

    [Fact]
    public void Clear_EmptiesRepositoryAndRestartsIdNumbering()
    {
        var repository = RepositoryWith("A", "B");

        repository.Clear();
        var added = new DataBD("C", "L");
        repository.Add(added);

        Assert.Equal(1, repository.Count());
        Assert.Equal(1, added.Id);
    }

    [Fact]
    public void Repository_ImplementsInterface()
    {
        Assert.IsAssignableFrom<IDataBDRepository>(new DataBDRepository());
    }

    [Fact]
    public void ImportFromExcel_AddsRowsFromWorkbook()
    {
        using var workbook = new TempWorkbook(
            new[] { "Tagname", "Loop" },
            ("2701-XZY-10101A", "L-101"),
            ("2701-XZY-10102B", "L-102"));
        var repository = new DataBDRepository();

        repository.ImportFromExcel(workbook.Path);

        Assert.Equal(2, repository.Count());
        Assert.Equal(new[] { 1, 2 }, repository.GetAll().Select(d => d.Id));
        Assert.Equal("2701-XZY-10101A", repository.GetById(1)!.Tagname);
    }

    [Fact]
    public void ImportFromExcel_MissingFile_ThrowsWrappedException()
    {
        var repository = new DataBDRepository();

        var exception = Assert.Throws<Exception>(
            () => repository.ImportFromExcel(Path.Combine(Path.GetTempPath(), "no-such-file.xlsx")));

        Assert.Contains("Ошибка при импорте из Excel", exception.Message);
        Assert.IsType<FileNotFoundException>(exception.InnerException);
        Assert.Equal(0, repository.Count());
    }
}
