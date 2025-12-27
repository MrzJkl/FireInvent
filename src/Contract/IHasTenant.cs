namespace FireInvent.Contract;

public interface IHasTenant
{
    /// <summary>
    /// The unique identifier of the tenant this entity belongs to.
    /// </summary>
    public Guid TenantId { get; set; }
}
