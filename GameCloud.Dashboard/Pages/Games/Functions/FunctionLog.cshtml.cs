using GameCloud.Dashboard.Abstractions;
using GameCloud.Dashboard.Models.Requests;
using GameCloud.Dashboard.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GameCloud.Dashboard.Pages.Games.Functions;

public class FunctionLogsModel(IGameClient gameClient) : PageModel
{
    [BindProperty(SupportsGet = true)] public string TimeRange { get; set; } = "24h";

    [BindProperty(SupportsGet = true)] public string DateRange { get; set; }

    [BindProperty(SupportsGet = true)] public string Status { get; set; }

    [BindProperty(SupportsGet = true)] public string Search { get; set; }

    [BindProperty(SupportsGet = true)] public int Page { get; set; } = 0;

    [BindProperty(SupportsGet = true)] public int PageSize { get; set; } = 20;

    public GameResponse Game { get; set; }
    public FunctionResponse Function { get; set; }
    public ActionStatsResponse Stats { get; set; }
    public PageableListResponse<ActionResponse> Logs { get; set; }

    [BindProperty(SupportsGet = true)]
    [FromRoute(Name = "gameId")]
    public Guid GameId { get; set; }

    [BindProperty(SupportsGet = true)]
    [FromRoute(Name = "functionId")]
    public Guid FunctionId { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var getGameTask = gameClient.GetAsync(GameId);
        var getFunctionTask = gameClient.GetFunctionAsync(GameId, FunctionId);
        var getLogsTask = gameClient.GetTestedFunctionLogsAsync(GameId, FunctionId, new PageableRequest()
        {
            PageSize = PageSize,
            PageIndex = Page,
        });

        DateTime? startDate = null;
        DateTime? endDate = null;

        if (TimeRange == "custom" && !string.IsNullOrEmpty(DateRange))
        {
            var dates = DateRange.Split(" - ");
            if (dates.Length == 2)
            {
                startDate = DateTime.Parse(dates[0]);
                endDate = DateTime.Parse(dates[1]);
            }
        }
        else
        {
            endDate = DateTime.UtcNow;
            startDate = TimeRange switch
            {
                "1h" => endDate.Value.AddHours(-1),
                "24h" => endDate.Value.AddDays(-1),
                "7d" => endDate.Value.AddDays(-7),
                "30d" => endDate.Value.AddDays(-30),
                _ => endDate.Value.AddDays(-1) // Default to 24h
            };
        }

        var getStatsTask = gameClient.GetFunctionStatsAsync(GameId, FunctionId);

        await Task.WhenAll(getGameTask, getFunctionTask, getStatsTask, getLogsTask);

        Game = await getGameTask;
        Function = await getFunctionTask;
        Stats = await getStatsTask;
        Logs = await getLogsTask;

        var logsRequest = new GetFunctionLogsRequest
        {
            StartDate = startDate,
            EndDate = endDate,
            Status = Status,
            Search = Search,
            Page = Page,
            PageSize = PageSize
        };

        // Logs = await _logsService.GetFunctionLogsAsync(GameId, FunctionId, logsRequest);
        // LogStats = await _logsService.GetFunctionLogStatsAsync(GameId, FunctionId, logsRequest);

        return Page();
    }

    public string GetPageUrl(int pageIndex)
    {
        var queryParams = new Dictionary<string, string>
        {
            { "timeRange", TimeRange },
            { "status", Status ?? "" },
            { "search", Search ?? "" },
            { "page", pageIndex.ToString() }
        };

        if (TimeRange == "custom")
        {
            queryParams.Add("dateRange", DateRange ?? "");
        }

        var queryString = string.Join("&", queryParams
            .Where(p => !string.IsNullOrEmpty(p.Value))
            .Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));

        return $"/game/{GameId}/functions/{FunctionId}/logs?{queryString}";
    }
}

public class GetFunctionLogsRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; }
    public string Search { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class FunctionLogResponse
{
    public Guid Id { get; set; }
    public string SessionId { get; set; }
    public string UserName { get; set; }
    public string IpAddress { get; set; }
    public DateTime ExecutedAt { get; set; }
    public int ExecutionTimeMs { get; set; }
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; }
    public object Payload { get; set; }
    public object Result { get; set; }
}

public class FunctionLogStatsResponse
{
    public int TotalExecutions { get; set; }
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public TimeSpan AvgResponseTime { get; set; }
}