namespace SpaFramework.App.Models.Data
{
    /// <summary>
    /// This interface indicates a data model supports soft deletes. With a soft delete, we maintain the underlying item in the database. But, by default, read/list services don't include
    /// soft deleted items.
    /// </summary>
    public interface IHasDeleted
    {
        bool Deleted { get; set; }
    }
}
