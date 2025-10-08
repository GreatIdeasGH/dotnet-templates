namespace Company.Project.Application.Common.Params;

public record DonationPagingParameters : PagingParameters
{
    public string? Campaign { get; set; }
    public string? DonationType { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? Amount { get; set; }
    public string? Donor { get; set; }
}
