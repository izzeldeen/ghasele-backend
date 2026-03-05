using System.Threading.Tasks;
using Ghasele.Application.DTOs;

namespace Ghasele.Application.Interfaces
{
    public interface IRouteOptimizationService
    {
        Task<OptimizedRouteResponseDto> OptimizeRouteAsync(OptimizeRouteRequestDto request);
    }
}
