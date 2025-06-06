using AutoMapper;
using RegistryApi.Models;
using RegistryApi.DTOs;

namespace RegistryApi.Mappings;

public class SpecificationProfile : Profile
{
    public SpecificationProfile()
    {
        // SpecificationIdentifyingInformation Mappings
        CreateMap<SpecificationIdentifyingInformationCreateDto, SpecificationIdentifyingInformation>()
            .ForMember(dest => dest.SpecificationType, opt => opt.MapFrom(src => src.SpecificationType))
            .ForMember(dest => dest.ConformanceLevel, opt => opt.MapFrom(src => src.ConformanceLevel)); // Added mapping for ConformanceLevel
        CreateMap<SpecificationIdentifyingInformationUpdateDto, SpecificationIdentifyingInformation>()
            .ForMember(dest => dest.SpecificationType, opt => opt.MapFrom(src => src.SpecificationType))
            .ForMember(dest => dest.ConformanceLevel, opt => opt.MapFrom(src => src.ConformanceLevel)); // Added mapping for ConformanceLevel

        // Ensure CreatedDate and ModifiedDate are mapped to the DTOs
        CreateMap<SpecificationIdentifyingInformation, SpecificationIdentifyingInformationHeaderDto>()
            .ForMember(dest => dest.SpecificationType, opt => opt.MapFrom(src => src.SpecificationType))
            .ForMember(dest => dest.ConformanceLevel, opt => opt.MapFrom(src => src.ConformanceLevel)) // Added mapping for ConformanceLevel
            .ForMember(dest => dest.Purpose, opt => opt.MapFrom(src => src.Purpose)) // Added mapping for Purpose
            .ForMember(dest => dest.PreferredSyntax, opt => opt.MapFrom(src => src.PreferredSyntax)) // Added mapping for PreferredSyntax
            .ForMember(dest => dest.GoverningEntity, opt => opt.MapFrom(src => src.GoverningEntity)); // Added mapping for GoverningEntity
        CreateMap<SpecificationIdentifyingInformation, SpecificationIdentifyingInformationDetailDto>()
             .ForMember(dest => dest.SpecificationCores, opt => opt.Ignore())
             .ForMember(dest => dest.SpecificationExtensionComponents, opt => opt.Ignore())
             .ForMember(dest => dest.SpecificationType, opt => opt.MapFrom(src => src.SpecificationType))
             .ForMember(dest => dest.ConformanceLevel, opt => opt.MapFrom(src => src.ConformanceLevel)); // Added mapping for ConformanceLevel

        // SpecificationCore Mappings
        CreateMap<SpecificationCoreCreateDto, SpecificationCore>();
        CreateMap<SpecificationCoreUpdateDto, SpecificationCore>();
        CreateMap<SpecificationCore, SpecificationCoreDto>()
            .ForMember(dest => dest.CoreBusinessTerm, opt => opt.MapFrom(src => src.CoreInvoiceModel.BusinessTerm))
            .ForMember(dest => dest.CoreLevel, opt => opt.MapFrom(src => src.CoreInvoiceModel.Level)) // Added mapping for CoreLevel
            .ForMember(dest => dest.CoreBusinessRules, opt => opt.MapFrom(src => src.CoreInvoiceModel.BusinessRules)) // Added mapping for CoreBusinessRules
            .ForMember(dest => dest.CoreDataType, opt => opt.MapFrom(src => src.CoreInvoiceModel.DataType)) // Added mapping for CoreDataType
            .ForMember(dest => dest.CoreSemanticDescription, opt => opt.MapFrom(src => src.CoreInvoiceModel.SemanticDescription))
            .ForMember(dest => dest.CoreParentID, opt => opt.MapFrom(src => src.CoreInvoiceModel.ParentID)); // NEW: Added mapping for CoreParentID

        // CoreInvoiceModel Mapping
        CreateMap<CoreInvoiceModel, CoreInvoiceModelDto>();

        // Extension Model Mappings
        CreateMap<ExtensionComponentsModelHeader, ExtensionComponentsModelHeaderDto>();
        CreateMap<ExtensionComponentModelElement, ExtensionComponentModelElementDto>();

        CreateMap<SpecificationExtensionComponentCreateDto, SpecificationExtensionComponent>();
        CreateMap<SpecificationExtensionComponentUpdateDto, SpecificationExtensionComponent>();
        CreateMap<SpecificationExtensionComponent, SpecificationExtensionComponentDto>()
            .ForMember(dest => dest.ExtLevel, opt => opt.MapFrom(src => src.ExtensionComponentModelElement.Level))
            .ForMember(dest => dest.ExtBusinessTerm, opt => opt.MapFrom(src => src.ExtensionComponentModelElement.BusinessTerm))
            .ForMember(dest => dest.ExtDataType, opt => opt.MapFrom(src => src.ExtensionComponentModelElement.DataType))
            .ForMember(dest => dest.ExtConformanceType, opt => opt.MapFrom(src => src.ExtensionComponentModelElement.ConformanceType))
            .ForMember(dest => dest.ExtParentID, opt => opt.MapFrom(src => src.ExtensionComponentModelElement.ParentID)); // NEW: Added mapping for ExtParentID

        // User Mappings
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.GroupName, opt => opt.MapFrom(src => src.UserGroup != null ? src.UserGroup.GroupName : null));

        CreateMap<UserCreateDto, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

        CreateMap<UserUpdateDto, User>()
            .ForMember(u => u.UserID, opt => opt.Ignore())
            .ForMember(u => u.Username, opt => opt.Ignore())
            .ForMember(u => u.PasswordHash, opt => opt.Ignore())
            .ForMember(u => u.CreatedDate, opt => opt.Ignore());

        // UserGroup Mappings
        CreateMap<UserGroup, UserGroupDto>();
        CreateMap<UserGroupCreateDto, UserGroup>()
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow));
        CreateMap<UserGroupUpdateDto, UserGroup>();
    }
}
