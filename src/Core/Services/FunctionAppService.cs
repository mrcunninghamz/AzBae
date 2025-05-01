using System.Text.Json;
using AutoMapper;
using AzBae.Core.Models.ARM;
using Azure.ResourceManager;
using Azure.ResourceManager.AppService;
using Azure.ResourceManager.ResourceGraph;
using Azure.ResourceManager.ResourceGraph.Models;

namespace AzBae.Core.Services
{
    public interface IFunctionAppService
    {
        Task<IEnumerable<FunctionApp>> GetFunctionAppsAsync();
    }
    public class FunctionAppService : IFunctionAppService
    {
        private readonly IMapper _mapper;
        private readonly ArmClient _armClient;

        public FunctionAppService(IMapper mapper, ArmClient client)
        {
            _mapper = mapper;
            _armClient = client;
        }

        public async Task<IEnumerable<FunctionApp>> GetFunctionAppsAsync()
        {
            var subscription = await _armClient.GetDefaultSubscriptionAsync();
            var functionApps = new List<FunctionApp>();

            var tenant = _armClient.GetTenants().First();
            var queryContent = new ResourceQueryContent(query: "Resources | where type =~ 'Microsoft.Web/sites' and kind contains 'functionapp' | project id, name, tenantId, location, resourceGroup");
            var response = await tenant.GetResourcesAsync(queryContent)!;
            
            // Deserialize the BinaryData into JSON elements
            var resourceData = response.Value.Data.ToObjectFromJson<JsonElement[]>();
            
            foreach (var jsonElement in resourceData)
            {
                var functionApp = new FunctionApp
                {
                    Id = jsonElement.GetProperty("id").GetString(),
                    Name = jsonElement.GetProperty("name").GetString(),
                    Location = jsonElement.GetProperty("location").GetString(),
                    ResourceGroup = jsonElement.GetProperty("resourceGroup").GetString(),
                    TenantId = jsonElement.GetProperty("tenantId").GetString(),
                };
                functionApps.Add(functionApp);
            }
            
            return functionApps;
        }
    }
}
