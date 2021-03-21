using AutoMapper;
using CompanyEmployees.ActionFilters;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CompanyEmployees.Controllers
{
    [Route("api/authentication")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly ILoggerManager logger;
        private readonly IMapper mapper;
        private readonly UserManager<User> userManager;
        
        public AuthenticationController(ILoggerManager logger, IMapper mapper, UserManager<User> userManager)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.userManager = userManager;
        }

        [HttpPost]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> RegisterUser([FromBody]UserForRegistrationDto userForRegistration)
        {
            var user = mapper.Map<User>(userForRegistration);

            var result = await userManager.CreateAsync(user, userForRegistration.Password);
            if(!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return BadRequest(ModelState);
            }

            await userManager.AddToRolesAsync(user, userForRegistration.Roles);

            return StatusCode(201);
        }
    }
}
