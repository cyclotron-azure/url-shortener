using Microsoft.Azure.Cosmos.Table;
using System.Text.Json;

namespace UrlShortener.Core.Domain;

public class StorageTableHelper
{
    private string StorageConnectionString { get; set; }

    /// <summary>
    /// Helper class for working with storage tables.
    /// </summary>
    public StorageTableHelper() { }


    public StorageTableHelper(string storageConnectionString)
    {
        StorageConnectionString = storageConnectionString;
    }

    /// <summary>
    /// Creates a CloudStorageAccount object from the storage connection string.
    /// </summary>
    /// <returns>The CloudStorageAccount object.</returns>
    public CloudStorageAccount CreateStorageAccountFromConnectionString()
    {
        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(StorageConnectionString);
        return storageAccount;
    }

    private CloudTable GetTable(string tableName)
    {
        CloudStorageAccount storageAccount = CreateStorageAccountFromConnectionString();
        CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
        CloudTable table = tableClient.GetTableReference(tableName);
        table.CreateIfNotExists();

        return table;
    }

    private CloudTable GetUrlsTable()
    {
        CloudTable table = GetTable("UrlsDetails");
        return table;
    }

    private CloudTable GetStatsTable()
    {
        CloudTable table = GetTable("ClickStats");
        return table;
    }

    /// <summary>
    /// Retrieves a ShortUrlEntity from the UrlsDetails table based on the provided row.
    /// </summary>
    /// <param name="row">The row to retrieve.</param>
    /// <returns>The retrieved ShortUrlEntity.</returns>
    public async Task<ShortUrlEntity> GetShortUrlEntity(ShortUrlEntity row)
    {
        TableOperation selOperation = TableOperation.Retrieve<ShortUrlEntity>(row.PartitionKey, row.RowKey);
        TableResult result = await GetUrlsTable().ExecuteAsync(selOperation).ConfigureAwait(false);
        ShortUrlEntity eShortUrl = result.Result as ShortUrlEntity;
        return eShortUrl;
    }

