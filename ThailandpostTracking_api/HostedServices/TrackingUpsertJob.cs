using Quartz;
using Serilog;
using ThailandpostTracking.Data;
using ThailandpostTracking.DTOs.Thailandpost.Request;
using ThailandpostTracking.Services.ThailandpostTracking;

namespace ThailandpostTracking.HostedServices
{
    public class TrackingUpsertJob : IJob
    {
        private readonly IThailandpostTrackingServices _services;
        private readonly AppDBContext _dbContext;

        public TrackingUpsertJob(IThailandpostTrackingServices services, AppDBContext dbContext)
        {
            _services = services;
            _dbContext = dbContext;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var tmpImportTrackings = _dbContext.TrackingHeaders.Where(x => x.IsActive == true).OrderBy(a => a.UpdatedDate).Take(10).ToList();
            var input = new GetItemsbyBarcodeRequestDTO
            {
                Status = "all",
                Language = "TH",
                Barcode = tmpImportTrackings.Select(x => x.TrackingCode).ToList()
            };
            Log.Information("TrackingUpsertJob Start");
            await _services.UpsertTracking(input);
            Log.Information("TrackingUpsertJob Successfully");
        }
    }
}