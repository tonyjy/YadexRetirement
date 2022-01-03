namespace Yadex.Retirement.Services;

public interface IAllocationService
{
    MsgResult<AllocationDto[]> GetAllAllocations(Asset[] assets);
}
