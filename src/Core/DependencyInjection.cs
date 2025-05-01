using AzBae.Core.Services;
using Azure.Identity;
using Azure.ResourceManager;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;

namespace AzBae.Core
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCore(this IServiceCollection services)
        {
            services.AddAzureClients(builder =>
            {
                builder.AddArmClient("349072a3-66bb-4f8f-b5c9-de28a6562170"); //TODO put this in config
                builder.UseCredential(new AzureCliCredential());
            });
            services.AddSingleton<IFunctionAppService, FunctionAppService>();
            services.AddAutoMapper(typeof(DependencyInjection).Assembly);
            return services;
        }
    }
}
