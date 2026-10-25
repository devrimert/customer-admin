namespace CustomerAdmin.Api.Domain;

/// <summary>
/// Base type for persisted entities.
/// </summary>
public abstract class Entity
{
    public int Id { get; set; }

    /// <summary>Set automatically by the DbContext when the entity is first saved (UTC).</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Set automatically by the DbContext on every update (UTC).</summary>
    public DateTime? UpdatedAt { get; set; }
}