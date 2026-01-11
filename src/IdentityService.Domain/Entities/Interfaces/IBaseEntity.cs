namespace IdentityService.Domain.Entities.Interfaces;

public interface IBaseEntity : IIdentifier
{
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public bool Removed { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? RemovedAt { get; set; }
    public Guid? RemovedBy { get; set; }

    public void PrepareToInsert(Guid userId);

    public void PrepareToUpdate(Guid userId);

    public void PrepareToRemove(Guid userId);
}
