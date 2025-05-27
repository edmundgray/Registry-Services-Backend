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
            .ForMember(dest => dest.SpecificationType, opt => opt.MapFrom(src => src.SpecificationType)); // Added mapping for SpecificationType
        CreateMap<SpecificationIdentifyingInformationUpdateDto, SpecificationIdentifyingInformation>()
            .ForMember(dest => dest.SpecificationType, opt => opt.MapFrom(src => src.SpecificationType)); // Added mapping for SpecificationType

        // Ensure CreatedDate and ModifiedDate are mapped to the DTOs
        CreateMap<SpecificationIdentifyingInformation, SpecificationIdentifyingInformationHeaderDto>()
            .ForMember(dest => dest.SpecificationType, opt => opt.MapFrom(src => src.SpecificationType)); // Added mapping for SpecificationType
        CreateMap<SpecificationIdentifyingInformation, SpecificationIdentifyingInformationDetailDto>()
             .ForMember(dest => dest.SpecificationCores, opt => opt.Ignore())
             .ForMember(dest => dest.SpecificationExtensionComponents, opt => opt.Ignore())
             .ForMember(dest => dest.SpecificationType, opt => opt.MapFrom(src => src.SpecificationType)); // Added mapping for SpecificationType

        // SpecificationCore Mappings
        CreateMap<SpecificationCoreCreateDto, SpecificationCore>();
        CreateMap<SpecificationCoreUpdateDto, SpecificationCore>();
        CreateMap<SpecificationCore, SpecificationCoreDto>()
            .ForMember(dest => dest.CoreBusinessTerm, opt => opt.MapFrom(src => src.CoreInvoiceModel.BusinessTerm))
            .ForMember(dest => dest.CoreLevel, opt => opt.MapFrom(src => src.CoreInvoiceModel.Level))
            .ForMember(dest => dest.CoreSemanticDescription, opt => opt.MapFrom(src => src.CoreInvoiceModel.SemanticDescription));

        // SpecificationExtensionComponent Mappings
        CreateMap<SpecificationExtensionComponentCreateDto, SpecificationExtensionComponent>();
        CreateMap<SpecificationExtensionComponentUpdateDto, SpecificationExtensionComponent>();
        CreateMap<SpecificationExtensionComponent, SpecificationExtensionComponentDto>();

        // New User Mappings
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.GroupName, opt => opt.MapFrom(src => src.UserGroup != null ? src.UserGroup.GroupName : null));

        CreateMap<UserCreateDto, User>()
            // Password hashing should be handled in the service layer before mapping/saving
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow)) // Set creation date on mapping
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true)); // Default to active

        CreateMap<UserUpdateDto, User>()
            .ForMember(u => u.UserID, opt => opt.Ignore()) // Typically ID is not changed on update
            .ForMember(u => u.Username, opt => opt.Ignore()) // Typically username is not changed on update
            .ForMember(u => u.PasswordHash, opt => opt.Ignore()) // Password changes should be a separate, dedicated process
            .ForMember(u => u.CreatedDate, opt => opt.Ignore()); // CreatedDate should not be changed on update

        // New UserGroup Mappings
        CreateMap<UserGroup, UserGroupDto>();
        CreateMap<UserGroupCreateDto, UserGroup>()
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow)); // Set creation date
        CreateMap<UserGroupUpdateDto, UserGroup>();
    }
}
