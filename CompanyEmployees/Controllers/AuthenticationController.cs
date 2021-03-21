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
        private readonly IAuthenticationManager manager;

        public AuthenticationController(ILoggerManager logger, IMapper mapper, 
            UserManager<User> userManager, IAuthenticationManager manager)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.userManager = userManager;
            this.manager = manager;
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
        [HttpPost("login")] 
        [ServiceFilter(typeof(ValidationFilterAttribute))] 
        public async Task<IActionResult> Authenticate([FromBody] UserForAuthenticationDto user) 
        { 
            if (!await manager.ValidateUser(user)) 
            { 
                logger.LogWarn($"{nameof(Authenticate)}: Authentication failed. Wrong user name or password."); 
                return Unauthorized(); 
            } 
            return Ok(new { Token = await manager.CreateToken() }); 
        }
    }
}
