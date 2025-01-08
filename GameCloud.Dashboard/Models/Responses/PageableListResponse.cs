namespace GameCloud.Dashboard.Models.Responses;

public record PageableListResponse<TData>(
    int Index,
    int Size,
    int Count,
    int Pages,
    bool HasPreviousPage,
    bool HasNextPage,
    IList<TData> Items);