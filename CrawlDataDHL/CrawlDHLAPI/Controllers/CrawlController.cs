using CrawlLib;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CrawlDHLAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CrawlController : ControllerBase
    {
        [HttpGet("{trackingNumber}")]
        public async Task<ActionResult<string>> GetTodoItem(string trackingNumber)
        {
            Crawler crawler = new Crawler();
            return crawler.Crawl(trackingNumber);
        }
    }
}
