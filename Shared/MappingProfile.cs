using AutoMapper;
using FireInvent.Database.Models;
using FireInvent.Shared.Models;

namespace FireInvent.Shared;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CreateDepartmentModel, Department>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()));
        CreateMap<CreateStorageLocationModel, StorageLocation>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()));
        CreateMap<CreateClothingProductModel, ClothingProduct>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()));
        CreateMap<CreateClothingItemModel, ClothingItem>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()));
        CreateMap<CreateClothingVariantModel, ClothingVariant>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()));
        CreateMap<CreateClothingItemAssignmentHistoryModel, ClothingItemAssignmentHistory>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()));
        CreateMap<CreateMaintenanceModel, Maintenance>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()));
        CreateMap<CreatePersonModel, Person>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()));
        CreateMap<CreateOrderItemModel, OrderItem>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()));
        CreateMap<CreateOrderModel, Order>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()));

        CreateMap<Department, DepartmentModel>().ReverseMap();
        CreateMap<StorageLocation, StorageLocationModel>().ReverseMap();
        CreateMap<ClothingProduct, ClothingProductModel>().ReverseMap();
        CreateMap<ClothingItemAssignmentHistory, ClothingItemAssignmentHistoryModel>().ReverseMap();
        CreateMap<ClothingItem, ClothingItemModel>().ReverseMap();
        CreateMap<Maintenance, MaintenanceModel>().ReverseMap();
        CreateMap<ClothingVariant, ClothingVariantModel>().ReverseMap();
        CreateMap<Person, PersonModel>().ReverseMap();
        CreateMap<Order, OrderModel>().ReverseMap();
        CreateMap<OrderItem, OrderItemModel>().ReverseMap();
        CreateMap<User, UserModel>()
            .ReverseMap();
    }
}
