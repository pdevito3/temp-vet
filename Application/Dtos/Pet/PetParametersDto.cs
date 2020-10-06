namespace Application.Dtos.Pet
{
    public class PetParametersDto : PetPaginationParameters
    {
        public string Filters { get; set; }
        public string SortOrder { get; set; }
    }
}