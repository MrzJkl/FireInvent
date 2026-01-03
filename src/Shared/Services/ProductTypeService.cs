using FireInvent.Database;
using FireInvent.Database.Extensions;
using FireInvent.Contract;
using FireInvent.Contract.Exceptions;
using FireInvent.Shared.Extensions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Services
{
    public class ProductTypeService(AppDbContext context, ProductTypeMapper mapper) : IProductTypeService
    {
        public async Task<ProductTypeModel> CreateProductTypeAsync(CreateOrUpdateProductTypeModel model)
        {
            var exists = await context.ProductTypes
                .AnyAsync(p => p.Name == model.Name);

            if (exists)
                throw new ConflictException("A productType with the same name already exists.");

            var productType = mapper.MapCreateOrUpdateProductTypeModelToProductType(model);

            await context.ProductTypes.AddAsync(productType);
            await context.SaveChangesAsync();

            return mapper.MapProductTypeToProductTypeModel(productType);
        }

        public async Task<PagedResult<ProductTypeModel>> GetAllProductTypesAsync(PagedQuery pagedQuery, CancellationToken cancellationToken)
        {
            var query = context.ProductTypes
                .OrderBy(pt => pt.Name)
                .AsNoTracking();

            query = query.ApplySearch(pagedQuery.SearchTerm);

            var projected = mapper.ProjectProductTypesToProductTypeModels(query);

            return await projected.ToPagedResultAsync(
                pagedQuery.Page,
                pagedQuery.PageSize,
                cancellationToken);
        }

        public async Task<ProductTypeModel?> GetProductTypeByIdAsync(Guid id)
        {
            var productType = await context.ProductTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            return productType is null ? null : mapper.MapProductTypeToProductTypeModel(productType);
        }

        public async Task<bool> UpdateProductTypeAsync(Guid id, CreateOrUpdateProductTypeModel model)
        {
            var productType = await context.ProductTypes.FindAsync(id);
            if (productType is null)
                return false;

            var nameExists = await context.ProductTypes.AnyAsync(p =>
                p.Id != id && p.Name == model.Name);

            if (nameExists)
                throw new ConflictException("Another productType with the same name already exists.");

            mapper.MapCreateOrUpdateProductTypeModelToProductType(model, productType);

            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteProductTypeAsync(Guid id)
        {
            var productType = await context.ProductTypes.FindAsync(id);
            if (productType is null)
                return false;

            context.ProductTypes.Remove(productType);
            await context.SaveChangesAsync();
            return true;
        }
    }
}
