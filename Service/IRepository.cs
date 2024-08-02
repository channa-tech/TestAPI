using Newtonsoft.Json;
using System.Collections.Concurrent;
using Service.Models;

namespace Service
{
    public interface IRepository
    {
        public Task<List<Story>> GetStories();
        public Task<List<Story>> SearchStories(string Name);
        public Task<Story> GetStory(long id);
    }
    
    
}
