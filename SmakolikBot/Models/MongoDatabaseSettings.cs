namespace SmakolikBot.Models;

public class MongoDatabaseSettings
{
    public string ConnectionString { get; set; } = null!;

    public string DatabaseName { get; set; } = null!;

    public MongoDatabaseCollections CollectionsName { get; set; } = null!;
}