using FireInvent.Database.Models;
using FireInvent.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace FireInvent.Shared.Mapper;

[Mapper]
public partial class ItemMapper : BaseMapper
{
    public partial ItemModel MapItemToItemModel(Item item);

    [MapValue(nameof(Item.Id), Use = nameof(NewGuid))]
    public partial Item MapCreateOrUpdateItemModelToItem(CreateOrUpdateItemModel createItemModel);

    public partial List<ItemModel> MapItemsToItemModels(List<Item> items);

    public partial void MapCreateOrUpdateItemModelToItem(CreateOrUpdateItemModel source, Item target);
}
