using Service.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class Cache : ICache
    {
        public Cache()
        {
            Stories=new ConcurrentBag<Story> ();
        }

        public ConcurrentBag<Story> Stories { get; private set; }
        /// <summary>
        /// Method to fetch Cache Data
        /// </summary>
        /// <returns>Concurrent bag of stories</returns>
        public ConcurrentBag<Story> GetCacheData()
        {
            return Stories;
        }
        /// <summary>
        /// Method to check if the cache is empty
        /// </summary>
        /// <returns>bool</returns>
        public bool IsEmpty()
        {
            return Stories.IsEmpty;
        }
    }
    
}
