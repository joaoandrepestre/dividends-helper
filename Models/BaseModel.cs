namespace DividendsHelper.Models;
public interface IBaseModel<TId> where TId : notnull {
    TId Id { get; }
}
