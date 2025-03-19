using Microsoft.Extensions.Options;
using Quartz;
using Serilog;
using ThailandpostTracking.Configurations;
using ThailandpostTracking.Data;
using ThailandpostTracking.DTOs.Thailandpost.Request;
using ThailandpostTracking.Services.ThailandpostTracking;

namespace ThailandpostTracking.HostedServices
{
    public class TrackingUpsertJob : IJob
    {
        private readonly IThailandpostTrackingServices _services;
        private readonly AppDBContext _dbContext;
        private readonly ThailandpostTrackingSetting _configuration;

        public TrackingUpsertJob(IThailandpostTrackingServices services, AppDBContext dbContext, IOptions<ThailandpostTrackingSetting> configuration)
        {
            _services = services;
            _dbContext = dbContext;
            _configuration = configuration.Value;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var tmpImportTrackings = _dbContext.TrackingHeaders.Where(x => x.IsActive == true && x.Status != "501").OrderBy(a => a.UpdatedDate).Take(_configuration.TakeUpSert).ToList();
            var input = new GetItemsbyBarcodeRequestDTO
            {
                Status = "all",
                Language = "TH",
                Barcode = tmpImportTrackings.Select(x => x.TrackingCode).ToList()
            };
            Log.Information("TrackingUpsertJob Start");
            if (tmpImportTrackings.Count > 0)
                await _services.UpsertTracking(input);
            Log.Information("TrackingUpsertJob Successfully");
        }
    }
}