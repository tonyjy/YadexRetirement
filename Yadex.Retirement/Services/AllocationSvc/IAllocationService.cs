﻿namespace Yadex.Retirement.Services;

public interface IAllocationService
{
    MsgResult<AllocationDto[]> GetAllAllocations(Dictionary<int, Asset[]> assets);
}