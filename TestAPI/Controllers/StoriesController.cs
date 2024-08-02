using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Service.Models;
using Service;
namespace TestAPI.Controllers
{
    /// <summary>
    /// Story controller
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class StoriesController : ControllerBase
    {
       

        private readonly ILogger<StoriesController> _logger;
        private readonly IRepository repo;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="repo"></param>
        public StoriesController(ILogger<StoriesController> logger,IRepository repo)
        {
            _logger = logger;
            this.repo = repo;
        }
        /// <summary>
        /// Endpoint to fetch list of all new stories
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<Story> Get()
        {
            return repo.GetStories().Result;
        }
        /// <summary>
        /// Endpoint to search stories by title
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpGet("Search")]
        public IEnumerable<Story> Search(string? name)
        {
            return repo.SearchStories(name).Result;
        }

        /// <summary>
        /// Fetch Story by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("Story/{id:long}")]
        public Story GetStory(long id)
        {
            return  repo.GetStory(id).Result;
        }

        
    }

   
}
