using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TestAPI.Models;
using TestAPI.Services;
namespace TestAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StoriesController : ControllerBase
    {
       

        private readonly ILogger<StoriesController> _logger;
        private readonly IRepository repo;

        public StoriesController(ILogger<StoriesController> logger,IRepository repo)
        {
            _logger = logger;
            this.repo = repo;
        }

        [HttpGet]
        public IEnumerable<Story> Get(string? name)
        {
            return repo.GetStories().Result;
        }

        [HttpGet("Search")]
        public IEnumerable<Story> Search(string? name)
        {
            return repo.SearchStories(name).Result;
        }

        
    }

   
}
