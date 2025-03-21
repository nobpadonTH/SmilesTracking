using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RestSharp;
using Serilog;
using System.Globalization;
using System.Linq.Dynamic.Core;
using System.Text.Json;
using ThailandpostTracking.Configurations;
using ThailandpostTracking.Data;
using ThailandpostTracking.DTOs.Thailandpost.Request;
using ThailandpostTracking.DTOs.Thailandpost.Response;
using ThailandpostTracking.Helpers;
using ThailandpostTracking.Models;
using ThailandpostTracking.Services.ThailandpostTracking.Report;
using ILogger = Serilog.ILogger;

namespace ThailandpostTracking.Services.ThailandpostTracking
{
    public class ThailandpostTrackingServices : IThailandpostTrackingServices
    {
        private readonly RestClient _client;
        private readonly ThailandpostTrackingSetting _configuration;
        private readonly AppDBContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContext;
        private readonly ILogger _logger;

        private readonly List<string> _trackingInfo = new List<string>
        {
            "TrackingCode",
            "ReferenceCode",
            "สถานะ",
            "คำอธิบายสถานะ",
            "สถานะของวันที่",
            "สถานะของเวลา",
            "รายละเอียดเพิ่มเติมของสถานะ",
            "สถานที่ตั้ง",
            "รหัสไปรษณีย์",
            "สถานะการจัดส่ง",
            "คำอธิบายการจัดส่ง",
            "วันที่จัดส่ง",
            "เวลาจัดส่ง",
            "ชื่อผู้รับ",
            "ลายเซ็น",
            "ชื่อเจ้าหน้าที่นำจ่าย",
            "เบอร์ติดต่อเจ้าหน้าที่นำจ่าย",
            "ชื่อที่ทำการไปรษณีย์",
            "เบอร์ติดต่อที่ทำการไปรษณีย์",
            "THP Contact Center"
        };

        public ThailandpostTrackingServices(IOptions<ThailandpostTrackingSetting> configuration, AppDBContext dbContext, IMapper mapper, IHttpContextAccessor httpContext)
        {
            _client = new RestClient();
            _configuration = configuration.Value;
            _dbContext = dbContext;
            _mapper = mapper;
            _httpContext = httpContext;
            _logger = Log.ForContext<ThailandpostTrackingServices>();
        }

        public async Task<GetItemsbyBarcodeResponseDTO> InsertTracking(GetItemsbyBarcodeRequestDTO input)
        {
            var response = await GetItemsbyBarcode(input);

            string jsonString = JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });

            var now = DateTime.Now;
            var batchId = Guid.NewGuid();
            var dataBatch = new TrackingBatch()
            {
                TrackingBatchId = batchId,
                JsonResponse = jsonString,
                TrackingBatchTypeId = 1,
                IsActive = true,
                CreatedDate = now,
                CreatedByUserId = 1,
                UpdatedDate = now,
                UpdatedByUserId = 1
            };

