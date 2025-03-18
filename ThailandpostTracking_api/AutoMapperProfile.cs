using AutoMapper;
using ThailandpostTracking.DTOs.Thailandpost.Response;
using ThailandpostTracking.Models;

namespace ThailandpostTracking
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<TrackingItem, TrackingDetail>();
            CreateMap<TrackingHeader, GetTrackingHeaderResponseDTO>();
            CreateMap<TrackingDetail, GetTrackingDetailResponseDTO>();
        }
    }
}