using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyMonolithicApp.Application.Auth;

namespace MyMonolithicApp.Api.Auth
{
    [ApiController]
    [Route("[controller]")]
    [AllowAnonymous]
    public class TokenController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TokenController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("connect")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LoginResponse>> ConnectAsync(LoginCommand command, CancellationToken cancellationToken = default)
        {
            var loginResponse = await _mediator.Send(command, cancellationToken);
            return loginResponse.IsT0
                ? loginResponse.AsT0
                : (ActionResult<LoginResponse>)BadRequest(new
                {
                    Errors = new[] { loginResponse.AsT1 }
                });
        }
    }
}
