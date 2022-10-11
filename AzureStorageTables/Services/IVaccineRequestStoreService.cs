using AzureStorageTables.Models;

namespace AzureStorageTables.Services;

public interface IVaccineRequestStoreService
{
    Task CreateRequestAsync(VaccineRequest entity);
    Task<VaccineRequest> GetRequestAsync(string curp, string state, string city);
    IAsyncEnumerable<VaccineRequest> GetRequestsByCityAsync(string state, string city);
}