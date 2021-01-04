using System.Collections.Generic;
using Yadex.Retirement.Common;
using Yadex.Retirement.Dtos;
using Yadex.Retirement.Models;

namespace Yadex.Retirement.Services
{
    public class SimpleAllocationService : IAllocationService
    {
        /// <summary>
        /// This is the main entry point for calculate the allocations
        /// </summary>
        /// <param name="settings">The settings include birth year, pension, social security, compound rate</param>
        /// <param name="assets">The all the actual asset</param>
        /// <returns></returns>
        public MsgResult<AllocationDto[]> GetAllAllocations(YadexRetirementSettings settings, Asset[] assets)
        {
            var allocations = new List<AllocationDto>();
            
            
            
            return new MsgResult<AllocationDto[]>(true, string.Empty, allocations.ToArray());
        }
    }
}