using System;

namespace Yadex.Retirement.Common
{
    public class Guard
    {
        public static T NotNull<T>(string name, T obj)
        {
            return obj switch
            {
                not null => obj,
                _ => throw new ArgumentNullException(name)
            };
        }
    }
}