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

    public partial IQueryable<OrderModel> ProjectOrdersToOrderModels(IQueryable<Order> orders);

    public partial void MapCreateOrUpdateOrderModelToOrder(CreateOrUpdateOrderModel source, Order target);

    // OrderItem mappings
    public partial OrderItemModel MapOrderItemToOrderItemModel(OrderItem orderItem);

    [MapValue(nameof(OrderItem.Id), Use = nameof(NewGuid))]
    public partial OrderItem MapCreateOrUpdateOrderItemModelToOrderItem(CreateOrUpdateOrderItemModel createOrderItemModel);


    public partial List<OrderItemModel> MapOrderItemsToOrderItemModels(List<OrderItem> orderItems);

    public partial IQueryable<OrderItemModel> ProjectOrderItemsToOrderItemModels(IQueryable<OrderItem> orderItems);

    public partial void MapCreateOrUpdateOrderItemModelToOrderItem(CreateOrUpdateOrderItemModel source, OrderItem target);
}
