namespace CustomerAdmin.Api.Domain;

/// <summary>
/// Base type for persisted entities.
/// </summary>
public abstract class Entity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}