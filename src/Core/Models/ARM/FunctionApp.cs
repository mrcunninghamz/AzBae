namespace AzBae.Core.Models.ARM
{
    public class FunctionApp
    {
        public string Id { get; set; }
        
        public string? TenantId { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string ResourceGroup { get; set; }
        
        public string DefaultHostNameWithHttp => $"http://{Name}.azurewebsites.net";
        public string PortalUri => $"https://portal.azure.com/#@{TenantId}/resource/{Id}";
    }
}
