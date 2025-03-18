using Quartz;
using Serilog;
using ThailandpostTracking.Data;
using ThailandpostTracking.DTOs.Thailandpost.Request;
using ThailandpostTracking.Services.ThailandpostTracking;

namespace ThailandpostTracking.HostedServices
{
    public class TrackingInsertJob : IJob
    {
        private readonly IThailandpostTrackingServices _services;
        private readonly AppDBContext _dbContext;

        public TrackingInsertJob(IThailandpostTrackingServices services, AppDBContext dbContext)
        {
            _services = services;
            _dbContext = dbContext;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var tmpImportTrackings = _dbContext.TmpImportTrackings.Where(x => x.IsInsert == null && x.IsResult == null).OrderBy(a => a.TmpImportTrackingId).Take(100).ToList();
            var input = new GetItemsbyBarcodeRequestDTO
            {
                Status = "all",
                Language = "TH",
                Barcode = tmpImportTrackings.Select(x => x.TrackingCode).ToList()
            };
            Log.Information("TrackingInsertJob Start");
            //await _services.GetItemsbyBarcode(input);
            Log.Information("TrackingInsertJob Successfully");
        }
    }
}