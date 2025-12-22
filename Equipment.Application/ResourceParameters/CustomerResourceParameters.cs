namespace Application.ResourceParameters
{
    public class CustomerResourceParameters : ResourceParameters
    {
        private string _FilteredByCountry = "";
        private string _FilteredByCity = "";

        public string FilterByCountry
        {
            get => _FilteredByCountry;
            set => _FilteredByCountry = value.Trim().ToLowerInvariant();
        }

        public string FilterByCity
        {
            get => _FilteredByCity;
            set => _FilteredByCity = value.Trim().ToLowerInvariant();
        }
    }
}
