using System.ComponentModel;
using System.Text.RegularExpressions;
using AzBae.Core.Models.ARM;
using AzBae.Core.Services;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace AzBae.MCP.Tools;

[McpServerToolType]
public class FunctionsTool
{
    private readonly ILogger<FunctionsTool> _logger;
    private readonly IFunctionAppService _functionAppService;

    public FunctionsTool(ILogger<FunctionsTool> logger, IFunctionAppService functionAppService)
    {
        _logger = logger;
        _functionAppService = functionAppService;
    }
    
    [McpServerTool(Name = "azb_get_function_apps"), Description("Gets the list of Azure Function Apps in the current subscription. " +
        "You can optionally filter the results by providing a filter pattern (e.g., 'myapp*'). " +
        "If no filter is provided, all function apps will be returned.")]
    public async Task<string> GetFunctionAppsAsync(
        [Description("Optional filter pattern to match function app names (e.g., 'myapp*')")] string? filterPattern = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving function apps with filter pattern: {FilterPattern}", filterPattern);
        
        try
        {
            var functionApps = (await _functionAppService.GetFunctionAppsAsync()).ToList();
            
            
            if (!string.IsNullOrWhiteSpace(filterPattern))
            {
                functionApps = ApplyFilter(filterPattern, functionApps);
            }
            
            var stringWriter = new StringWriter();
            await stringWriter.WriteLineAsync("Function Apps:");
            foreach(var app in functionApps)
            {
                await stringWriter.WriteLineAsync($"- {app.Name} -");
                await stringWriter.WriteLineAsync($"  Location: {app.Location}");
                await stringWriter.WriteLineAsync($"  State: {app.State}");
                await stringWriter.WriteLineAsync($"  FunctionApp Id: {app.Id}");
                await stringWriter.WriteLineAsync($"  Resource Group: {app.ResourceGroup}");
                await stringWriter.WriteLineAsync($"  Default Host: {app.DefaultHostNameWithHttp}");
                await stringWriter.WriteLineAsync($"  Portal: {app.PortalUri}");
                await stringWriter.WriteLineAsync();
            }

            return stringWriter.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve function apps");
            throw;
        }
    }
    
    [McpServerTool(Name = "azb_get_app_insights_url"), Description("Gets the Application Insights URL for a specific Function App in order to view logs. " +
        "You need to provide the Function App ID and name to retrieve the Application Insights URL.")]
    public async Task<string> GetAppInsightsUrlAsync(
        [Description("The Id of the Function App")] string functionAppId,
        [Description("Name of the Function App")] string functionAppName)
    {
        _logger.LogInformation("Retrieving Application Insights URL for Function App: {FunctionAppName}", functionAppName);
        
        try
        {
            var appInsightsUrl = await _functionAppService.GetAppInsightsUrl(functionAppId, functionAppName);
            if (string.IsNullOrWhiteSpace(appInsightsUrl))
            {
                return $"No Application Insights URL found for Function App '{functionAppName}'.";
            }
            
            return $"Application Insights URL for '{functionAppName}': {appInsightsUrl}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Application Insights URL for Function App: {FunctionAppName}", functionAppName);
            throw;
        }
    }
    
    private List<FunctionApp> ApplyFilter(string FilterPattern, List<FunctionApp> AllFunctionApps)
    {
        Regex regex;
        var response = new List<FunctionApp>();
        
        try
        {
            if (IsValidRegex(FilterPattern))
            {
                regex = new Regex(FilterPattern, RegexOptions.IgnoreCase);
            }
            else
            {
                return AllFunctionApps;
            }
            
            var filteredList = AllFunctionApps.Where(app => regex.IsMatch(app.Name) ||  regex.IsMatch(app.ResourceGroup)).ToList();
            
            response.AddRange(filteredList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occured in filtering the functions: {Message}", ex.Message);
            throw;
        }

        return response;
    }
    
    private bool IsValidRegex(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            return false;
        
        try
        {
            // Attempt to create a Regex object with the pattern
            Regex.IsMatch("", pattern);
            return true;
        }
        catch (ArgumentException)
        {
            // If ArgumentException is thrown, the pattern is invalid
            return false;
        }
    }
}
