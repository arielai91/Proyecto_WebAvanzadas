namespace ApiPetFoundation.Soap.Api.Models;

public class AdoptionSummaryResponse
{
    public int Total { get; init; }
    public int Pending { get; init; }
    public int Approved { get; init; }
    public int Rejected { get; init; }
    public int Cancelled { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
}
