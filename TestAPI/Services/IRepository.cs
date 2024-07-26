using Newtonsoft.Json;
using System.Collections.Concurrent;
using TestAPI.Models;

namespace TestAPI.Services
{
    public interface IRepository
    {
        public Task<List<Story>> GetStories();
        public Task<List<Story>> SearchStories(string Name);
    }
    
    public class StoryRepo : IRepository
    {
        private static ConcurrentBag<Story> Cachestories=new ConcurrentBag<Story>();
        private readonly HttpClient _httpClient;
        public StoryRepo(HttpClient client)
        {
            _httpClient = client;
            client.BaseAddress =new Uri("https://hacker-news.firebaseio.com/v0/");
        }
        public async Task<List<Story>> GetStories()
        {
            if (Cachestories.IsEmpty)
            {
               
                var response = await _httpClient.GetAsync("newstories.json?print=pretty");
                var res = await response.Content.ReadAsStringAsync();
                var output = JsonConvert.DeserializeObject<string[]>(res);
                await Parallel.ForEachAsync(output, async (id, tk) =>
                 {
                     Cachestories.Add(await GetStory(id));
                 });
            }
            Cachestories.Order();
            return Cachestories.ToList();
        }

        private async Task<Story> GetStory(string Id)
        {
            
          
            var response = await _httpClient.GetAsync($"item/{Id}.json?print=pretty");
            var res=await response.Content.ReadAsStringAsync();
            var val=JsonConvert.DeserializeObject<dynamic>(res);
           return ConvertTOStory(val);
            
        }

        private Story ConvertTOStory(dynamic response)
        {
            return new Story()
            {
                Id = response.id,
                Title = response.title,
                Url = response.url,
                PostedBy = response.by
            };
        }

        public async Task<List<Story>> SearchStories(string Name)
        {
            if (Cachestories.IsEmpty)
            {
                await GetStories();
            }
            if (string.IsNullOrEmpty(Name))
                return Cachestories.ToList();
            return Cachestories.Where(s => s.Title.Contains(Name,StringComparison.OrdinalIgnoreCase)).ToList();
        }
    }
}
