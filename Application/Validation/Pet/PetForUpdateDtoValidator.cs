namespace Application.Validation.Pet
{
    using Application.Dtos.Pet;

    public class PetForUpdateDtoValidator: PetForManipulationDtoValidator<PetForUpdateDto>
    {
        public PetForUpdateDtoValidator()
        {
            // add fluent validation rules that should only be run on update operations here
            //https://fluentvalidation.net/
        }
    }
}