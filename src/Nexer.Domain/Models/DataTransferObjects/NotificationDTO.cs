using Nexer.Domain.Models.Enumerations;

namespace Nexer.Domain.Models.DataTransferObjects
{
    public class NotificationDTO
    {
        public string Message { get; set; }
        public NotificationTypeEnum Type { get; set; }
    }
}
