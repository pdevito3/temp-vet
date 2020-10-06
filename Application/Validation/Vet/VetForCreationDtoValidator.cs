namespace Application.Validation.Vet
{
    using Application.Dtos.Vet;

    public class VetForCreationDtoValidator: VetForManipulationDtoValidator<VetForCreationDto>
    {
        public VetForCreationDtoValidator()
        {
            // add fluent validation rules that should only be run on creation operations here
            //https://fluentvalidation.net/
        }
    }
}