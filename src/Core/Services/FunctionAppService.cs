using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using System.Web;
using AutoMapper;
using AzBae.Core.Configuration;
using AzBae.Core.Models.ARM;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.AppService;
using Azure.ResourceManager.ResourceGraph;
using Azure.ResourceManager.ResourceGraph.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AzBae.Core.Services
{
    public interface IFunctionAppService
    {
        /// <summary>
        ///  Gets function apps with the option of filtering by access, regex
        /// </summary>
        /// <param name="checkForAccess"></param>
        /// <returns></returns>
        Task<IEnumerable<FunctionApp>> GetFunctionAppsAsync(bool checkForAccess = false);
        Task<string> GetAppInsightsUrl(string functionAppId, string name);
    }
    public class FunctionAppService : IFunctionAppService
    {
        private readonly ILogger<FunctionAppService> _logger;
        private readonly IMapper _mapper;
        private readonly ResourceFilterSettings _filterSettings;
        private readonly ArmClient _armClient;
        private readonly AzureCliCredential _credential;

        public FunctionAppService(ILogger<FunctionAppService> logger, IMapper mapper, IOptions<ResourceFilterSettings> filterSettings, ArmClient client, AzureCliCredential credential)
        {
            _logger = logger;
            _mapper = mapper;
            _filterSettings = filterSettings.Value;
            _armClient = client;
            _credential = credential;
        }
        
        public async Task<string> GetAppInsightsUrl(string functionAppId, string name)
        {
            var instrumentationKey = await GetInstrumentationKey(functionAppId);

            if (string.IsNullOrEmpty(instrumentationKey))
            {
                return string.Empty;
            }
            
            var tenant = _armClient.GetTenants().First();
            var queryContent = new ResourceQueryContent(query: "resources" +
                                                               $"| where type == \"microsoft.insights/components\" and properties.InstrumentationKey == \"{instrumentationKey}\"");
            var response = await tenant.GetResourcesAsync(queryContent)!;
            
            // Deserialize the BinaryData into JSON elements
            var resourceData = response.Value.Data.ToObjectFromJson<JsonElement[]>();
            
            var id = resourceData?[0].GetProperty("id").GetString() ?? string.Empty;
            var table = "{\"tables\":[\"availabilityResults\",\"requests\",\"exceptions\",\"pageViews\",\"traces\",\"customEvents\",\"dependencies\"]," +
                        "\"timeContextWhereClause\":\"| where timestamp > datetime(\\\"" + DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'") + "\\\") and timestamp < datetime(\\\"" + DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'") + "\\\")\"," +
                        "\"filterWhereClause\":\"| where not(operation_Name in (\\\"HealthCheck\\\"))| where cloud_RoleName in (\\\"" + name + "\\\")" +
                        "| order by timestamp desc\",\"originalParams\":{\"eventTypes\":[{\"value\":\"request\",\"tableName\":\"requests\",\"label\":\"Request\"},{\"value\":\"availabilityResult\",\"tableName\":\"availabilityResults\",\"label\":\"Availability\"},{\"value\":\"customEvent\",\"tableName\":\"customEvents\",\"label\":\"Custom Event\"},{\"value\":\"dependency\",\"tableName\":\"dependencies\",\"label\":\"Dependency\"},{\"value\":\"exception\",\"tableName\":\"exceptions\",\"label\":\"Exception\"},{\"value\":\"pageView\",\"tableName\":\"pageViews\",\"label\":\"Page View\"},{\"value\":\"trace\",\"tableName\":\"traces\",\"label\":\"Trace\"}],\"timeContext\":{\"durationMs\":86400000},\"filter\":[{\"dimension\":{\"displayName\":\"Operation+name\",\"tables\":[\"availabilityResults\",\"requests\",\"exceptions\",\"pageViews\",\"traces\",\"customEvents\",\"dependencies\"],\"name\":\"operation/name\",\"draftKey\":\"operation/name\"},\"values\":[\"HealthCheck\"],\"operator\":{\"label\":\"!=\",\"value\":\"!==\"}},{\"dimension\":{\"displayName\":\"Cloud role name\",\"tables\":[\"availabilityResults\",\"requests\",\"exceptions\",\"pageViews\",\"traces\",\"customEvents\",\"dependencies\"],\"name\":\"cloud/roleName\",\"draftKey\":\"cloud/roleName\"}," +
                        "\"values\":[\"" + name + "\"],\"operator\":{\"label\":\"=\",\"value\":\"==\",\"isSelected\":true}}],\"searchPhrase\":{\"originalPhrase\":\"\",\"_tokens\":[]},\"sort\":\"desc\"}}";

  
            return $"https://portal.azure.com/#blade/AppInsightsExtension/BladeRedirect/BladeName/searchV1/ResourceId/{HttpUtility.UrlEncode(HttpUtility.UrlEncode(id))}/BladeInputs/{HttpUtility.UrlEncode(table)}";
        }

        public async Task<IEnumerable<FunctionApp>> GetFunctionAppsAsync(bool checkForAccess = false)
        {
            var filterRegex = _filterSettings.FunctionAppFilterPattern;
            var where = _filterSettings.FunctionAppWhere;
            
            var functionApps = new List<FunctionApp>();

            var tenant = _armClient.GetTenants().First();
            var filterRegexQuery = string.Empty;
            if (!string.IsNullOrEmpty(filterRegex))
            {
                filterRegexQuery = $"| where resourceGroup matches regex \"{filterRegex}\" or name matches regex \"{filterRegex}\"";
            }
            var queryContent = new ResourceQueryContent(query: "Resources " +
                                                               "| where type =~ 'Microsoft.Web/sites' and kind contains 'functionapp' " +
                                                               $"{filterRegexQuery}" +
                                                               $"| where {where}"+
                                                               "| project id, name, tenantId, location, resourceGroup, state = properties.state");
            var response = await tenant.GetResourcesAsync(queryContent)!;
            
            // Deserialize the BinaryData into JSON elements
            var resourceData = response.Value.Data.ToObjectFromJson<JsonElement[]>();


            var authorizationData = new List<dynamic>();
            if (checkForAccess)
            {
                
                var principalId = await GetPrincipalId();
                var authorizationQueryContentContent = new ResourceQueryContent(query: "authorizationresources" +
                    " | where type =~ 'Microsoft.Authorization/RoleAssignments'" +
                    $"| where tostring(properties['principalId']) == \"{principalId}\"" +
                    "| project scope = tostring(properties['scope']), definitionId = properties['roleDefinitionId']");
                var autorizationResponse = await tenant.GetResourcesAsync(authorizationQueryContentContent)!;
                
                authorizationData.AddRange(autorizationResponse.Value.Data.ToObjectFromJson<JsonElement[]>()!.Select(x => new { Scope = x.GetProperty("scope").GetString(), DefinitionId = x.GetProperty("definitionId").GetString()}).ToList());

            }
            
            foreach (var jsonElement in resourceData)
            {
                var functionApp = new FunctionApp
                {
                    Id = jsonElement.GetProperty("id").GetString(),
                    Name = jsonElement.GetProperty("name").GetString(),
                    Location = jsonElement.GetProperty("location").GetString(),
                    ResourceGroup = jsonElement.GetProperty("resourceGroup").GetString(),
                    TenantId = jsonElement.GetProperty("tenantId").GetString(),
                    State = jsonElement.GetProperty("state").GetString()
                };
                
                if (checkForAccess)
                {
                    if (authorizationData.Any(x => functionApp.Id!.Contains(x.Scope!)))
                    {
                        functionApps.Add(functionApp);   
                    }
                }
                else
                {
                    functionApps.Add(functionApp);
                }
            }
            
            return functionApps;
        }

        private async Task<string> GetInstrumentationKey(string functionAppId)
        {
            var resourceId = new ResourceIdentifier(functionAppId);
            var function =  _armClient.GetWebSiteResource(resourceId);
            var instrumentationKey = string.Empty;
            try
            {
                var appSettingResponse = await function.GetApplicationSettingsAsync();
                var setting = appSettingResponse.Value.Properties["APPINSIGHTS_INSTRUMENTATIONKEY"];
                if (setting != null)
                {
                    instrumentationKey = setting;
                }
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 404)
            {
                _logger.LogInformation("App Insights instrumentation key not found for function app {FunctionAppId}", functionAppId);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get instrumentation key for function app {FunctionAppId}: {ErrorMessage}", 
                    functionAppId, e.Message);
            }


            if (!string.IsNullOrEmpty(instrumentationKey))
            {
                return instrumentationKey;
            }
            
            try
            {
                var appSettingResponse = await function.GetApplicationSettingsAsync();
                var setting = appSettingResponse.Value.Properties["APPLICATIONINSIGHTS_CONNECTION_STRING"];
                if (setting != null)
                {
                    instrumentationKey = ExtractInstrumentationKey(setting);
                }
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 404)
            {
                _logger.LogInformation("App Insights instrumentation key not found for function app {FunctionAppId}", functionAppId);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get instrumentation key for function app {FunctionAppId}: {ErrorMessage}", 
                    functionAppId, e.Message);
            }

            return instrumentationKey;
        }

        private async Task<string> GetPrincipalId()
        {
            var scopes = new[] { "https://graph.microsoft.com/.default" };
            var token = await _credential.GetTokenAsync(new Azure.Core.TokenRequestContext(scopes));

            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token.Token) as JwtSecurityToken;
            var upn = jsonToken.Claims.First(c => c.Type == "oid").Value;

            return upn;
        }
        
        private static string ExtractInstrumentationKey(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                return string.Empty;
        
            // Pattern to match InstrumentationKey=value; in the connection string
            var match = System.Text.RegularExpressions.Regex.Match(
                connectionString, 
                @"InstrumentationKey=([^;]+)"
            );
    
            if (match.Success && match.Groups.Count > 1)
            {
                return match.Groups[1].Value; // Return the captured value
            }
    
            return string.Empty;
        }
    }
}
