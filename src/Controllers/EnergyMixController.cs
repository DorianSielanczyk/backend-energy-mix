using EnergyMix.API.DTOs;
using EnergyMix.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnergyMix.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnergyMixController : ControllerBase
    {
        private readonly IEnergyMixService _energyMixService;

        public EnergyMixController(IEnergyMixService energyMixService)
        {
            _energyMixService = energyMixService;
        }

        [HttpGet("daily-summaries")]
        public async Task<List<DailySummaryResponse>> GetDailySummariesAsync()
        {
            var dailySummaries = await _energyMixService.GetDailySummariesAsync();
            return dailySummaries;
        }

        [HttpGet("best-charging-window")]
        public async Task<BestWindowResponse?> GetBestChargingWindowAsync(int chargingHours)
        {
            var bestWindow = await _energyMixService.GetBestChargingWindowAsync(chargingHours);
            return bestWindow;
        }
    }
}
