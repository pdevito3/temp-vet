namespace Application.Dtos.Vet
{
    public class VetParametersDto : VetPaginationParameters
    {
        public string Filters { get; set; }
        public string SortOrder { get; set; }
    }
}