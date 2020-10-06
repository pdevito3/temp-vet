namespace Application.Validation.City
{
    using Application.Dtos.City;

    public class CityForCreationDtoValidator: CityForManipulationDtoValidator<CityForCreationDto>
    {
        public CityForCreationDtoValidator()
        {
            // add fluent validation rules that should only be run on creation operations here
            //https://fluentvalidation.net/
        }
    }
}