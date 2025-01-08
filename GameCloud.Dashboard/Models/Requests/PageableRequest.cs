namespace GameCloud.Dashboard.Models.Requests;

public record PageableRequest(
    int PageIndex = 0,
    int PageSize = 10);