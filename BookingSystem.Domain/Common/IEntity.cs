namespace BookingSystem.Domain.Common;

public interface IEntity
{
    protected uint RowVersion { get; }
}