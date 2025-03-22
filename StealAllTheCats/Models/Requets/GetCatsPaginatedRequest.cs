namespace StealAllTheCats.Models.Requets
{
    public class GetCatsPaginatedRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Tag { get; set; }
    }
}