            if (response.Status)
            {
                foreach (var track in input.Barcode)
                {
                    var jsonObject = response?.Response.Items[track];

                    var tmpImportTracking = await _dbContext.TmpImportTrackings.OrderBy(x => x.TmpImportTrackingId).FirstOrDefaultAsync(x => x.TrackingCode == track);
                    if (jsonObject?.Count != 0)
                    {
                        var headerId = Guid.NewGuid();

                        var selectList = jsonObject?
                            .Select(item => new
                            {
                                Item = item,
                                ParsedDate = ParseBuddhistDate(item.Status_Date)
                            })
                            .OrderByDescending(x => x.ParsedDate)
                            .FirstOrDefault();

                        var dataInsert = new TrackingHeader
                        {
                            TrackingHeaderId = headerId,
                            TmpImportTrackingId = tmpImportTracking.TmpImportTrackingId,
                            TrackingBatchId = batchId,
                            TrackingCode = track,
                            Status = selectList?.Item.Status,
                            Status_Description = selectList?.Item.Status_Description,
                            Status_Date = ParseBuddhistDate(selectList.Item.Status_Date),
                            Location = selectList?.Item.Location,
                            Postcode = selectList?.Item.Postcode,
                            Delivery_Description = selectList?.Item.Delivery_Description,
                            Delivery_Datetime = ParseBuddhistDate(selectList.Item.Delivery_Datetime),
                            Signature = selectList?.Item.Signature,
                            Receiver_Name = selectList?.Item.Receiver_Name,
                            Delivery_Officer_Name = selectList?.Item.Delivery_Officer_Name,
                            Delivery_Officer_Tel = selectList?.Item.Delivery_Officer_Tel,
                            Office_Name = selectList?.Item.Office_Name,
                            Office_Tel = selectList?.Item.Office_Tel,
                            Call_Center_Tel = selectList?.Item.Call_Center_Tel,
                            Delivery_Status = selectList?.Item.Delivery_Status,
                            IsActive = true,
                            CreatedDate = now,
                            CreatedByUserId = 1,
                            UpdatedDate = now,
                            UpdatedByUserId = 1
                        };
                        _dbContext.Add(dataInsert);

                        var trackingItem = _mapper.Map<List<TrackingDetail>>(jsonObject);

                        trackingItem.ForEach(x =>
                        {
                            x.TrackingDetailId = Guid.NewGuid();
                            x.TrackingBatchId = batchId;
                            x.TrackingHeaderId = headerId;
                            x.Status_Date = x.Status_Date?.AddYears(-543);
                            x.Delivery_Datetime = x.Delivery_Datetime?.AddYears(-543);
                            x.IsActive = true;
                            x.CreatedDate = now;
                            x.CreatedByUserId = 1;
                            x.UpdatedDate = now;
                            x.UpdatedByUserId = 1;
                        });

                        await _dbContext.AddRangeAsync(trackingItem);

                        // Ensure tmpImportTracking is not null before updating
                        if (tmpImportTracking != null)
                        {
                            tmpImportTracking.IsInsert = true;
                            tmpImportTracking.IsResult = true;
                            tmpImportTracking.TransactionDate = now;
                            tmpImportTracking.Message = "บันทึกสำเร็จ";

                            _dbContext.Update(tmpImportTracking);
                        }
                    }
                    else
                    {
                        // Ensure tmpImportTracking is not null before updating
                        if (tmpImportTracking != null)
                        {
                            tmpImportTracking.IsInsert = false;
                            tmpImportTracking.IsResult = false;
                            tmpImportTracking.TransactionDate = now;
                            tmpImportTracking.Message = "บันทึกไม่สำเร็จ";

                            _dbContext.Update(tmpImportTracking);
                        }
                    }
                }
                _dbContext.Add(dataBatch);

                await _dbContext.SaveChangesAsync();
            }
            return response;
        }

        public async Task<GetItemsbyBarcodeResponseDTO> UpsertTracking(GetItemsbyBarcodeRequestDTO input)
        {
            var response = await GetItemsbyBarcode(input);

            string jsonString = JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });

            var now = DateTime.Now;
            var batchId = Guid.NewGuid();
            var dataBatch = new TrackingBatch()
            {
                TrackingBatchId = batchId,
                JsonResponse = jsonString,
                TrackingBatchTypeId = 2,
                IsActive = true,
                CreatedDate = now,
                CreatedByUserId = 1,
                UpdatedDate = now,
                UpdatedByUserId = 1
            };

            if (response.Status)
            {
                foreach (var track in input.Barcode)
                {
                    var jsonObject = response?.Response.Items[track];

                    if (jsonObject?.Count != 0)
                    {
                        var headerId = Guid.NewGuid();

                        var selectList = jsonObject?
                            .Select(item => new
                            {
                                Item = item,
                                ParsedDate = ParseBuddhistDate(item.Status_Date)
                            })
                            .OrderByDescending(x => x.ParsedDate)
                            .FirstOrDefault();

                        var dataInsert = await _dbContext.TrackingHeaders.Where(x => x.IsActive == true && x.TrackingCode == track).SingleOrDefaultAsync();

                        if (dataInsert != null)
                        {
                            dataInsert.TrackingBatchId = batchId;
                            dataInsert.Status = selectList?.Item.Status;
                            dataInsert.Status_Description = selectList?.Item.Status_Description;
                            dataInsert.Status_Date = ParseBuddhistDate(selectList.Item.Status_Date);
                            dataInsert.Location = selectList?.Item.Location;
                            dataInsert.Postcode = selectList?.Item.Postcode;
                            dataInsert.Delivery_Description = selectList?.Item.Delivery_Description;
                            dataInsert.Delivery_Datetime = ParseBuddhistDate(selectList.Item.Delivery_Datetime);
                            dataInsert.Signature = selectList?.Item.Signature;
                            dataInsert.Receiver_Name = selectList?.Item.Receiver_Name;
                            dataInsert.Delivery_Officer_Name = selectList?.Item.Delivery_Officer_Name;
                            dataInsert.Delivery_Officer_Tel = selectList?.Item.Delivery_Officer_Tel;
                            dataInsert.Office_Name = selectList?.Item.Office_Name;
                            dataInsert.Office_Tel = selectList?.Item.Office_Tel;
                            dataInsert.Call_Center_Tel = selectList?.Item.Call_Center_Tel;
                            dataInsert.Delivery_Status = selectList?.Item.Delivery_Status;
                            dataInsert.UpdatedDate = now;
                            dataInsert.UpdatedByUserId = 1;
                            _dbContext.Update(dataInsert);
                        }

                        var trackingItem = _mapper.Map<List<TrackingDetail>>(jsonObject);

                        trackingItem.ForEach(x =>
                        {
                            x.TrackingDetailId = Guid.NewGuid();
                            x.TrackingBatchId = batchId;
                            x.TrackingHeaderId = dataInsert?.TrackingHeaderId;
                            x.Status_Date = x.Status_Date?.AddYears(-543);
                            x.Delivery_Datetime = x.Delivery_Datetime?.AddYears(-543);
                            x.IsActive = true;
                            x.CreatedDate = now;
                            x.CreatedByUserId = 1;
                            x.UpdatedDate = now;
                            x.UpdatedByUserId = 1;
                        });
                        await _dbContext.AddRangeAsync(trackingItem);
                    }
                }
                _dbContext.Add(dataBatch);

                Log.Debug("[UpsertTracking] - SaveChangesAsync");
                await _dbContext.SaveChangesAsync();
            }

            return response;
        }

        private async Task<string> GetToken()
        {
            _client.AddDefaultHeader("Authorization", "Token " + _configuration.APIKey);
            _client.AddDefaultHeader("Content-Type", "application/json");

            var req = new RestRequest(_configuration.GetTokenEnpoint);
            var response = await Task.FromResult(_client.Post<ThailandpostTrackingGetTokenResponseDTO>(req));
            Log.Information("[GetToken] - response data {@response}", response);

            var token = response?.token;
            return token;
        }

        private async Task<GetItemsbyBarcodeResponseDTO> GetItemsbyBarcode(GetItemsbyBarcodeRequestDTO input)
        {
            var token = await GetToken();

            while (_client.DefaultParameters.Any(p => p.Name == "Authorization"))
                _client.DefaultParameters.RemoveParameter(_client.DefaultParameters.First(p => p.Name == "Authorization"));

            _client.AddDefaultHeader("Authorization", "Token " + token);

            var req = new RestRequest(_configuration.GetItemsbyBarcodeEnpoint);
            req.AddBody(input);
            var response = await Task.FromResult(_client.Post<GetItemsbyBarcodeResponseDTO>(req));
            string jsonString = JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });

            Log.Information("[GetItemsbyBarcode] - response data Message {@Message}", response.Message);

            return response;
        }

        // Convert Buddhist Era date (ปีพ.ศ. 2568) to Gregorian (ค.ศ. 2025)
        private DateTime ParseBuddhistDate(string dateString)
        {
            try
            {
                var format = "dd/MM/yyyy HH:mm:sszzz"; // Example: 28/02/2568 10:09:25+07:00
                var culture = new CultureInfo("th-TH"); // Thai culture (Buddhist Era)

                // Convert BE 2568 → CE 2025
                DateTime parsedDate = DateTime.ParseExact(dateString, format, culture);
                return parsedDate.AddYears(-543);
            }
            catch
            {
                return DateTime.MinValue; // Fallback for errors
            }
        }

        public async Task<ServiceResponseWithPagination<List<GetTrackingHeaderResponseDTO>>> GetTrackingHeader(GetTrackingHeaderRequestDTO filter)
        {
            try
            {
                var data = _dbContext.TrackingHeaders
                    .Include(x => x.TmpImportTracking)
                    .Include(x => x.TrackingBatch)
                    .OrderByDescending(x => x.UpdatedDate).ThenByDescending(x => x.TrackingCode).AsQueryable();

                if (!string.IsNullOrWhiteSpace(filter.TrackingCode))
                {
                    data = data.Where(x => x.TrackingCode == filter.TrackingCode);
                }

                //Ordering
                if (!string.IsNullOrWhiteSpace(filter.OrderingField))
                {
                    try
                    {
                        data = data.OrderBy($"{filter.OrderingField} {(filter.AscendingOrder ? "ascending" : "descending")}");
                    }
                    catch (Exception e)
                    {
                        return ResponseResultWithPagination.Failure<List<GetTrackingHeaderResponseDTO>>($"Could not order by field: {filter.OrderingField}");
                    }
                }

                //Pagination
                var paginationResult = await _httpContext.HttpContext.InsertPaginationParametersInResponse(data, filter.RecordsPerPage, filter.Page);
                var dto = await data.Paginate(filter).ToListAsync();

                //mapping dto response
                var dtoOutput = _mapper.Map<List<GetTrackingHeaderResponseDTO>>(dto);

                return ResponseResultWithPagination.Success(dtoOutput, paginationResult, "Success");
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, "[GetTrackingHeader] - An error occurred");
                return ResponseResultWithPagination.Failure<List<GetTrackingHeaderResponseDTO>>(ex.Message);
            }
        }

        public async Task<List<GetTimeLineResponseDTO>> GetTrackingDetail(GetTrackingDetailRequestDTO filter)
        {
            var data = _dbContext.TrackingHeaders
                       .Include(x => x.TrackingDetails.Where(x => x.TrackingHeaderId == filter.TrackingHeaderId && x.TrackingBatchId == filter.TrackingBatchId).OrderByDescending(d => d.Status_Date))
                       .Include(x => x.TmpImportTracking)
                       .Include(x => x.TrackingBatch)
                       .Where(x => x.TrackingHeaderId == filter.TrackingHeaderId
                        && x.TrackingBatchId == filter.TrackingBatchId
                        && x.IsActive == true)
                       .OrderByDescending(_ => _.Status_Date).AsQueryable();

            //mapping dto response
            var dtoOutput = _mapper.Map<List<GetTimeLineResponseDTO>>(data);

            return dtoOutput;
        }

        public async Task<ServiceResponse<ReportReponseDto>> GetReportTracking()
        {
            try
            {
                _logger.Information("[ReportTracking]- Start Date: {Date} Procedures.usp_ReportTracking_SelectAsync");

                var result = await _dbContext.Procedures.usp_ReportTracking_SelectAsync();

                if (result.Count == 0)
                {
                    throw new Exception("ไม่พบข้อมูล");
                }

                var dtoOut = _mapper.Map<List<usp_ReportTracking_SelectResult>>(result);

                string fileName = $"รายTracking_{DateTime.Now:ddMMyyyy_HHmmss}.xlsx";

                var excelService = new NPOIExcelExportService();

                excelService.AddSheetHeader(dtoOut, "รายTracking", _trackingInfo);

                var response = new ReportReponseDto
                {
                    Data = excelService.GetFile(),
                    IsResult = true,
                    Message = "Success",
                    FileName = fileName,
                };

                return ResponseResult.Success(response, "Success");
            }
            catch (Exception e)
            {
                _logger.Error(e, "[ReportTracking] - An error occurred , {Msg}", e.Message);
                return ResponseResult.Failure<ReportReponseDto>(e.Message);
            }
        }
    }
}