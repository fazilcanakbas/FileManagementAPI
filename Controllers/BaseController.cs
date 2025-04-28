using Microsoft.AspNetCore.Mvc;

namespace FileManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseController : ControllerBase
    {
        protected IActionResult HandleError(Exception ex)
        {
            
            Console.WriteLine($"An error occurred: {ex.Message}");

            return StatusCode(500, new
            {
                error = "An error occurred while processing your request.",
                message = ex.Message
            });
        }
    }
}