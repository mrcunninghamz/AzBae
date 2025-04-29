using AutoMapper;
using AzBae.Core.Configuration;
using CLI.Commands;

namespace CLI.Profiles;

public class CosmosProfile : Profile
{
    public CosmosProfile()
    {
        CreateMap<CosmosCommandSettings, CosmosDeleteCommand.Settings>();
        CreateMap<CosmosCommandSettings, CosmosCreateContainerCommand.Settings>();

        CreateMap<CosmosAppSettings, CosmosCommandSettings>()
            .ForMember(dest => dest.AccountEndpoint, opt => opt.MapFrom((src, dest) => CheckAppSettings(dest.AccountEndpoint, src.AccountEndpoint)))
            .ForMember(dest => dest.DatabaseName, opt => opt.MapFrom((src, dest) => CheckAppSettings(dest.DatabaseName, src.DatabaseName)))
            .ForMember(dest => dest.ContainerName, opt => opt.MapFrom((src, dest) => CheckAppSettings(dest.ContainerName, src.ContainerName)))
            .ForMember(dest => dest.PartitionKey, opt => opt.MapFrom((src, dest) => CheckAppSettings(dest.PartitionKey, src.PartitionKey)));

        CreateMap<CosmosAppSettings, CosmosCreateContainerCommand.Settings>()
            .IncludeBase<CosmosAppSettings, CosmosCommandSettings>()
            .ForMember(dest => dest.AccountKey, opt => opt.MapFrom((src, dest) => CheckAppSettings(dest.AccountKey, src.AccountKey)));
    }


    /// <summary>
    /// This prioritzies command setting over appSetting
    /// </summary>
    /// <param name="commandSetting"></param>
    /// <param name="appSetting"></param>
    private static string CheckAppSettings(string? commandSetting, string appSetting)
    {
        if (string.IsNullOrEmpty(commandSetting))
        {
            return appSetting;
        }
        
        return commandSetting;
    }
}
