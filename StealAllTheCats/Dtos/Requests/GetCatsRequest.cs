using System.ComponentModel.DataAnnotations;

namespace StealAllTheCats.Dtos.Requests;

public class GetCatsRequest
{
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 10;

    public string? Tag { get; set; }
}
