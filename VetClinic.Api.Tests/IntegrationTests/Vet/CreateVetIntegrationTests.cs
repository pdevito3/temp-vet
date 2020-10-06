
namespace VetClinic.Api.Tests.IntegrationTests.Vet
{
    using Application.Dtos.Vet;
    using FluentAssertions;
    using VetClinic.Api.Tests.Fakes.Vet;
    using Microsoft.AspNetCore.Mvc.Testing;
    using System.Threading.Tasks;
    using Xunit;
    using Newtonsoft.Json;
    using System.Net.Http;
    using WebApi;
    using System.Collections.Generic;

    [Collection("Sequential")]
    public class CreateVetIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    { 
        private readonly CustomWebApplicationFactory _factory;

        public CreateVetIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        
        [Fact]
        public async Task PostVetReturnsSuccessCodeAndResourceWithAccurateFields()
        {
            // Arrange
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
            var fakeVet = new FakeVetDto().Generate();

            // Act
            var httpResponse = await client.PostAsJsonAsync("api/Vets", fakeVet)
                .ConfigureAwait(false);

            // Assert
            httpResponse.EnsureSuccessStatusCode();

            var resultDto = JsonConvert.DeserializeObject<VetDto>(await httpResponse.Content.ReadAsStringAsync()
                .ConfigureAwait(false));

            httpResponse.StatusCode.Should().Be(201);
            resultDto.Name.Should().Be(fakeVet.Name);
            resultDto.Capacity.Should().Be(fakeVet.Capacity);
            resultDto.OpenDate.Should().Be(fakeVet.OpenDate);
            resultDto.HasSpayNeuter.Should().Be(fakeVet.HasSpayNeuter);
        }
    } 
}