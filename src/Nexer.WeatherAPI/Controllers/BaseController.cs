using Microsoft.AspNetCore.Mvc;
using Nexer.Domain.Interfaces.Notificator;
using Nexer.WeatherAPI.Responses;

namespace Nexer.WeatherAPI.Controllers
{
    public class BaseController : ControllerBase
    {
        protected readonly INotificator _notificator;
        public BaseController(INotificator notificator)
        {
            _notificator = notificator;
        }

        protected IActionResult CustomResult(object data = null)
        {
            if (_notificator.HasErrors())
            {
                return BadRequest(new CustomResponse
                {
                    Success = false,
                    Notifications = _notificator.GetNotifications(),
                    Data = null
                });
            }

            return Ok(new CustomResponse
            {
                Success = true,
                Notifications = _notificator.GetNotifications(),
                Data = data
            });
        }
    }
}
