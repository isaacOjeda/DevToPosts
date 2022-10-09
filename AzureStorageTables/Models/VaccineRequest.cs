using Azure;
using Azure.Data.Tables;

namespace AzureStorageTables.Models;

public class VaccineRequest : ITableEntity
{
    public string Curp { get; set; }
    public string FullName { get; set; }
    public string City { get; set; }
    public string State { get; set; }

    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}