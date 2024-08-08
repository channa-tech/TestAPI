using Newtonsoft.Json;
using Service.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class StoryRepo : IRepository
    {
        private readonly ICache _cache;
        private readonly HttpClient _httpClient;
        private static readonly SemaphoreSlim _semaphore=new(1,1);
        private static bool isCacheBuilt = false;
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
            Stopwatch sw = Stopwatch.StartNew();
            try
            {
              
                Console.WriteLine(Thread.CurrentThread.Name + " request is processing");
                if(!isCacheBuilt)
                   await _semaphore.WaitAsync();
                
                if (!isCacheBuilt)
                {

                    var response = await _httpClient.GetAsync("newstories.json?print=pretty");
                    var res = await response.Content.ReadAsStringAsync();
                    var output = JsonConvert.DeserializeObject<string[]>(res);
                    List<Task> tasks = new List<Task>();
                    await Parallel.ForEachAsync(output, async (id, tk) =>
                    {
                        tasks.Add(GetStory(id));
                    });
                    Task.WaitAll([.. tasks]);
                    isCacheBuilt = true;
                }
            }
            catch(Exception ex)
            {
                _cache.GetCacheData().Clear();
                isCacheBuilt=false;
                throw;
            }
            finally
            {
                if(_semaphore.CurrentCount> 0)
                    _semaphore.Release();
            }
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " request processed in "+ sw.Elapsed.TotalSeconds+" seconds");
            //Console.WriteLine();
            return _cache.GetCacheData().ToList();
        }
        /// <summary>
        /// Method to fetch sotry by Id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>returns Story for a specific Id</returns>
        private async Task GetStory(string Id)
        {


            var response = await _httpClient.GetAsync($"item/{Id}.json?print=pretty");
            var res = await response.Content.ReadAsStringAsync();
            var val = JsonConvert.DeserializeObject<dynamic>(res);
            var s= ConvertTOStory(val);
            _cache.GetCacheData().Add(s);

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

        async Task<Story> IRepository.GetStory(long id)
        {
            if(_cache.IsEmpty())
            {
                await GetStories();
            }
            var story = _cache.GetStory(id);
            return story;
        }

       
    }
}
