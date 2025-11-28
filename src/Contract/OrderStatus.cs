using System.Text.Json.Serialization;

namespace FireInvent.Contract;

[JsonConverter(typeof(JsonStringEnumConverter<OrderStatus>))]
public enum OrderStatus
{
    Draft,
    Submitted,
    Delivered,
    Completed,
}
