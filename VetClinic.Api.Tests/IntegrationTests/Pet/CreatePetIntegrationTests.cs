
namespace VetClinic.Api.Tests.IntegrationTests.Pet
{
    using Application.Dtos.Pet;
    using FluentAssertions;
    using VetClinic.Api.Tests.Fakes.Pet;
    using Microsoft.AspNetCore.Mvc.Testing;
    using System.Threading.Tasks;
    using Xunit;
    using Newtonsoft.Json;
    using System.Net.Http;
    using WebApi;
    using System.Collections.Generic;

    [Collection("Sequential")]
    public class CreatePetIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    { 
        private readonly CustomWebApplicationFactory _factory;

        public CreatePetIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        
        [Fact]
        public async Task PostPetReturnsSuccessCodeAndResourceWithAccurateFields()
        {
            // Arrange
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
            var fakePet = new FakePetDto().Generate();

            // Act
            var httpResponse = await client.PostAsJsonAsync("api/Pets", fakePet)
                .ConfigureAwait(false);

            // Assert
            httpResponse.EnsureSuccessStatusCode();

            var resultDto = JsonConvert.DeserializeObject<PetDto>(await httpResponse.Content.ReadAsStringAsync()
                .ConfigureAwait(false));

            httpResponse.StatusCode.Should().Be(201);
            resultDto.Name.Should().Be(fakePet.Name);
            resultDto.Type.Should().Be(fakePet.Type);
        }
    } 
}