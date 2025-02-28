using Microsoft.AspNetCore.Mvc;
using InBoostWeatherBot.Services;

namespace InBoostWeatherBot.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserInfo(int userId)
        {
            var user = await _userService.GetUserAsync(userId);
            if (user == null) return NotFound("Пользователь не найден");
            return Ok(user);
        }
    }
}
