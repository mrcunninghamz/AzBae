using AutoMapper;
using FunctionApp = GUI.Models.FunctionApps.FunctionApp;

namespace AzBae.GUI.Profiles
{
    public class FunctionAppProfile : Profile
    {
        public FunctionAppProfile()
        {
            CreateMap<Core.Models.ARM.FunctionApp, FunctionApp>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location))
                .ForMember(dest => dest.ResourceGroup, opt => opt.MapFrom(src => src.ResourceGroup))
                .ForMember(dest => dest.Uri, opt => opt.MapFrom(src => src.DefaultHostNameWithHttp))
                .ForMember(dest => dest.PortalUri, opt => opt.MapFrom(src => src.PortalUri))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.State));
        }
    }
}
