using AutoMapper;
using Confluent.Kafka;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RestSharp;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using ThailandpostTracking.Configurations;
using ThailandpostTracking.Data;
using ThailandpostTracking.DTOs.Thailandpost.Request;
using ThailandpostTracking.DTOs.Thailandpost.Response;
using ThailandpostTracking.Helpers;
using ThailandpostTracking.Models;
using ThailandpostTracking.Services.Auth;
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

        public ThailandpostTrackingServices(IOptions<ThailandpostTrackingSetting> configuration, AppDBContext dbContext, IMapper mapper, IHttpContextAccessor httpContext)
        {
            _client = new RestClient();
            _configuration = configuration.Value;
            _dbContext = dbContext;
            _mapper = mapper;
            _httpContext = httpContext;
            _logger = Log.ForContext<ThailandpostTrackingServices>();
        }

        public async Task<GetRequestItemsResponseDTO> GetRequestItems(GetItemsbyBarcodeRequestDTO input)
        {
            var token = await GetToken();

            while (_client.DefaultParameters.Any(p => p.Name == "Authorization"))
                _client.DefaultParameters.RemoveParameter(_client.DefaultParameters.First(p => p.Name == "Authorization"));

            _client.AddDefaultHeader("Authorization", "Token " + token);

            var req = new RestRequest("https://trackapi.thailandpost.co.th/post/api/v1/track/batch");
            req.AddBody(input);
            var response = await Task.FromResult(_client.Post<GetRequestItemsResponseDTO>(req));

            return response;
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

                    var tmpImportTracking = _dbContext.TmpImportTrackings.FirstOrDefault(x => x.TrackingCode == track);
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

                    var tmpImportTracking = _dbContext.TmpImportTrackings.FirstOrDefault(x => x.TrackingCode == track);
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

                        var dataInsert = _dbContext.TrackingHeaders.Where(x => x.IsActive == true && x.TrackingCode == track).SingleOrDefault();
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

            #region xxx

            //if (response.Status)
            //{
            //    foreach (var track in input.Barcode)
            //    {
            //        var jsonObject = response?.Response.Items[track];

            //        var tmpImportTracking = _dbContext.TmpImportTrackings.FirstOrDefault(x => x.Tracking == track);
            //        if (jsonObject?.Count != 0)
            //        {
            //            var headerId = Guid.NewGuid();

            //            var selectList = jsonObject?
            //                .Select(item => new
            //                {
            //                    Item = item,
            //                    ParsedDate = ParseBuddhistDate(item.Status_Date)
            //                })
            //                .OrderByDescending(x => x.ParsedDate)
            //                .FirstOrDefault();

            //            var dataInsert = new TrackingHeader
            //            {
            //                TrackingHeaderId = headerId,
            //                TrackingBatchId = batchId,
            //                TrackingCode = track,
            //                Status = selectList?.Item.Status,
            //                Status_Description = selectList?.Item.Status_Description,
            //                Status_Date = ParseBuddhistDate(selectList.Item.Status_Date),
            //                Location = selectList?.Item.Location,
            //                Postcode = selectList?.Item.Postcode,
            //                Delivery_Description = selectList?.Item.Delivery_Description,
            //                Delivery_Datetime = ParseBuddhistDate(selectList.Item.Delivery_Datetime),
            //                Signature = selectList?.Item.Signature,
            //                Receiver_Name = selectList?.Item.Receiver_Name,
            //                Delivery_Officer_Name = selectList?.Item.Delivery_Officer_Name,
            //                Delivery_Officer_Tel = selectList?.Item.Delivery_Officer_Tel,
            //                Office_Name = selectList?.Item.Office_Name,
            //                Office_Tel = selectList?.Item.Office_Tel,
            //                Call_Center_Tel = selectList?.Item.Call_Center_Tel,
            //                Delivery_Status = selectList?.Item.Delivery_Status,
            //                IsActive = true,
            //                CreatedDate = now,
            //                CreatedByUserId = 1,
            //                UpdatedDate = now,
            //                UpdatedByUserId = 1
            //            };
            //            _dbContext.Add(dataInsert);

            //            var trackingItem = _mapper.Map<List<TrackingDetail>>(jsonObject);

            //            trackingItem.ForEach(x =>
            //            {
            //                x.TrackingDetailId = Guid.NewGuid();
            //                x.TrackingBatchId = batchId;
            //                x.TrackingHeaderId = headerId;
            //                x.Status_Date = x.Status_Date?.AddYears(-543);
            //                x.Delivery_Datetime = x.Delivery_Datetime?.AddYears(-543);
            //                x.IsActive = true;
            //                x.CreatedDate = now;
            //                x.CreatedByUserId = 1;
            //                x.UpdatedDate = now;
            //                x.UpdatedByUserId = 1;
            //            });

            //            await _dbContext.AddRangeAsync(trackingItem);

            //            // Ensure tmpImportTracking is not null before updating
            //            if (tmpImportTracking != null)
            //            {
            //                tmpImportTracking.IsInsert = true;
            //                tmpImportTracking.IsResult = true;
            //                tmpImportTracking.TransactionDate = now;
            //                tmpImportTracking.Message = "บันทึกสำเร็จ";

            //                _dbContext.Update(tmpImportTracking);
            //            }
            //        }
            //        else
            //        {
            //            // Ensure tmpImportTracking is not null before updating
            //            if (tmpImportTracking != null)
            //            {
            //                tmpImportTracking.IsInsert = false;
            //                tmpImportTracking.IsResult = false;
            //                tmpImportTracking.TransactionDate = now;
            //                tmpImportTracking.Message = "บันทึกไม่สำเร็จ";

            //                _dbContext.Update(tmpImportTracking);
            //            }
            //        }
            //    }
            //    _dbContext.Add(dataBatch);

            //    await _dbContext.SaveChangesAsync();
            //}

            #endregion xxx

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
                var data = _dbContext.TrackingHeaders.AsQueryable();
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

        public async Task<ServiceResponseWithPagination<List<GetTrackingDetailResponseDTO>>> GetTrackingDetail(GetTrackingDetailRequestDTO filter)
        {
            try
            {
                var data = _dbContext.TrackingDetails.Where(x => x.TrackingHeaderId == filter.TrackingHeaderId && x.TrackingBatchId == filter.TrackingBatchId && x.IsActive == true).OrderBy(_ => _.Status_Date).AsQueryable();

                //Ordering
                if (!string.IsNullOrWhiteSpace(filter.OrderingField))
                {
                    try
                    {
                        data = data.OrderBy($"{filter.OrderingField} {(filter.AscendingOrder ? "ascending" : "descending")}");
                    }
                    catch (Exception e)
                    {
                        return ResponseResultWithPagination.Failure<List<GetTrackingDetailResponseDTO>>($"Could not order by field: {filter.OrderingField}");
                    }
                }

                //Pagination
                var paginationResult = await _httpContext.HttpContext.InsertPaginationParametersInResponse(data, filter.RecordsPerPage, filter.Page);
                var dto = await data.Paginate(filter).ToListAsync();

                //mapping dto response
                var dtoOutput = _mapper.Map<List<GetTrackingDetailResponseDTO>>(dto);

                return ResponseResultWithPagination.Success(dtoOutput, paginationResult, "Success");
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, "[GetTrackingDetail] - An error occurred");
                return ResponseResultWithPagination.Failure<List<GetTrackingDetailResponseDTO>>(ex.Message);
            }
        }
    }
}