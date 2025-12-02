namespace EquipmentAPI.Helper.BulkOperations
{
    public class BulkOperationResponse
    {
        public int TotalRequested { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public double SuccessRate { get; set; }
        public bool HasErrors => FailureCount > 0;
        public List<Guid> SuccessfulIds { get; set; } = new();
        public List<BulkOperationError> Errors { get; set; } = new();
        public string Message { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }
}
