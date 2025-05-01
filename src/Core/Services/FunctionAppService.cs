using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using AutoMapper;
using AzBae.Core.Configuration;
using AzBae.Core.Models.ARM;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.AppService;
using Azure.ResourceManager.ResourceGraph;
using Azure.ResourceManager.ResourceGraph.Models;
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
    }
    public class FunctionAppService : IFunctionAppService
    {
        private readonly IMapper _mapper;
        private readonly ResourceFilterSettings _filterSettings;
        private readonly ArmClient _armClient;
        private readonly AzureCliCredential _credential;

        public FunctionAppService(IMapper mapper, IOptions<ResourceFilterSettings> filterSettings, ArmClient client, AzureCliCredential credential)
        {
            _mapper = mapper;
            _filterSettings = filterSettings.Value;
            _armClient = client;
            _credential = credential;
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

        private async Task<string> GetPrincipalId()
        {
            var scopes = new[] { "https://graph.microsoft.com/.default" };
            var token = await _credential.GetTokenAsync(new Azure.Core.TokenRequestContext(scopes));

            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token.Token) as JwtSecurityToken;
            var upn = jsonToken.Claims.First(c => c.Type == "oid").Value;

            return upn;
        }
    }
}
