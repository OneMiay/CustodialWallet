using CustodialWallet.DTOs;
using CustodialWallet.Services;
using Microsoft.AspNetCore.Mvc;

namespace CustodialWallet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    /// <summary>
    /// Users management endpoints.
    /// </summary>
    public class UsersController : ControllerBase
    {
        private readonly IUserService _service;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService service, ILogger<UsersController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new user with the specified email. / Создает нового пользователя с указанным email.
        /// </summary>
        /// <param name="dto">User creation payload. / Запрос на создание пользователя.</param>
        /// <response code="201">User successfully created. / Пользователь успешно создан.</response>
        /// <response code="400">Validation error. / Ошибка валидации.</response>
        /// <response code="409">Email already exists. / Email уже существует.</response>
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

        /// <summary>
        /// Gets the current balance for a user. / Получает текущий баланс для пользователя.
        /// </summary>
        /// <param name="userId">User identifier. / Идентификатор пользователя.</param>
        /// <response code="200">Returns the current balance. / Возвращает текущий баланс.</response>
        /// <response code="404">User not found. / Пользователь не найден.</response>
        [HttpGet("{userId:guid}/balance")]
        public async Task<IActionResult> GetBalance([FromRoute] Guid userId)
        {
            var balance = await _service.GetBalanceAsync(userId);
            if (balance == null) return NotFound();
            return Ok(balance);
        }

        /// <summary>
        /// Deposits an amount to the user's balance. / Депозит средств на баланс пользователя.
        /// </summary>
        /// <param name="userId">User identifier. / Идентификатор пользователя.</param>
        /// <param name="dto">Deposit payload. / Запрос на депозит средств.</param>
        /// <response code="200">Deposit successful, returns new balance. / Депозит успешно выполнен, возвращает новый баланс.</response>
        /// <response code="400">Validation error. / Ошибка валидации.</response>
        /// <response code="404">User not found. / Пользователь не найден.</response>
        [HttpPost("{userId:guid}/deposit")]
        public async Task<IActionResult> Deposit([FromRoute] Guid userId, [FromBody] DepositDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var res = await _service.DepositAsync(userId, dto.Amount);
            if (res == null) return NotFound();
            return Ok(new { userId = res.UserId, newBalance = res.Balance });
        }

        /// <summary>
        /// Withdraws an amount from the user's balance. / Снятие средств с баланса пользователя.
        /// </summary>
        /// <param name="userId">User identifier. / Идентификатор пользователя.</param>
        /// <param name="dto">Withdraw payload. / Запрос на снятие средств.</param>
        /// <response code="200">Withdrawal successful, returns new balance. / Снятие успешно выполнено, возвращает новый баланс.</response>
        /// <response code="400">Validation error or insufficient funds. / Ошибка валидации или недостаточно средств.</response>
        /// <response code="404">User not found. / Пользователь не найден.</response>
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
