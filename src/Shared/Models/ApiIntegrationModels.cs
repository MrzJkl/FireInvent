using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

/// <summary>
/// Request model for creating a new API integration.
/// </summary>
public class CreateApiIntegrationRequest
{
    /// <summary>
    /// User-defined name for the API integration. This will be used to identify the integration.
    /// </summary>
    [Required(ErrorMessage = "Name is required.")]
    [MinLength(1, ErrorMessage = "Name cannot be empty.")]
    [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public required string Name { get; set; }

    /// <summary>
    /// Optional description of the integration's purpose.
    /// </summary>
    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    public string? Description { get; set; }
}

/// <summary>
/// Response model containing credentials for a newly created API integration.
/// </summary>
public class ApiIntegrationCredentials
{
    /// <summary>
    /// The client ID to use for authentication.
    /// </summary>
    public required string ClientId { get; set; }

    /// <summary>
    /// The client secret to use for authentication. This is only shown once upon creation.
    /// </summary>
    public required string ClientSecret { get; set; }

    /// <summary>
    /// The name of the integration as specified by the user.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The date and time when the integration was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Model representing an existing API integration in the list.
/// </summary>
public class ApiIntegrationListItem
{
    /// <summary>
    /// The client ID of the integration.
    /// </summary>
    public required string ClientId { get; set; }

    /// <summary>
    /// The name of the integration as specified by the user.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Optional description of the integration's purpose.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether the integration is currently enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// The date and time when the integration was created.
    /// </summary>
    public DateTime? CreatedAt { get; set; }
}
