using AutoMapper;
using Catalog.Bll.Dtos;
using Catalog.Domain.Entities;

namespace Catalog.Bll.Mapping;

public class CatalogMappingProfile : Profile
{
    public CatalogMappingProfile()
    {
        CreateMap<Product, ProductDto>()
            .ForMember(d => d.Detail, opt => opt.MapFrom(s => s.Detail))
            .ForMember(d => d.Images, opt => opt.MapFrom(s => s.Images.OrderBy(i => i.DisplayOrder)));

        CreateMap<ProductDetail, ProductDetailDto>();

        CreateMap<ProductImage, ProductImageDto>();

        CreateMap<Category, CategoryDto>()
            .ForMember(d => d.SubCategories, opt => opt.MapFrom(s => s.SubCategories.Where(sc => !sc.IsDeleted)));

        CreateMap<Store, StoreDto>();

        CreateMap<StoreEmployee, StoreEmployeeDto>()
            .ForMember(d => d.UserFullName, opt => opt.MapFrom(s => s.User != null ? s.User.FullName : null))
            .ForMember(d => d.UserEmail, opt => opt.MapFrom(s => s.User != null ? s.User.Email : null));

        CreateMap<User, UserDto>();
    }
}
