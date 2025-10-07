using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace TimeTracker.Web.Controllers
{
    public class BaseController : Controller
    {
        public string userId
        {
            get
            {
                return User?.FindFirstValue(ClaimTypes.NameIdentifier);

            }
        }
    }
}
