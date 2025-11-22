using FireInvent.Database.Models;
using FireInvent.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace FireInvent.Shared.Mapper;

[Mapper]
public partial class ItemMapper : BaseMapper
{
    [MapperIgnoreSource(nameof(Item.Variant))]
    [MapperIgnoreSource(nameof(Item.StorageLocation))]
    [MapperIgnoreSource(nameof(Item.Assignments))]
    [MapperIgnoreSource(nameof(Item.Maintenances))]
    public partial ItemModel MapItemToItemModel(Item item);

    [MapValue(nameof(Item.Id), Use = nameof(NewGuid))]
    [MapperIgnoreTarget(nameof(Item.Variant))]
    [MapperIgnoreTarget(nameof(Item.StorageLocation))]
    [MapperIgnoreTarget(nameof(Item.Assignments))]
    [MapperIgnoreTarget(nameof(Item.Maintenances))]
    public partial Item MapCreateOrUpdateItemModelToItem(CreateOrUpdateItemModel createItemModel);

    public partial List<ItemModel> MapItemsToItemModels(List<Item> items);

    [MapperIgnoreTarget(nameof(Item.Id))]
    [MapperIgnoreTarget(nameof(Item.Variant))]
    [MapperIgnoreTarget(nameof(Item.StorageLocation))]
    [MapperIgnoreTarget(nameof(Item.Assignments))]
    [MapperIgnoreTarget(nameof(Item.Maintenances))]
    public partial void MapCreateOrUpdateItemModelToItem(CreateOrUpdateItemModel source, Item target);
}
