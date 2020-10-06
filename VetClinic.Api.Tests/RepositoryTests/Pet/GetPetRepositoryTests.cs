
namespace VetClinic.Api.Tests.RepositoryTests.Pet
{
    using Application.Dtos.Pet;
    using FluentAssertions;
    using VetClinic.Api.Tests.Fakes.Pet;
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
    public class GetPetRepositoryTests
    { 
        
        [Fact]
        public void GetPet_ParametersMatchExpectedValues()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VetClinicDbContext>()
                .UseInMemoryDatabase(databaseName: $"PetDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var currentUser = new Mock<ICurrentUserService>();
            currentUser.SetupGet(c => c.UserId).Returns("testuser");
            var currentUserService = currentUser.Object;

            var fakePet = new FakePet { }.Generate();

            //Act
            using (var context = new VetClinicDbContext(dbOptions, currentUserService, new DateTimeService()))
            {
                context.Pets.AddRange(fakePet);
                context.SaveChanges();

                var service = new PetRepository(context, new SieveProcessor(sieveOptions));

                //Assert
                var petById = service.GetPet(fakePet.PetId);
                                petById.PetId.Should().Be(fakePet.PetId);
                petById.Name.Should().Be(fakePet.Name);
                petById.Type.Should().Be(fakePet.Type);
            }
        }
        
        [Fact]
        public async void GetPetsAsync_CountMatchesAndContainsEquivalentObjects()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VetClinicDbContext>()
                .UseInMemoryDatabase(databaseName: $"PetDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var currentUser = new Mock<ICurrentUserService>();
            currentUser.SetupGet(c => c.UserId).Returns("testuser");
            var currentUserService = currentUser.Object;

            var fakePetOne = new FakePet { }.Generate();
            var fakePetTwo = new FakePet { }.Generate();
            var fakePetThree = new FakePet { }.Generate();

            //Act
            using (var context = new VetClinicDbContext(dbOptions, currentUserService, new DateTimeService()))
            {
                context.Pets.AddRange(fakePetOne, fakePetTwo, fakePetThree);
                context.SaveChanges();

                var service = new PetRepository(context, new SieveProcessor(sieveOptions));

                var petRepo = await service.GetPetsAsync(new PetParametersDto());

                //Assert
                petRepo.Should()
                    .NotBeEmpty()
                    .And.HaveCount(3);

                petRepo.Should().ContainEquivalentOf(fakePetOne);
                petRepo.Should().ContainEquivalentOf(fakePetTwo);
                petRepo.Should().ContainEquivalentOf(fakePetThree);

                context.Database.EnsureDeleted();
            }
        }
        
        [Fact]
        public async void GetPetsAsync_ReturnExpectedPageSize()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VetClinicDbContext>()
                .UseInMemoryDatabase(databaseName: $"PetDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var currentUser = new Mock<ICurrentUserService>();
            currentUser.SetupGet(c => c.UserId).Returns("testuser");
            var currentUserService = currentUser.Object;

            var fakePetOne = new FakePet { }.Generate();
            var fakePetTwo = new FakePet { }.Generate();
            var fakePetThree = new FakePet { }.Generate();

            //Act
            using (var context = new VetClinicDbContext(dbOptions, currentUserService, new DateTimeService()))
            {
                context.Pets.AddRange(fakePetOne, fakePetTwo, fakePetThree);
                context.SaveChanges();

                var service = new PetRepository(context, new SieveProcessor(sieveOptions));

                var petRepo = await service.GetPetsAsync(new PetParametersDto { PageSize = 2 });

                //Assert
                petRepo.Should()
                    .NotBeEmpty()
                    .And.HaveCount(2);

                petRepo.Should().ContainEquivalentOf(fakePetOne);
                petRepo.Should().ContainEquivalentOf(fakePetTwo);

                context.Database.EnsureDeleted();
            }
        }
        
        [Fact]
        public async void GetPetsAsync_ReturnExpectedPageNumberAndSize()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VetClinicDbContext>()
                .UseInMemoryDatabase(databaseName: $"PetDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var currentUser = new Mock<ICurrentUserService>();
            currentUser.SetupGet(c => c.UserId).Returns("testuser");
            var currentUserService = currentUser.Object;

            var fakePetOne = new FakePet { }.Generate();
            var fakePetTwo = new FakePet { }.Generate();
            var fakePetThree = new FakePet { }.Generate();

            //Act
            using (var context = new VetClinicDbContext(dbOptions, currentUserService, new DateTimeService()))
            {
                context.Pets.AddRange(fakePetOne, fakePetTwo, fakePetThree);
                context.SaveChanges();

                var service = new PetRepository(context, new SieveProcessor(sieveOptions));

                var petRepo = await service.GetPetsAsync(new PetParametersDto { PageSize = 1, PageNumber = 2 });

                //Assert
                petRepo.Should()
                    .NotBeEmpty()
                    .And.HaveCount(1);

                petRepo.Should().ContainEquivalentOf(fakePetTwo);

                context.Database.EnsureDeleted();
            }
        }
        
        
    } 
}