using System.Text.Json;

namespace QuantForge.Core;

public sealed class DatasetCatalogFileStore
{
    private readonly string _filePath;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public DatasetCatalogFileStore(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("A catalog file path is required.", nameof(filePath));
        _filePath = Path.GetFullPath(filePath);
    }

    public void Save(DatasetCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        var directory = Path.GetDirectoryName(_filePath);
        if (string.IsNullOrWhiteSpace(directory))
            throw new InvalidOperationException("Catalog path has no usable parent directory.");
        Directory.CreateDirectory(directory);

        var payload = JsonSerializer.Serialize(catalog.List(), JsonOptions);
        var temp = _filePath + ".tmp-" + Guid.NewGuid().ToString("N");
        File.WriteAllText(temp, payload);
        try
        {
            File.Move(temp, _filePath, overwrite: true);
        }
        catch
        {
            if (File.Exists(temp)) File.Delete(temp);
            throw;
        }
    }

    public DatasetCatalog Load()
    {
        if (!File.Exists(_filePath))
            return new DatasetCatalog();

        var entries = JsonSerializer.Deserialize<DatasetCatalogEntry[]>(File.ReadAllText(_filePath), JsonOptions)
            ?? throw new InvalidOperationException("Dataset catalog file is empty or invalid.");
        var catalog = new DatasetCatalog();
        foreach (var entry in entries)
            catalog.Register(entry);
        return catalog;
    }
}
