using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using System.Net.Http.Json;
using System.Net;
using AquaControl.Domain.Enums;

namespace AquaControl.Tests.Integration.API;

public class TanksControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public TanksControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetTanks_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/tanks");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateTank_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var createRequest = new
        {
            name = "Integration Test Tank",
            capacity = 1500,
            capacityUnit = "L",
            building = "Test Building",
            room = "Test Room",
            zone = (string?)null,
            latitude = (decimal?)null,
            longitude = (decimal?)null,
            tankType = "Freshwater"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tanks", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateTank_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var createRequest = new
        {
            name = "", // Invalid empty name
            capacity = -100, // Invalid negative capacity
            capacityUnit = "L",
            building = "Test Building",
            room = "Test Room",
            zone = (string?)null,
            latitude = (decimal?)null,
            longitude = (decimal?)null,
            tankType = "Freshwater"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tanks", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}

