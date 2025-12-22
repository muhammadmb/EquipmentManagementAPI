namespace Application.BulkOperations
{
    public class BulkOperationResult
    {
            public int SuccessCount { get; set; }
            public int FailureCount { get; set; }
            public List<Guid> SuccessIds { get; set; } = new();
            public List<BulkOperationError> Errors { get; set; } = new();
            public bool HasErrors => FailureCount > 0;
            public double SuccessRate => (SuccessCount + FailureCount) > 0
                ? (double)SuccessCount / (SuccessCount + FailureCount) * 100
                : 0;
    }
}
