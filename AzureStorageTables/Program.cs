using Azure;
using AzureStorageTables.Models;
using AzureStorageTables.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IVaccineRequestStoreService, VaccineRequestStoreService>();

var app = builder.Build();

app.MapPost("api/vaccine-requests", async (VaccineRequestDto request, IVaccineRequestStoreService store) =>
{
    try
    {
        await store.CreateRequestAsync(new VaccineRequest
        {
            City = request.City,
            State = request.State,
            Curp = request.Curp,
            FullName = request.FullName
        });
    }
    catch (RequestFailedException ex)
        // Ya existe una solicitud con esta CURP
        when (ex.Status == StatusCodes.Status409Conflict)
    {
        return Results.BadRequest($"Ya existe una solicitud del CURP {request.Curp}");
    }

    return Results.Ok();
});

app.MapGet("api/vaccine-requests", (string state, string city, IVaccineRequestStoreService store) =>
{
    return Results.Ok(store
        .GetRequestsByCityAsync(state, city)
        .Select(s => new VaccineRequestDto(s.Curp, s.FullName, s.City, s.State))
    );
});

app.MapGet("api/vaccine-requests/{curp}", async(string curp, string state, string city, IVaccineRequestStoreService store) =>
{
    var request = await store.GetRequestAsync(curp, state, city);

    return new VaccineRequestDto(request.Curp, request.FullName, request.State, request.City);
});

app.Run();
