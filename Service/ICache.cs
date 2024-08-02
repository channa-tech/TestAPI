using Service.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public interface ICache
    {
        public bool IsEmpty();
        public ConcurrentBag<Story> GetCacheData();
        public Story? GetStory(long id);

    }
}
