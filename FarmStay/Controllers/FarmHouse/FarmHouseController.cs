using FarmStay.Application.DTOs.Admin;
using FarmStay.Application.Interfaces.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmStay.API.Controllers.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Super Admin")]
    public class FarmHouseController : ControllerBase
    {
        private readonly IFarmHouseService _farmHouseService;

        public FarmHouseController(IFarmHouseService farmHouseService)
        {
            _farmHouseService = farmHouseService;
        }

        // Create FarmHouse
        [HttpPost("save-farmhouse")]
        public async Task<IActionResult> Create([FromBody] CreateFarmHouseDto dto)
        {
            var result = await _farmHouseService.CreateAsync(dto);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // Update FarmHouse
        [HttpPut("update-farmhouse")]
        public async Task<IActionResult> Update([FromBody] UpdateFarmHouseDto dto)
        {
            var result = await _farmHouseService.UpdateAsync(dto);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // Delete FarmHouse
        [HttpDelete("delete-farmhouse/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _farmHouseService.DeleteAsync(id);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        // Get FarmHouse By Id
        [HttpGet("get-byid-farmhouse/{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _farmHouseService.GetByIdAsync(id);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        // Get Current FarmHouse
        [HttpGet("current")]
        public async Task<IActionResult> GetCurrent()
        {
            var result = await _farmHouseService.GetCurrentAsync();

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        // Get All FarmHouses
        [HttpGet("get-all-farmhouse")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _farmHouseService.GetAllAsync();

            return Ok(result);
        }
    }
}