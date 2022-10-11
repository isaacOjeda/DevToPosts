using Azure.Data.Tables;
using AzureStorageTables.Models;

namespace AzureStorageTables.Services;

public class VaccineRequestStoreService : IVaccineRequestStoreService
{
    public const string TableName = "VaccineRequests";
    private TableClient _tableClient;

    public VaccineRequestStoreService(IConfiguration config)
    {
        _tableClient = new TableClient(config["TableStorage:ConnectionString"], TableName);
    }

    public async Task CreateRequestAsync(VaccineRequest entity)
    {
        await _tableClient.CreateIfNotExistsAsync();

        entity.PartitionKey = $"{entity.State}_{entity.City}";
        entity.RowKey = entity.Curp;

        var response = await _tableClient.AddEntityAsync(entity);

        // TODO: Handle errors or something
    }

    public async Task<VaccineRequest> GetRequestAsync(string curp, string state, string city)
    {
        var results = _tableClient
            .QueryAsync<VaccineRequest>(q => q.PartitionKey == $"{state}_{city}" && q.RowKey == curp);

        await foreach (var entity in results)
        {
            return entity;
        }

        return null;
    }

    public IAsyncEnumerable<VaccineRequest> GetRequestsByCityAsync(string state, string city) =>
         _tableClient
            .QueryAsync<VaccineRequest>(q => q.PartitionKey == $"{state}_{city}");

}