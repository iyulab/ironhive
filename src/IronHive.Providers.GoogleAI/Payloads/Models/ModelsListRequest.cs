namespace IronHive.Providers.GoogleAI.Payloads.Models;

internal class ModelsListRequest
{
    /// <summary>
    /// how many items to return per page. Defaults to 20. Ranges from 1 to 1000.
    /// </summary>
    public int? PageSize { get; set; }

    /// <summary>
    /// pagination token, typically from a previous call.
    /// </summary>
    public string? PageToken { get; set; }
}