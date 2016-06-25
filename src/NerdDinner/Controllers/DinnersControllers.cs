using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NerdDinner.Web.Common;
using NerdDinner.Web.Models;
using NerdDinner.Web.Persistence;


// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace NerdDinner.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class DinnersController : Controller
    {
        private readonly INerdDinnerRepository _repository;

        private readonly UserManager<ApplicationUser> _userManager;

        public DinnersController(INerdDinnerRepository repository, UserManager<ApplicationUser> userManager)
        {
            _repository = repository;
            _userManager = userManager;
        }

        [HttpGet("{id:int}", Name = "GetDinnerById")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDinnerAsync(int id)
        {
            var dinner = await _repository.GetDinnerAsync(id);
            if (dinner == null)
            {
                return NotFound();
            }

            return new ObjectResult(dinner);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IEnumerable<Dinner>> GetDinnersAsync(
            DateTime? startDate,
            DateTime? endDate,
            double? lat,
            double? lng,
            int? pageIndex,
            int? pageSize,
            string searchQuery = null,
            string sort = null,
            bool descending = false)
        {
            return await _repository.GetDinnersAsync(startDate, endDate, string.Empty, searchQuery, sort, descending, lat, lng, pageIndex, pageSize);
        }

        [HttpGet("my")]
        [AllowAnonymous]
        public async Task<IEnumerable<Dinner>> GetMyDinnersAsync(
            DateTime? startDate,
            DateTime? endDate,
            double? lat,
            double? lng,
            int? pageIndex,
            int? pageSize,
            string searchQuery = null,
            string sort = null,
            bool descending = false)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            return await _repository.GetDinnersAsync(startDate, endDate, user.UserName, searchQuery, sort, descending, lat, lng, pageIndex, pageSize);
        }

        [HttpGet("popular")]
        [AllowAnonymous]
        public async Task<IEnumerable<Dinner>> GetPopularDinnersAsync()
        {
            return await _repository.GetPopularDinnersAsync();
        }

        [HttpGet("count")]
        [AllowAnonymous]
        public int GetDinnersCount()
        {
            return _repository.GetDinnersCount();
        }

        [HttpGet("isUserHost")]
        [AllowAnonymous]
        public async Task<IActionResult> IsUserHost(int id)
        {
            if (HttpContext.User == null)
            {
                return new ObjectResult(false);
            }

            var dinner = await _repository.GetDinnerAsync(id);
            var user = await _userManager.GetUserAsync(HttpContext.User);
            return new ObjectResult(dinner.IsUserHost(user.UserName));
        }

        [HttpGet("isUserRegistered")]
        [AllowAnonymous]
        public async Task<IActionResult> IsUserRegistered(int id)
        {
            if (HttpContext.User == null)
            {
                return new ObjectResult(false);
            }

            var dinner = await _repository.GetDinnerAsync(id);
            var user = await _userManager.GetUserAsync(HttpContext.User);
            return new ObjectResult(dinner.IsUserRegistered(user.UserName));
        }

        [HttpPost]
        public async Task<IActionResult> CreateDinnerAsync([FromBody] Dinner dinner)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            dinner.UserName = user.UserName;

            GeoLocation.SearchByPlaceNameOrZip(dinner);
            dinner = await _repository.CreateDinnerAsync(dinner);
            var url = Url.RouteUrl("GetDinnerById", new { id = dinner.DinnerId }, Request.Scheme, Request.Host.ToUriComponent());
            HttpContext.Response.StatusCode = (int)HttpStatusCode.Created;
            HttpContext.Response.Headers["Location"] = url;
            return new ObjectResult(dinner);
        }

        [HttpPut("{id:int}", Name = "UpdateDinnerById")]
        public async Task<IActionResult> UpdateDinnerAsync(int id, [FromBody] Dinner dinner)
        {
            if (dinner.DinnerId != id)
            {
                return new StatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (!dinner.IsUserHost(user.UserName))
            {
                return NotFound();
            }

            GeoLocation.SearchByPlaceNameOrZip(dinner);
            dinner = await _repository.UpdateDinnerAsync(dinner);
            return new ObjectResult(dinner);
        }

        [HttpDelete("{id:int}", Name = "DeleteDinnerById")]
        public async Task<IActionResult> DeleteDinnerAsync(int id)
        {
            var dinner = await _repository.GetDinnerAsync(id);
            var user = await _userManager.GetUserAsync(HttpContext.User);

            if (!dinner.IsUserHost(user.UserName))
            {
                return NotFound();
            }

            await _repository.DeleteDinnerAsync(id);
            return new StatusCodeResult((int)HttpStatusCode.NoContent);
        }
    }
}
