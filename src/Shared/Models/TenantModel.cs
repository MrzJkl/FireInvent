using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record TenantModel : CreateOrUpdateTenantModel
{
    [Required]
    public Guid Id { get; init; }

    [Required]
    public DateTimeOffset CreatedAt { get; init; }

    public UserModel? CreatedBy { get; init; }

    public DateTimeOffset? ModifiedAt { get; init; }

    public UserModel? ModifiedBy { get; init; }
}
