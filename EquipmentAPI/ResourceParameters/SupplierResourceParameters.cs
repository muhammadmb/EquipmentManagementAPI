namespace EquipmentAPI.ResourceParameters
{
    public class SupplierResourceParameters : ResourceParameters
    {
        private string _FilterByCountry = "";
        private string _FilterByCity = "";

        public string FilterByCountry
        {
            get => _FilterByCountry;
            set => _FilterByCountry = value?.Trim().ToLowerInvariant();
        }

        public string FilterByCity
        {
            get => _FilterByCity;
            set => _FilterByCity = value?.Trim().ToLowerInvariant();
        }
    }
}
