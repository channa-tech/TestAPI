using Newtonsoft.Json;
using Service.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class StoryRepo : IRepository
    {
        private readonly ICache _cache;
        private readonly HttpClient _httpClient;
        public StoryRepo(HttpClient client,ICache cache)
        {
            _httpClient = client;
            client.BaseAddress = new Uri("https://hacker-news.firebaseio.com/v0/");
            _cache = cache;
        }

        
        /// <summary>
        /// method to fetch all new stories
        /// </summary>
        /// <returns> Returns List of Stories</returns>
        public async Task<List<Story>> GetStories()
        {
            if (_cache.IsEmpty())
            {

                var response = await _httpClient.GetAsync("newstories.json?print=pretty");
                var res = await response.Content.ReadAsStringAsync();
                var output = JsonConvert.DeserializeObject<string[]>(res);
                await Parallel.ForEachAsync(output, async (id, tk) =>
                {
                 _cache.GetCacheData().Add(await GetStory(id));
                });
            }
            _cache.GetCacheData().Order();
            return _cache.GetCacheData().ToList();
        }
        /// <summary>
        /// Method to fetch sotry by Id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>returns Story for a specific Id</returns>
        private async Task<Story> GetStory(string Id)
        {


            var response = await _httpClient.GetAsync($"item/{Id}.json?print=pretty");
            var res = await response.Content.ReadAsStringAsync();
            var val = JsonConvert.DeserializeObject<dynamic>(res);
            return ConvertTOStory(val);

        }
        /// <summary>
        /// Convert response to Story Object
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Method to search stories by Title
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public async Task<List<Story>> SearchStories(string Name)
        {
            if (_cache.IsEmpty())
            {
                await GetStories();
            }
            if (string.IsNullOrEmpty(Name))
                return _cache.GetCacheData().ToList();
            return _cache.GetCacheData().Where(s => s.Title.Contains(Name, StringComparison.OrdinalIgnoreCase)).ToList();
        }
    }
}
