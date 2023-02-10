using AsyncProductApi.Models;

namespace AsyncProductApi.Dtos;

public class ListingStatus
{
    public string? RequestStatus { get; set; }

    public DateTime? EstimatedCompletionTime { get; set; }

    public string? ResourceUrl { get; set; }
}
