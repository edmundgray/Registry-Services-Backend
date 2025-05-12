using AutoMapper;
using RegistryApi.Models;
using RegistryApi.DTOs;

namespace RegistryApi.Mappings;

public class SpecificationProfile : Profile
{
    public SpecificationProfile()
    {
        // SpecificationIdentifyingInformation Mappings
        CreateMap<SpecificationIdentifyingInformationCreateDto, SpecificationIdentifyingInformation>();
        CreateMap<SpecificationIdentifyingInformationUpdateDto, SpecificationIdentifyingInformation>();
        CreateMap<SpecificationIdentifyingInformation, SpecificationIdentifyingInformationHeaderDto>();
        // For Detail Dto - map only header fields here, children handled in service
        CreateMap<SpecificationIdentifyingInformation, SpecificationIdentifyingInformationDetailDto>()
             .ForMember(dest => dest.SpecificationCores, opt => opt.Ignore()) // Handled separately
             .ForMember(dest => dest.SpecificationExtensionComponents, opt => opt.Ignore()); // Handled separately


        // SpecificationCore Mappings
        CreateMap<SpecificationCoreCreateDto, SpecificationCore>();
        CreateMap<SpecificationCoreUpdateDto, SpecificationCore>();
        CreateMap<SpecificationCore, SpecificationCoreDto>();

        // SpecificationExtensionComponent Mappings
        CreateMap<SpecificationExtensionComponentCreateDto, SpecificationExtensionComponent>();
        CreateMap<SpecificationExtensionComponentUpdateDto, SpecificationExtensionComponent>();
        CreateMap<SpecificationExtensionComponent, SpecificationExtensionComponentDto>();
    }
}
