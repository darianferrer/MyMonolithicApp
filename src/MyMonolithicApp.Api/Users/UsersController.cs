using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyMonolithicApp.Domain.Auth;

namespace MyMonolithicApp.Api.Users
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult Get()
        {
            return Ok();
        }

        [HttpGet("permission")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize(PermissionCodes.CanManageUsers)]
        public ActionResult GetWithPermission()
        {
            return Ok();
        }

        [HttpGet("without-permission")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize("fake-permission")]
        public ActionResult GetWithoutPermission()
        {
            return Ok();
        }
    }
}
