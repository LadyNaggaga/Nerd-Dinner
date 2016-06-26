using System.Net;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NerdDinner.Web.Models;
using NerdDinner.Web.Persistence;
using System;


// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace NerdDinner.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class RsvpController : Controller
    {
        private readonly INerdDinnerRepository _repository;

        private readonly UserManager<ApplicationUser> _userManager;

        public RsvpController(INerdDinnerRepository repository, UserManager<ApplicationUser> userManager)
        {
            _repository = repository;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> CreateRsvpAsync(int dinnerId)
        {
            var dinner = await _repository.GetDinnerAsync(dinnerId);
            if (dinner == null)
            {
                return HttpNotFound();
            }

            var user = await _userManager.GetUserAsync(HttpContext.User);
            var rsvp = await _repository.CreateRsvpAsync(dinner, user.UserName);
            return new JsonResult(rsvp);
        }

        private IActionResult HttpNotFound()
        {
            throw new NotImplementedException();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteRsvpAsync(int dinnerId)
        {
            var dinner = await _repository.GetDinnerAsync(dinnerId);
            if (dinner == null)
            {
                return HttpNotFound();
            }

            var user = await _userManager.GetUserAsync(HttpContext.User);

            await _repository.DeleteRsvpAsync(dinner, user.UserName);
            return new StatusCodeResult((int)HttpStatusCode.NoContent);
        }
    }
}