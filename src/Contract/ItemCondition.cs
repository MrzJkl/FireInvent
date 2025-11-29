using System.Text.Json.Serialization;

namespace FireInvent.Contract;

[JsonConverter(typeof(JsonStringEnumConverter<ItemCondition>))]
public enum ItemCondition
{
    New,
    Used,
    Damaged,
    Destroyed,
    Lost
}
