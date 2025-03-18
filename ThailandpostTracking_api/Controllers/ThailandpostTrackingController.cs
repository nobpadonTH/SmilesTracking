using Microsoft.AspNetCore.Mvc;
using ThailandpostTracking.Attributes;
using ThailandpostTracking.DTOs.Thailandpost.Request;
using ThailandpostTracking.DTOs.Thailandpost.Response;
using ThailandpostTracking.Models;
using ThailandpostTracking.Services.ThailandpostTracking;

namespace ThailandpostTracking.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ThailandpostTrackingController : ControllerBase
    {
        private readonly IThailandpostTrackingServices _services;

        public ThailandpostTrackingController(IThailandpostTrackingServices services)
        {
            _services = services;
        }

        [HttpPost("inserttracking")]
        public async Task<ServiceResponse<GetItemsbyBarcodeResponseDTO>> InsertTracking([FromBody] GetItemsbyBarcodeRequestDTO input)
        {
            try
            {
                var result = await _services.InsertTracking(input);
                return ResponseResult.Success(result, "Success");
            }
            catch (Exception e)
            {
                return ResponseResult.Failure<GetItemsbyBarcodeResponseDTO>(e.Message);
            }
        }

        [HttpPost("upserttracking")]
        public async Task<ServiceResponse<GetItemsbyBarcodeResponseDTO>> UpsertTracking([FromBody] GetItemsbyBarcodeRequestDTO input)
        {
            try
            {
                var result = await _services.UpsertTracking(input);
                return ResponseResult.Success(result, "Success");
            }
            catch (Exception e)
            {
                return ResponseResult.Failure<GetItemsbyBarcodeResponseDTO>(e.Message);
            }
        }

        [HttpPost("getrequestitems")]
        public async Task<ServiceResponse<GetRequestItemsResponseDTO>> GetRequestItems([FromBody] GetItemsbyBarcodeRequestDTO input)
        {
            try
            {
                var result = await _services.GetRequestItems(input);
                return ResponseResult.Success(result, "Success");
            }
            catch (Exception e)
            {
                return ResponseResult.Failure<GetRequestItemsResponseDTO>(e.Message);
            }
        }

        [HttpGet("gettrackingheader/filter")]
        public async Task<IActionResult> GetTrackingHeader([FromQuery] GetTrackingHeaderRequestDTO param)
        {
            var data = await _services.GetTrackingHeader(param);
            return Ok(data);
        }

        [HttpGet("gettrackingdetail/filter")]
        public async Task<IActionResult> GetTrackingDetail([FromQuery] GetTrackingDetailRequestDTO param)
        {
            var data = await _services.GetTrackingDetail(param);
            return Ok(data);
        }
    }
}