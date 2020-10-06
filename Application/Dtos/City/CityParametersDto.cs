namespace Application.Dtos.City
{
    public class CityParametersDto : CityPaginationParameters
    {
        public string Filters { get; set; }
        public string SortOrder { get; set; }
    }
}