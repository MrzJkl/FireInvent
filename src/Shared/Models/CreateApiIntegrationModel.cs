using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public class CreateApiIntegrationModel
{
    [Required]
    [MinLength(1)]
    [MaxLength(100)]
    public required string Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }
}
