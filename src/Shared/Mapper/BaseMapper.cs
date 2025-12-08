namespace FireInvent.Shared.Mapper;

public class BaseMapper
{
    protected static Guid NewGuid() => Guid.NewGuid();
    protected static DateTimeOffset UtcNow() => DateTimeOffset.UtcNow;
}