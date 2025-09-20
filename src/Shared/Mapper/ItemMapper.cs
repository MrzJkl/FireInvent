using FireInvent.Database.Models;
using FireInvent.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace FireInvent.Shared.Mapper;

[Mapper]
public partial class ItemMapper : BaseMapper
{
    public partial ItemModel MapItemToItemModel(Item item);

    [MapValue(nameof(Item.Id), Use = nameof(NewGuid))]
    public partial Item MapCreateItemModelToItem(CreateItemModel createItemModel);

    public partial List<ItemModel> MapItemsToItemModels(List<Item> items);

    [MapperIgnoreTarget(nameof(Item.Id))]
    public partial void MapItemModelToItem(ItemModel source, Item target);
}
