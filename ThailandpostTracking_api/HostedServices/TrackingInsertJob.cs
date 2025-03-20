using Microsoft.Extensions.Options;
using Quartz;
using Serilog;
using ThailandpostTracking.Configurations;
using ThailandpostTracking.Data;
using ThailandpostTracking.DTOs.Thailandpost.Request;
using ThailandpostTracking.Services.ThailandpostTracking;

namespace ThailandpostTracking.HostedServices
{
    public class TrackingInsertJob : IJob
    {
        private readonly IThailandpostTrackingServices _services;
        private readonly AppDBContext _dbContext;
        private readonly ThailandpostTrackingSetting _configuration;

        public TrackingInsertJob(IThailandpostTrackingServices services, AppDBContext dbContext, IOptions<ThailandpostTrackingSetting> configuration)
        {
            _services = services;
            _dbContext = dbContext;
            _configuration = configuration.Value;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var tmpImportTrackings = _dbContext.TmpImportTrackings.Where(x => x.IsInsert == null && x.IsResult == null).OrderBy(a => a.TmpImportTrackingId).Take(_configuration.TakeInSert).ToList();
            var input = new GetItemsbyBarcodeRequestDTO
            {
                Status = "all",
                Language = "TH",
                Barcode = tmpImportTrackings.Select(x => x.TrackingCode).ToList()
            };
            Log.Information("TrackingInsertJob Start");
            Log.Information("TrackingInsertJob Count: {@Count}", tmpImportTrackings.Count);
            if (tmpImportTrackings.Count > 0)
                await _services.InsertTracking(input);
            Log.Information("TrackingInsertJob Successfully");
        }
    }
}