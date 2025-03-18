using Microsoft.AspNetCore.Mvc;
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
    }
}