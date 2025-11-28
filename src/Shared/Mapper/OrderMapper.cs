using FireInvent.Database.Models;
using FireInvent.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace FireInvent.Shared.Mapper;

[Mapper]
public partial class OrderMapper : BaseMapper
{
    public partial OrderModel MapOrderToOrderModel(Order order);

    [MapValue(nameof(Order.Id), Use = nameof(NewGuid))]
    public partial Order MapCreateOrUpdateOrderModelToOrder(CreateOrUpdateOrderModel createOrderModel);

    public partial List<OrderModel> MapOrdersToOrderModels(List<Order> orders);

    public partial void MapCreateOrUpdateOrderModelToOrder(CreateOrUpdateOrderModel source, Order target);
}
