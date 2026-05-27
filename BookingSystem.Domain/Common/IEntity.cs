namespace BookingSystem.Domain.Common;

public interface IEntity
{
    uint RowVersion { get; set; }
}