using FireInvent.Contract;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireInvent.Shared.Models;

public record CreateClothingProductModel
{
    [Required]
    [MaxLength(ModelConstants.MaxStringLength)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [MaxLength(ModelConstants.MaxStringLength)]
    public string Manufacturer { get; init; } = string.Empty;

    [MaxLength(ModelConstants.MaxStringLengthLong)]
    public string? Description { get; init; }

    [Required]
    public GearType Type { get; init; }
}
