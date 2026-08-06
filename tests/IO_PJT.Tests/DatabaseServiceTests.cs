using IO_PJT.Services;

namespace IO_PJT.Tests;

/// <summary>
/// Only the platform independent surface of <see cref="DatabaseService"/> is covered here:
/// every other member opens an OleDb connection, which requires the Windows ACE/Jet providers.
/// </summary>
public class DatabaseServiceTests
{
    private static string TempPath(string extension) =>
        Path.Combine(Path.GetTempPath(), $"io-pjt-tests-{Guid.NewGuid():N}{extension}");

    [Theory]
    [InlineData(".accdb")]
    [InlineData(".ACCDB")]
    [InlineData(".mdb")]
    [InlineData("")]
    public void Constructor_AcceptsAnyDatabasePathWithoutTouchingTheProvider(string extension)
    {
        var service = new DatabaseService(TempPath(extension));

        Assert.False(service.DatabaseExists());
    }

    [Fact]
    public void DatabaseExists_ReturnsFalseWhenFileIsMissing()
    {
        Assert.False(new DatabaseService(TempPath(".accdb")).DatabaseExists());
    }

    [Fact]
    public void DatabaseExists_ReturnsTrueWhenFileIsPresent()
    {
        var path = TempPath(".accdb");
        File.WriteAllText(path, string.Empty);

        try
        {
            Assert.True(new DatabaseService(path).DatabaseExists());
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void DatabaseExists_ReturnsFalseForADirectoryPath()
    {
        Assert.False(new DatabaseService(Path.GetTempPath()).DatabaseExists());
    }

    [Fact]
    public void CreateEmptyDatabase_IsANoOpWhenTheDatabaseAlreadyExists()
    {
        var path = TempPath(".accdb");
        File.WriteAllText(path, "existing content");

        try
        {
            new DatabaseService(path).CreateEmptyDatabase();

            Assert.Equal("existing content", File.ReadAllText(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void ProviderProbes_ReportUnavailabilityInsteadOfThrowing()
    {
        if (OperatingSystem.IsWindows())
            return;

        Assert.False(DatabaseService.IsAceProviderAvailable());
        Assert.False(DatabaseService.IsJetProviderAvailable());
    }
}
