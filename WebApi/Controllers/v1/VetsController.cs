namespace WebApi.Controllers.v1
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using AutoMapper;
    using FluentValidation.AspNetCore;
    using Application.Dtos.Vet;
    using Application.Enums;
    using Application.Interfaces.Vet;
    using Application.Validation.Vet;
    using Domain.Entities;
    using Microsoft.AspNetCore.JsonPatch;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using System.Threading.Tasks;

    [ApiController]
    [Route("api/Vets")]
    [ApiVersion("1.0")]
    [Authorize]
    public class VetsController: Controller
    {
        private readonly IVetRepository _vetRepository;
        private readonly IMapper _mapper;

        public VetsController(IVetRepository vetRepository
            , IMapper mapper)
        {
            _vetRepository = vetRepository ??
                throw new ArgumentNullException(nameof(vetRepository));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
        }


        [HttpGet(Name = "GetVets")]
        public async Task<ActionResult<IEnumerable<VetDto>>> GetVets([FromQuery] VetParametersDto vetParametersDto)
        {
            var vetsFromRepo = await _vetRepository.GetVetsAsync(vetParametersDto);

            var paginationMetadata = new
            {
                totalCount = vetsFromRepo.TotalCount,
                pageSize = vetsFromRepo.PageSize,
                pageNumber = vetsFromRepo.PageNumber,
                totalPages = vetsFromRepo.TotalPages,
                hasPrevious = vetsFromRepo.HasPrevious,
                hasNext = vetsFromRepo.HasNext
            };

            Response.Headers.Add("X-Pagination",
                JsonSerializer.Serialize(paginationMetadata));

            var vetsDto = _mapper.Map<IEnumerable<VetDto>>(vetsFromRepo);
            return Ok(vetsDto);
        }


        [HttpGet("{vetId}", Name = "GetVet")]
        public async Task<ActionResult<VetDto>> GetVet(int vetId)
        {
            var vetFromRepo = await _vetRepository.GetVetAsync(vetId);

            if (vetFromRepo == null)
            {
                return NotFound();
            }

            var vetDto = _mapper.Map<VetDto>(vetFromRepo);

            return Ok(vetDto);
        }

        [HttpPost]
        public async Task<ActionResult<VetDto>> AddVet(VetForCreationDto vetForCreation)
        {
            var validationResults = new VetForCreationDtoValidator().Validate(vetForCreation);
            validationResults.AddToModelState(ModelState, null);

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
                //return ValidationProblem();
            }

            var vet = _mapper.Map<Vet>(vetForCreation);
            _vetRepository.AddVet(vet);
            var saveSuccessful = await _vetRepository.SaveAsync();

            if(saveSuccessful)
            {
                var vetDto = await _vetRepository.GetVetAsync(vet.VetId); //get from repo for fk object, if needed
                return CreatedAtRoute("GetVet",
                    new { vetDto.VetId },
                    vetDto);
            }

            return StatusCode(500);
        }

        [HttpDelete("{vetId}")]
        public async Task<ActionResult> DeleteVet(int vetId)
        {
            var vetFromRepo = await _vetRepository.GetVetAsync(vetId);

            if (vetFromRepo == null)
            {
                return NotFound();
            }

            _vetRepository.DeleteVet(vetFromRepo);
            await _vetRepository.SaveAsync();

            return NoContent();
        }

        [HttpPut("{vetId}")]
        public async Task<IActionResult> UpdateVet(int vetId, VetForUpdateDto vet)
        {
            var vetFromRepo = await _vetRepository.GetVetAsync(vetId);

            if (vetFromRepo == null)
            {
                return NotFound();
            }

            var validationResults = new VetForUpdateDtoValidator().Validate(vet);
            validationResults.AddToModelState(ModelState, null);

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
                //return ValidationProblem();
            }

            _mapper.Map(vet, vetFromRepo);
            _vetRepository.UpdateVet(vetFromRepo);

            await _vetRepository.SaveAsync();

            return NoContent();
        }

        [HttpPatch("{vetId}")]
        public async Task<IActionResult> PartiallyUpdateVet(int vetId, JsonPatchDocument<VetForUpdateDto> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            var existingVet = await _vetRepository.GetVetAsync(vetId);

            if (existingVet == null)
            {
                return NotFound();
            }

            var vetToPatch = _mapper.Map<VetForUpdateDto>(existingVet); // map the vet we got from the database to an updatable vet model
            patchDoc.ApplyTo(vetToPatch, ModelState); // apply patchdoc updates to the updatable vet

            if (!TryValidateModel(vetToPatch))
            {
                return ValidationProblem(ModelState);
            }

            _mapper.Map(vetToPatch, existingVet); // apply updates from the updatable vet to the db entity so we can apply the updates to the database
            _vetRepository.UpdateVet(existingVet); // apply business updates to data if needed

            await _vetRepository.SaveAsync(); // save changes in the database

            return NoContent();
        }
    }
}