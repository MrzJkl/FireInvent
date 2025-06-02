using AutoMapper;
using FlameGuardLaundry.Database.Models;
using FlameGuardLaundry.Shared.Models;

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
        }
    }
}
