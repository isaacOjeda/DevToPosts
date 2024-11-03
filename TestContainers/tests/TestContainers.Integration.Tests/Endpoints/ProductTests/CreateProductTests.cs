using System.Net;
using System.Net.Http.Json;
using FluentAssert;
using Microsoft.EntityFrameworkCore;
using TestContainers.Integration.Tests.Infrastructure;
using TestContainers.Web.Data;
using TestContainers.Web.Entities;

namespace TestContainers.Integration.Tests.Endpoints.ProductTests;

[Collection(SqlServerContainerFixture.FixtureName)]
public class CreateProductTests : IClassFixture<WebApplicationFixture>
{
    private readonly WebApplicationFixture _factory;
    private readonly AppDbContext _dbContext;
    private readonly HttpClient _client;

    public CreateProductTests(WebApplicationFixture factory)
    {
        _factory = factory;

        _dbContext = factory.DbContext;
        _client = factory.HttpClient;
    }

    [Fact]
    public async Task CreateProduct_ReturnsProduct()
    {
        // Arrange
        var category = await GetFirstCategory();
        var product = new Product
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 10.0m,
            CategoryId = category.Id
        };

        // Act
        var response = await _client.PostAsJsonAsync("/products", product);

        // Assert
        response.EnsureSuccessStatusCode();

        var createdProduct = await response.Content.ReadFromJsonAsync<Product>();

        createdProduct.ShouldNotBeNull();
        createdProduct.Name.ShouldBeEqualTo(product.Name);
        createdProduct.Price.ShouldBeEqualTo(product.Price);
    }

    [Fact]
    public async Task CreateProduct_Fails_WhenCategoryDoesNotExist()
    {
        // Arrange
        var product = new Product
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 10.0m,
            CategoryId = 0
        };

        // Act
        var response = await _client.PostAsJsonAsync("/products", product);

        // Assert
        response.StatusCode.ShouldBeEqualTo(HttpStatusCode.BadRequest);
    }

    private async Task<Category> GetFirstCategory()
    {
        return await _dbContext.Categories.FirstAsync();
    }
}