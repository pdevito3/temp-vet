namespace WebApi.Controllers.v1
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using AutoMapper;
    using FluentValidation.AspNetCore;
    using Application.Dtos.City;
    using Application.Enums;
    using Application.Interfaces.City;
    using Application.Validation.City;
    using Domain.Entities;
    using Microsoft.AspNetCore.JsonPatch;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using System.Threading.Tasks;

    [ApiController]
    [Route("api/Cities")]
    [ApiVersion("1.0")]
    public class CitiesController: Controller
    {
        private readonly ICityRepository _cityRepository;
        private readonly IMapper _mapper;

        public CitiesController(ICityRepository cityRepository
            , IMapper mapper)
        {
            _cityRepository = cityRepository ??
                throw new ArgumentNullException(nameof(cityRepository));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
        }


        [HttpGet(Name = "GetCities")]
        public async Task<ActionResult<IEnumerable<CityDto>>> GetCities([FromQuery] CityParametersDto cityParametersDto)
        {
            var citysFromRepo = await _cityRepository.GetCitiesAsync(cityParametersDto);

            var paginationMetadata = new
            {
                totalCount = citysFromRepo.TotalCount,
                pageSize = citysFromRepo.PageSize,
                pageNumber = citysFromRepo.PageNumber,
                totalPages = citysFromRepo.TotalPages,
                hasPrevious = citysFromRepo.HasPrevious,
                hasNext = citysFromRepo.HasNext
            };

            Response.Headers.Add("X-Pagination",
                JsonSerializer.Serialize(paginationMetadata));

            var citysDto = _mapper.Map<IEnumerable<CityDto>>(citysFromRepo);
            return Ok(citysDto);
        }


        [HttpGet("{cityId}", Name = "GetCity")]
        public async Task<ActionResult<CityDto>> GetCity(int cityId)
        {
            var cityFromRepo = await _cityRepository.GetCityAsync(cityId);

            if (cityFromRepo == null)
            {
                return NotFound();
            }

            var cityDto = _mapper.Map<CityDto>(cityFromRepo);

            return Ok(cityDto);
        }

        [HttpPost]
        public async Task<ActionResult<CityDto>> AddCity(CityForCreationDto cityForCreation)
        {
            var validationResults = new CityForCreationDtoValidator().Validate(cityForCreation);
            validationResults.AddToModelState(ModelState, null);

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
                //return ValidationProblem();
            }

            var city = _mapper.Map<City>(cityForCreation);
            _cityRepository.AddCity(city);
            var saveSuccessful = await _cityRepository.SaveAsync();

            if(saveSuccessful)
            {
                var cityDto = await _cityRepository.GetCityAsync(city.CityId); //get from repo for fk object, if needed
                return CreatedAtRoute("GetCity",
                    new { cityDto.CityId },
                    cityDto);
            }

            return StatusCode(500);
        }

        [HttpDelete("{cityId}")]
        public async Task<ActionResult> DeleteCity(int cityId)
        {
            var cityFromRepo = await _cityRepository.GetCityAsync(cityId);

            if (cityFromRepo == null)
            {
                return NotFound();
            }

            _cityRepository.DeleteCity(cityFromRepo);
            await _cityRepository.SaveAsync();

            return NoContent();
        }

        [HttpPut("{cityId}")]
        public async Task<IActionResult> UpdateCity(int cityId, CityForUpdateDto city)
        {
            var cityFromRepo = await _cityRepository.GetCityAsync(cityId);

            if (cityFromRepo == null)
            {
                return NotFound();
            }

            var validationResults = new CityForUpdateDtoValidator().Validate(city);
            validationResults.AddToModelState(ModelState, null);

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
                //return ValidationProblem();
            }

            _mapper.Map(city, cityFromRepo);
            _cityRepository.UpdateCity(cityFromRepo);

            await _cityRepository.SaveAsync();

            return NoContent();
        }

        [HttpPatch("{cityId}")]
        public async Task<IActionResult> PartiallyUpdateCity(int cityId, JsonPatchDocument<CityForUpdateDto> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            var existingCity = await _cityRepository.GetCityAsync(cityId);

            if (existingCity == null)
            {
                return NotFound();
            }

            var cityToPatch = _mapper.Map<CityForUpdateDto>(existingCity); // map the city we got from the database to an updatable city model
            patchDoc.ApplyTo(cityToPatch, ModelState); // apply patchdoc updates to the updatable city

            if (!TryValidateModel(cityToPatch))
            {
                return ValidationProblem(ModelState);
            }

            _mapper.Map(cityToPatch, existingCity); // apply updates from the updatable city to the db entity so we can apply the updates to the database
            _cityRepository.UpdateCity(existingCity); // apply business updates to data if needed

            await _cityRepository.SaveAsync(); // save changes in the database

            return NoContent();
        }
    }
}