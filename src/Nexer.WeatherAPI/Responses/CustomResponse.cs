using Nexer.Domain.Models.DataTransferObjects;
using System.Collections.Generic;

namespace Nexer.WeatherAPI.Responses
{
    public class CustomResponse
    {
        public bool Success { get; set; }
        public IReadOnlyList<NotificationDTO> Notifications { get; set; }
        public object Data { get; set; }
    }

    public class CustomResponse<T>
    {
        public bool Success { get; set; }
        public IReadOnlyList<NotificationDTO> Notifications { get; set; }
        public T Data { get; set; }
    }
}
