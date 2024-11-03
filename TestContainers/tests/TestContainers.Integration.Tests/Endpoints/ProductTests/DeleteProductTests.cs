using System.Net;
using FluentAssert;
using Microsoft.EntityFrameworkCore;
using TestContainers.Integration.Tests.Infrastructure;
using TestContainers.Web.Data;
using TestContainers.Web.Entities;

namespace TestContainers.Integration.Tests.Endpoints.ProductTests;

[Collection(SqlServerContainerFixture.FixtureName)]
public class DeleteProductTests : IClassFixture<WebApplicationFixture>
{
    private readonly HttpClient _client;
    private readonly AppDbContext _dbContext;

    public DeleteProductTests(WebApplicationFixture factory)
    {
        _client = factory.HttpClient;
        _dbContext = factory.DbContext;
    }

    [Theory]
    [InlineData("Laptop")]
    [InlineData("Smartphone")]
    [InlineData("Tablet")]
    public async Task DeleteProduct_ReturnsNoContent(string productName)
    {
        // Arrange
        var productId = await _dbContext.Products
            .Where(p => p.Name == productName)
            .Select(p => p.Id)
            .FirstOrDefaultAsync();

        // Act
        var response = await _client.DeleteAsync($"/products/{productId}");

        // Assert
        response.StatusCode.ShouldBeEqualTo(HttpStatusCode.NoContent);

        var deletedProduct = await FindProductById(productId);
        deletedProduct.ShouldBeNull();
    }

    [Fact]
    public async Task DeleteProduct_ReturnsNotFound()
    {
        // Act
        var response = await _client.DeleteAsync("/products/0");

        // Assert
        response.StatusCode.ShouldBeEqualTo(HttpStatusCode.NotFound);
    }

    private async Task<Product?> FindProductById(int id)
    {
        return await _dbContext.Products.FindAsync(id);
    }
}