using EnergyMix.API.DTOs;
using EnergyMix.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnergyMix.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnergyMixController(IEnergyMixService energyMixService) : ControllerBase
    {

        [HttpGet("daily-summaries")]
        public async Task<ActionResult<IEnumerable<DailySummaryResponse>>> GetDailySummariesAsync()
        {
            return Ok(await energyMixService.GetDailySummariesAsync());
        }

        [HttpGet("best-charging-window")]
        public async Task<ActionResult<BestWindowResponse>> GetBestChargingWindowAsync(int chargingHours)
        {
            return Ok(await energyMixService.GetBestChargingWindowAsync(chargingHours));
        }
    }
}
