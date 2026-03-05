using System;
using System.Threading.Tasks;
using Ghasele.Application.DTOs;
using Ghasele.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ghasele.API.Controllers
{
    [ApiController]
    [Route("api/route-optimization")]
    [Authorize]
    public class RouteOptimizationController : ControllerBase
    {
        private readonly IRouteOptimizationService _routeOptimizationService;

        public RouteOptimizationController(IRouteOptimizationService routeOptimizationService)
        {
            _routeOptimizationService = routeOptimizationService;
        }

        [HttpPost("optimize")]
        public async Task<IActionResult> OptimizeRoute([FromBody] OptimizeRouteRequestDto request)
        {
            try
            {
                var optimizedRoute = await _routeOptimizationService.OptimizeRouteAsync(request);
                return Ok(optimizedRoute);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
