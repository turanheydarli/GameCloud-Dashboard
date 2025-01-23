namespace GameCloud.Dashboard.Models.Requests;

public record PageableRequest(
    string? Search = null,
    int PageIndex = 0,
    int PageSize = 10,
    bool IsAscending = true);