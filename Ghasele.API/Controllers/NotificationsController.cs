using System;
using System.Threading.Tasks;
using Ghasele.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Ghasele.API.Controllers
{
    /// <summary>
    /// The customer's in-app notification list, for signed-in customers and guests alike.
    /// </summary>
    /// <remarks>
    /// Open to anonymous callers on purpose: a guest's list is filed under the
    /// <c>X-Device-Token</c> of the install that placed their orders, exactly as their orders
    /// are. A signed-in caller always gets their account's list, never the device's.
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ApiControllerBase
    {
        private readonly IUserNotificationService _notificationService;

        public NotificationsController(IUserNotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var notifications = await _notificationService.GetNotificationsAsync(CallerId(), DeviceToken());
            return Ok(notifications);
        }

        [HttpPatch("{id}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var found = await _notificationService.MarkAsReadAsync(id, CallerId(), DeviceToken());
            return found ? Ok() : NotFound();
        }

        [HttpPatch("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            await _notificationService.MarkAllAsReadAsync(CallerId(), DeviceToken());
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var found = await _notificationService.DeleteNotificationAsync(id, CallerId(), DeviceToken());
            return found ? Ok() : NotFound();
        }
    }
}
