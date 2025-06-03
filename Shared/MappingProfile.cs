using AutoMapper;
using FlameGuardLaundry.Database.Models;
using FlameGuardLaundry.Shared.Models;
using Microsoft.AspNetCore.Identity;

namespace FlameGuardLaundry.Shared
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Department, DepartmentModel>().ReverseMap();
            CreateMap<StorageLocation, StorageLocationModel>().ReverseMap();
            CreateMap<ClothingProduct, ClothingProductModel>().ReverseMap();
            CreateMap<ClothingItemAssignmentHistory, ClothingItemAssignmentHistoryModel>().ReverseMap();
            CreateMap<ClothingItem, ClothingItemModel>().ReverseMap();
            CreateMap<Maintenance, MaintenanceModel>().ReverseMap();
            CreateMap<ClothingVariant, ClothingVariantModel>().ReverseMap();
            CreateMap<Person, PersonModel>().ReverseMap();
            CreateMap<IdentityUser, UserModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ReverseMap();
        }
    }
}
