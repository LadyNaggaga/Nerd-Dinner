using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

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

            var user = await _userManager.FindByIdAsync(Context.User.GetUserId());
            var rsvp = await _repository.CreateRsvpAsync(dinner, user.UserName);
            return new JsonResult(rsvp);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteRsvpAsync(int dinnerId)
        {
            var dinner = await _repository.GetDinnerAsync(dinnerId);
            if (dinner == null)
            {
                return HttpNotFound();
            }

            var user = await _userManager.FindByIdAsync(Context.User.GetUserId());

            await _repository.DeleteRsvpAsync(dinner, user.UserName);
            return new HttpStatusCodeResult((int)HttpStatusCode.NoContent);
        }
    }
}