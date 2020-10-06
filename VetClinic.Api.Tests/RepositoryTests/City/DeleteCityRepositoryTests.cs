
namespace VetClinic.Api.Tests.RepositoryTests.City
{
    using Application.Dtos.City;
    using FluentAssertions;
    using VetClinic.Api.Tests.Fakes.City;
    using Infrastructure.Persistence.Contexts;
    using Infrastructure.Persistence.Repositories;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Sieve.Models;
    using Sieve.Services;
    using System;
    using System.Linq;
    using Xunit;
    using Application.Interfaces;
    using Moq;
    using Infrastructure.Shared.Services;

    [Collection("Sequential")]
    public class DeleteCityRepositoryTests
    { 
        
        [Fact]
        public void DeleteCity_ReturnsProperCount()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VetClinicDbContext>()
                .UseInMemoryDatabase(databaseName: $"CityDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var currentUser = new Mock<ICurrentUserService>();
            currentUser.SetupGet(c => c.UserId).Returns("testuser");
            var currentUserService = currentUser.Object;

            var fakeCityOne = new FakeCity { }.Generate();
            var fakeCityTwo = new FakeCity { }.Generate();
            var fakeCityThree = new FakeCity { }.Generate();

            //Act
            using (var context = new VetClinicDbContext(dbOptions, currentUserService, new DateTimeService()))
            {
                context.Cities.AddRange(fakeCityOne, fakeCityTwo, fakeCityThree);

                var service = new CityRepository(context, new SieveProcessor(sieveOptions));
                service.DeleteCity(fakeCityTwo);

                context.SaveChanges();

                //Assert
                var cityList = context.Cities.ToList();

                cityList.Should()
                    .NotBeEmpty()
                    .And.HaveCount(2);

                cityList.Should().ContainEquivalentOf(fakeCityOne);
                cityList.Should().ContainEquivalentOf(fakeCityThree);
                Assert.DoesNotContain(cityList, c => c == fakeCityTwo);

                context.Database.EnsureDeleted();
            }
        }
    } 
}