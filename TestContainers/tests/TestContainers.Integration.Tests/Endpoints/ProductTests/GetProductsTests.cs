using System.Net.Http.Json;
using FluentAssert;
using TestContainers.Integration.Tests.Infrastructure;
using TestContainers.Web.Entities;

namespace TestContainers.Integration.Tests.Endpoints.ProductTests;

[Collection(SqlServerContainerFixture.FixtureName)]
public class GetProductsTests : IClassFixture<WebApplicationFixture>
{
    private readonly HttpClient _client;

    public GetProductsTests(WebApplicationFixture factory)
    {
        _client = factory.HttpClient;
    }

    [Fact]
    public async Task GetProducts_ReturnsProducts()
    {
        // Arrange
        var response = await _client.GetAsync("/products");

        // Act
        response.EnsureSuccessStatusCode();

        var products = await response.Content.ReadFromJsonAsync<List<Product>>();

        // Assert
        products.ShouldNotBeNull();
        products.Count.ShouldBeGreaterThan(0);
    }
}