    /// <summary>
    /// Retrieves all ShortUrlEntities from the UrlsDetails table.
    /// </summary>
    /// <returns>A list of all ShortUrlEntities.</returns>
    public async Task<List<ShortUrlEntity>> GetAllShortUrlEntities()
    {
        var tblUrls = GetUrlsTable();
        TableContinuationToken? token = null;
        var lstShortUrl = new List<ShortUrlEntity>();
        do
        {
            // Retrieving all entities that are NOT the NextId entity 
            // (it's the only one in the partition "KEY")
            TableQuery<ShortUrlEntity> rangeQuery = new TableQuery<ShortUrlEntity>().Where(
                filter: TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.NotEqual, "KEY"));

            var queryResult = await tblUrls.ExecuteQuerySegmentedAsync(rangeQuery, token).ConfigureAwait(false);
            lstShortUrl.AddRange(queryResult.Results as List<ShortUrlEntity>);
            token = queryResult.ContinuationToken;
        } while (token != null);
        return lstShortUrl;
    }

    /// <summary>
    /// Returns the ShortUrlEntity of the specified vanity.
    /// </summary>
    /// <param name="vanity">The vanity to search for.</param>
    /// <returns>The ShortUrlEntity with the specified vanity.</returns>
    public async Task<ShortUrlEntity> GetShortUrlEntityByVanity(string vanity)
    {
        var tblUrls = GetUrlsTable();
        TableContinuationToken token = null;
        ShortUrlEntity shortUrlEntity = null;
        do
        {
            TableQuery<ShortUrlEntity> query = new TableQuery<ShortUrlEntity>().Where(
                filter: TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, vanity));
            var queryResult = await tblUrls.ExecuteQuerySegmentedAsync(query, token).ConfigureAwait(false);
            shortUrlEntity = queryResult.Results.FirstOrDefault();
        } while (token != null);

        return shortUrlEntity;
    }

    /// <summary>
    /// Saves a ClickStatsEntity to the ClickStats table.
    /// </summary>
    /// <param name="newStats">The ClickStatsEntity to save.</param>
    public async Task SaveClickStatsEntity(ClickStatsEntity newStats)
    {
        TableOperation insOperation = TableOperation.InsertOrMerge(newStats);
        TableResult result = await GetStatsTable().ExecuteAsync(insOperation).ConfigureAwait(false);
    }

    /// <summary>
    /// Saves a ShortUrlEntity to the UrlsDetails table.
    /// </summary>
    /// <param name="newShortUrl">The ShortUrlEntity to save.</param>
    /// <returns>The saved ShortUrlEntity.</returns>
    public async Task<ShortUrlEntity> SaveShortUrlEntity(ShortUrlEntity newShortUrl)
    {
        TableOperation insOperation = TableOperation.InsertOrMerge(newShortUrl);
        TableResult result = await GetUrlsTable().ExecuteAsync(insOperation).ConfigureAwait(false);
        ShortUrlEntity eShortUrl = result.Result as ShortUrlEntity;
        return eShortUrl;
    }

    /// <summary>
    /// Checks if a ShortUrlEntity with the specified vanity exists.
    /// </summary>
    /// <param name="vanity">The vanity to check.</param>
    /// <returns>True if the ShortUrlEntity exists, false otherwise.</returns>
    public async Task<bool> IfShortUrlEntityExistByVanity(string vanity)
    {
        ShortUrlEntity shortUrlEntity = await GetShortUrlEntityByVanity(vanity).ConfigureAwait(false);
        return (shortUrlEntity != null);
    }

    /// <summary>
    /// Checks if a ShortUrlEntity exists based on the provided row.
    /// </summary>
    /// <param name="row">The row to check.</param>
    /// <returns>True if the ShortUrlEntity exists, false otherwise.</returns>
    public async Task<bool> IfShortUrlEntityExist(ShortUrlEntity row)
    {
        ShortUrlEntity eShortUrl = await GetShortUrlEntity(row).ConfigureAwait(false);
        return (eShortUrl != null);
    }

    /// <summary>
    /// Gets the next table ID.
    /// </summary>
    /// <returns>The next table ID.</returns>
    public async Task<int> GetNextTableId()
    {
        // Get current ID
        TableOperation selOperation = TableOperation.Retrieve<NextId>("1", "KEY");
        TableResult result = await GetUrlsTable().ExecuteAsync(selOperation).ConfigureAwait(false);
        NextId entity = result.Result as NextId;

        if (entity == null)
        {
            entity = new NextId
            {
                PartitionKey = "1",
                RowKey = "KEY",
                Id = 1024
            };
        }
        entity.Id++;

        // Update
        TableOperation updOperation = TableOperation.InsertOrMerge(entity);

        // Execute the operation.
        await GetUrlsTable().ExecuteAsync(updOperation).ConfigureAwait(false);

        return entity.Id;
    }

    /// <summary>
    /// Updates a ShortUrlEntity in the UrlsDetails table.
    /// </summary>
    /// <param name="urlEntity">The ShortUrlEntity to update.</param>
    /// <returns>The updated ShortUrlEntity.</returns>
    public async Task<ShortUrlEntity> UpdateShortUrlEntity(ShortUrlEntity urlEntity)
    {
        ShortUrlEntity originalUrl = await GetShortUrlEntity(urlEntity).ConfigureAwait(false);
        originalUrl.Url = urlEntity.Url;
        originalUrl.Title = urlEntity.Title;
        originalUrl.SchedulesPropertyRaw = JsonSerializer.Serialize<List<Schedule>>(urlEntity.Schedules);

        return await SaveShortUrlEntity(originalUrl).ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieves all ClickStatsEntities from the ClickStats table based on the specified vanity.
    /// If vanity is null or empty, retrieves all ClickStatsEntities.
    /// </summary>
    /// <param name="vanity">The vanity to filter by.</param>
    /// <returns>A list of ClickStatsEntities.</returns>
    public async Task<List<ClickStatsEntity>> GetAllStatsByVanity(string vanity)
    {
        var tblUrls = GetStatsTable();
        TableContinuationToken token = null;
        var lstShortUrl = new List<ClickStatsEntity>();
        do
        {
            TableQuery<ClickStatsEntity> rangeQuery;

            if (string.IsNullOrEmpty(vanity))
            {
                rangeQuery = new TableQuery<ClickStatsEntity>();
            }
            else
            {
                rangeQuery = new TableQuery<ClickStatsEntity>().Where(
                filter: TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, vanity));
            }

            var queryResult = await tblUrls.ExecuteQuerySegmentedAsync(rangeQuery, token).ConfigureAwait(false);
            lstShortUrl.AddRange(queryResult.Results as List<ClickStatsEntity>);
            token = queryResult.ContinuationToken;
        } while (token != null);
        return lstShortUrl;
    }

    /// <summary>
    /// Archives a ShortUrlEntity in the UrlsDetails table.
    /// </summary>
    /// <param name="urlEntity">The ShortUrlEntity to archive.</param>
    /// <returns>The archived ShortUrlEntity.</returns>
    public async Task<ShortUrlEntity> ArchiveShortUrlEntity(ShortUrlEntity urlEntity)
    {
        ShortUrlEntity originalUrl = await GetShortUrlEntity(urlEntity).ConfigureAwait(false);
        originalUrl.IsArchived = true;

        return await SaveShortUrlEntity(originalUrl).ConfigureAwait(false);
    }
}