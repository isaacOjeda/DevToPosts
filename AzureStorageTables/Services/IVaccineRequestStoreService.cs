using AzureStorageTables.Models;

namespace AzureStorageTables.Services;

public interface IVaccineRequestStoreService
{
    Task CreateRequestAsync(VaccineRequest entity);
    Task<VaccineRequest> GetRequestAsync(string curp, string state, string city);
    Task<List<VaccineRequest>> GetRequestsByCityAsync(string state, string city);
}