namespace ThailandpostTracking.DTOs.Thailandpost.Response
{
    public class Response
    {
        public List<object> Items { get; set; }
        public TrackCount TrackCount { get; set; }
    }

    public class GetRequestItemsResponseDTO
    {
        public Response Response { get; set; }
        public string Message { get; set; }
        public bool Status { get; set; }
    }
}