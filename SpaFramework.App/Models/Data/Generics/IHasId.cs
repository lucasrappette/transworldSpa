namespace SpaFramework.App.Models.Data
{
    public interface IHasId<TIdType>
    {
        TIdType GetId();
    }
}
