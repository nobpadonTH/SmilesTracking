using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThailandpostTracking.DTOs.Thailandpost.Request;
using ThailandpostTracking.DTOs.Thailandpost.Response;
using ThailandpostTracking.Models;

namespace ThailandpostTracking.Services.ThailandpostTracking
{
    public interface IThailandpostTrackingServices
    {
        Task<GetItemsbyBarcodeResponseDTO> InsertTracking(GetItemsbyBarcodeRequestDTO input);

        Task<GetItemsbyBarcodeResponseDTO> UpsertTracking(GetItemsbyBarcodeRequestDTO input);

        Task<GetRequestItemsResponseDTO> GetRequestItems(GetItemsbyBarcodeRequestDTO input);
    }
}