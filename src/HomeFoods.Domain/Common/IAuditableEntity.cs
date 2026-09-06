namespace HomeFoods.Domain.Common;

public interface IAuditableEntity : IEntity
{
    DateTime CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
}
