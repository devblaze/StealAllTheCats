namespace StealAllTheCats.Models.Requets
{
    public class GetCatsRequest
    {
        public int Id { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Tag { get; set; }
    }
}