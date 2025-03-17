namespace EquipmentAPI.ResourceParameters
{
    public class EquipmentResourceParameters : ResourceParameters
    {
        private string _SortBy = "name";

        public new string SortBy
        {
            get => _SortBy;
            set => _SortBy = value?.Trim().ToLowerInvariant();
        }
    }
}
