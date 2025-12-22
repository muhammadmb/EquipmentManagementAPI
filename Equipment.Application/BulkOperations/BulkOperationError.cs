namespace Application.BulkOperations
{
    public class BulkOperationError
    {
        public Guid EntityId { get; set; }
        public string ErrorMessage { get; set; } 
        public string? StackTrace { get; set; }
    }
}
