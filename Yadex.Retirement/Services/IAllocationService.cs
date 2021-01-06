using System;
using System.Collections.Generic;
using Yadex.Retirement.Common;
using Yadex.Retirement.Dtos;
using Yadex.Retirement.Models;

namespace Yadex.Retirement.Services
{
    public interface IAllocationService
    {
        MsgResult<AllocationDto[]> GetAllAllocations(Asset[] assets);
    }
}