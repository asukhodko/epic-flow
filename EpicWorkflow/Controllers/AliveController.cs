using Microsoft.AspNetCore.Mvc;

namespace EpicWorkflow.Controllers
{
    [Route("alive")]
    public class AliveController : Controller
    {
        public IActionResult Index()
        {
            return new OkResult();
        }
    }
}