namespace EquipmentAPI.ResourceParameters
{
    public class ResourceParameters
    {
        const int MaxPageSize = 16;

        private int _PageSize = 10;

        public int PageSize { get => _PageSize; set => _PageSize = Math.Clamp(value, 1, MaxPageSize); }

        private int _pageNumber = 1;
        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = value < 1 ? 1 : value;
        }

        public string Fields { get; set; } = "";

        private string _SearchQuery = "";
        public string SearchQuery
        {
            get => _SearchQuery;
            set => _SearchQuery = value?.Trim().ToLowerInvariant();
        }

        private string _SortBy = "";

        public string SortBy
        {
            get => _SortBy;
            set => _SortBy = value?.Trim().ToLowerInvariant();
        }
        public bool SortDescending { get; set; } = false;
    }
}
