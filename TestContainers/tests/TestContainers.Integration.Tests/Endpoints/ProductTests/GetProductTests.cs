using System.Net.Http.Json;
using FluentAssert;
using TestContainers.Integration.Tests.Infrastructure;
using TestContainers.Web.Data;
using TestContainers.Web.Entities;

namespace TestContainers.Integration.Tests.Endpoints.ProductTests;

[Collection(SqlServerContainerFixture.FixtureName)]
public class GetProductTests : IClassFixture<WebApplicationFixture>
{
    private readonly WebApplicationFixture _factory;
    private readonly AppDbContext _dbContext;

    public GetProductTests(WebApplicationFixture factory)
    {
        _factory = factory;
        _dbContext = factory.DbContext;
    }

    [Theory]
    [InlineData("Laptop")]
    [InlineData("Smartphone")]
    [InlineData("Tablet")]
    [InlineData("Headphones")]
    [InlineData("Book 1")]
    [InlineData("Book 2")]
    [InlineData("Book 3")]
    public async Task GetProduct_ReturnsSuccessStatusCode(string productName)
    {
        // Arrange
        var client = _factory.CreateClient();
        var productId = _dbContext.Products.Single(p => p.Name == productName).Id;

        // Act
        var response = await client.GetAsync($"/products/{productId}");

        // Assert
        response.EnsureSuccessStatusCode();

        var product = await response.Content.ReadFromJsonAsync<Product>();

        product.ShouldNotBeNull();
        product.Id.ShouldBeEqualTo(productId);
    }
}