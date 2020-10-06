namespace Application.Validation.Vet
{
    using Application.Dtos.Vet;

    public class VetForUpdateDtoValidator: VetForManipulationDtoValidator<VetForUpdateDto>
    {
        public VetForUpdateDtoValidator()
        {
            // add fluent validation rules that should only be run on update operations here
            //https://fluentvalidation.net/
        }
    }
}