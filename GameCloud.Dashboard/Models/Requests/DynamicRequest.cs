namespace GameCloud.Dashboard.Models.Requests;

public record DynamicRequest(
    IEnumerable<Sort>? Sort,
    Filter? Filter,
    int PageIndex = 0,
    int PageSize = 10);

public record Sort(string Field, string Dir);
public record Filter(string Field, string Operator, string? Value = null ,string? Logic= null, IEnumerable<Filter>? Filters = null);