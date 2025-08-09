using CustodialWallet.DTOs;
using CustodialWallet.Services;
using Microsoft.AspNetCore.Mvc;

namespace CustodialWallet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _service;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService service, ILogger<UsersController> logger)
        {
            _service = service;
            _logger = logger;
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var (success, error, created) = await _service.CreateUserAsync(dto);
            if (!success)
            {
                if (error == "Email already exists")
                {
                    return Conflict(new { error });
                }
                return BadRequest(new { error });
            }

            return CreatedAtAction(nameof(GetBalance), new { userId = created!.UserId }, created);
        }


        [HttpGet("{userId:guid}/balance")]
        public async Task<IActionResult> GetBalance([FromRoute] Guid userId)
        {
            var balance = await _service.GetBalanceAsync(userId);
            if (balance == null) return NotFound();
            return Ok(balance);
        }


        [HttpPost("{userId:guid}/deposit")]
        public async Task<IActionResult> Deposit([FromRoute] Guid userId, [FromBody] DepositDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var res = await _service.DepositAsync(userId, dto.Amount);
            if (res == null) return NotFound();
            return Ok(new { userId = res.UserId, newBalance = res.Balance });
        }


        [HttpPost("{userId:guid}/withdraw")]
        public async Task<IActionResult> Withdraw([FromRoute] Guid userId, [FromBody] WithdrawDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var (success, error, result) = await _service.WithdrawAsync(userId, dto.Amount);
            if (!success)
            {
                if (error == "User not found") return NotFound(new { error });
                return BadRequest(new { error });
            }

            return Ok(new { userId = result!.UserId, newBalance = result.Balance });
        }
    }
}
