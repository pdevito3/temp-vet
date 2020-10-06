namespace WebApi.Controllers.v1
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using AutoMapper;
    using FluentValidation.AspNetCore;
    using Application.Dtos.Pet;
    using Application.Enums;
    using Application.Interfaces.Pet;
    using Application.Validation.Pet;
    using Domain.Entities;
    using Microsoft.AspNetCore.JsonPatch;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using System.Threading.Tasks;

    [ApiController]
    [Route("api/Pets")]
    [ApiVersion("1.0")]
    public class PetsController: Controller
    {
        private readonly IPetRepository _petRepository;
        private readonly IMapper _mapper;

        public PetsController(IPetRepository petRepository
            , IMapper mapper)
        {
            _petRepository = petRepository ??
                throw new ArgumentNullException(nameof(petRepository));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
        }


        [HttpGet(Name = "GetPets")]
        public async Task<ActionResult<IEnumerable<PetDto>>> GetPets([FromQuery] PetParametersDto petParametersDto)
        {
            var petsFromRepo = await _petRepository.GetPetsAsync(petParametersDto);

            var paginationMetadata = new
            {
                totalCount = petsFromRepo.TotalCount,
                pageSize = petsFromRepo.PageSize,
                pageNumber = petsFromRepo.PageNumber,
                totalPages = petsFromRepo.TotalPages,
                hasPrevious = petsFromRepo.HasPrevious,
                hasNext = petsFromRepo.HasNext
            };

            Response.Headers.Add("X-Pagination",
                JsonSerializer.Serialize(paginationMetadata));

            var petsDto = _mapper.Map<IEnumerable<PetDto>>(petsFromRepo);
            return Ok(petsDto);
        }


        [HttpGet("{petId}", Name = "GetPet")]
        public async Task<ActionResult<PetDto>> GetPet(int petId)
        {
            var petFromRepo = await _petRepository.GetPetAsync(petId);

            if (petFromRepo == null)
            {
                return NotFound();
            }

            var petDto = _mapper.Map<PetDto>(petFromRepo);

            return Ok(petDto);
        }

        [HttpPost]
        public async Task<ActionResult<PetDto>> AddPet(PetForCreationDto petForCreation)
        {
            var validationResults = new PetForCreationDtoValidator().Validate(petForCreation);
            validationResults.AddToModelState(ModelState, null);

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
                //return ValidationProblem();
            }

            var pet = _mapper.Map<Pet>(petForCreation);
            _petRepository.AddPet(pet);
            var saveSuccessful = await _petRepository.SaveAsync();

            if(saveSuccessful)
            {
                var petDto = await _petRepository.GetPetAsync(pet.PetId); //get from repo for fk object, if needed
                return CreatedAtRoute("GetPet",
                    new { petDto.PetId },
                    petDto);
            }

            return StatusCode(500);
        }

        [HttpDelete("{petId}")]
        public async Task<ActionResult> DeletePet(int petId)
        {
            var petFromRepo = await _petRepository.GetPetAsync(petId);

            if (petFromRepo == null)
            {
                return NotFound();
            }

            _petRepository.DeletePet(petFromRepo);
            await _petRepository.SaveAsync();

            return NoContent();
        }

        [HttpPut("{petId}")]
        public async Task<IActionResult> UpdatePet(int petId, PetForUpdateDto pet)
        {
            var petFromRepo = await _petRepository.GetPetAsync(petId);

            if (petFromRepo == null)
            {
                return NotFound();
            }

            var validationResults = new PetForUpdateDtoValidator().Validate(pet);
            validationResults.AddToModelState(ModelState, null);

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
                //return ValidationProblem();
            }

            _mapper.Map(pet, petFromRepo);
            _petRepository.UpdatePet(petFromRepo);

            await _petRepository.SaveAsync();

            return NoContent();
        }

        [HttpPatch("{petId}")]
        public async Task<IActionResult> PartiallyUpdatePet(int petId, JsonPatchDocument<PetForUpdateDto> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            var existingPet = await _petRepository.GetPetAsync(petId);

            if (existingPet == null)
            {
                return NotFound();
            }

            var petToPatch = _mapper.Map<PetForUpdateDto>(existingPet); // map the pet we got from the database to an updatable pet model
            patchDoc.ApplyTo(petToPatch, ModelState); // apply patchdoc updates to the updatable pet

            if (!TryValidateModel(petToPatch))
            {
                return ValidationProblem(ModelState);
            }

            _mapper.Map(petToPatch, existingPet); // apply updates from the updatable pet to the db entity so we can apply the updates to the database
            _petRepository.UpdatePet(existingPet); // apply business updates to data if needed

            await _petRepository.SaveAsync(); // save changes in the database

            return NoContent();
        }
    }
}