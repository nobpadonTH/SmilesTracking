using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quartz;
using ThailandpostTracking.Attributes;
using ThailandpostTracking.DTOs.Thailandpost.Request;
using ThailandpostTracking.DTOs.Thailandpost.Response;
using ThailandpostTracking.Models;
using ThailandpostTracking.Services.ThailandpostTracking;

namespace ThailandpostTracking.Controllers
{
    [Authorize(Permission.Base)]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ThailandpostTrackingController : ControllerBase
    {
        private readonly IThailandpostTrackingServices _services;
        private readonly ISchedulerFactory _schedulerFactory;

        public ThailandpostTrackingController(IThailandpostTrackingServices services, ISchedulerFactory schedulerFactory)
        {
            _services = services;
            _schedulerFactory = schedulerFactory;
        }

        /// <summary>
        /// Insert Tracking
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>

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

        /// <summary>
        /// Upsert Tracking
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>

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

        /// <summary>
        /// Get Tracking Header
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>

        [HttpGet("gettrackingheader/filter")]
        public async Task<IActionResult> GetTrackingHeader([FromQuery] GetTrackingHeaderRequestDTO param)
        {
            var data = await _services.GetTrackingHeader(param);
            return Ok(data);
        }

        /// <summary>
        /// Get Tracking Detail
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>

        [HttpGet("gettrackingdetail/filter")]
        public async Task<ServiceResponse<List<GetTimeLineResponseDTO>>> GetTrackingDetail([FromQuery] GetTrackingDetailRequestDTO param)
        {
            try
            {
                var data = await _services.GetTrackingDetail(param);
                return ResponseResult.Success(data, "Success");
            }
            catch (Exception e)
            {
                return ResponseResult.Failure<List<GetTimeLineResponseDTO>>(e.Message);
            }
        }

        /// <summary>
        /// Get Report Tracking
        /// </summary>
        /// <returns></returns>

        [HttpGet("getreporttrackingDownload", Name = "GetReportTrackingDownload")]
        public async Task<IActionResult> GetReportPremiumTrackingDownload()
        {
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var result = await _services.GetReportTracking();

            if (result.IsSuccess == true)
                return File(result.Data.Data, contentType, result.Data.FileName);

            return Ok(result);
        }

        [HttpPost("trigger")]
        public async Task<IActionResult> TriggerJob([FromQuery] string jobName = "SampleJob")
        {
            var scheduler = await _schedulerFactory.GetScheduler();

            // 🟢 Ensure scheduler is started
            if (!scheduler.IsStarted)
                await scheduler.Start();

            var jobKey = new JobKey(jobName);

            if (!await scheduler.CheckExists(jobKey))
            {
                return NotFound($"Job '{jobName}' does not exist.");
            }

            await scheduler.TriggerJob(jobKey);
            return Ok($"Job '{jobName}' triggered.");
        }
    }
}