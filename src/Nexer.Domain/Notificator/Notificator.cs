using FluentValidation.Results;
using Nexer.Domain.Interfaces.Notificator;
using Nexer.Domain.Models.DataTransferObjects;
using Nexer.Domain.Models.Enumerations;
using System.Collections.Generic;
using System.Linq;

namespace Nexer.Domain.Notificator
{
    public class Notificator : INotificator
    {
        private readonly IList<NotificationDTO> _notifications;

        public Notificator()
        {
            _notifications = new List<NotificationDTO>(); 
        }

        public void AddMessage(NotificationDTO notification)
        {
            _notifications.Add(notification);
        }

        public void AddMessage(string message, NotificationTypeEnum notificationType = NotificationTypeEnum.Error)
        {
            if (string.IsNullOrEmpty(message))
                return;

            AddMessage(new NotificationDTO { Message = message, Type = notificationType });
        }

        public void AddSuccessMessage(string message)
        {
            AddMessage(message, NotificationTypeEnum.Success);
        }

        public void AddErrorMessage(string message)
        {
            AddMessage(message, NotificationTypeEnum.Error);
        }

        public void AddInformationMessage(string message)
        {
            AddMessage(message, NotificationTypeEnum.Information);
        }

        public void AddWarningMessage(string message)
        {
            AddMessage(message, NotificationTypeEnum.Warning);
        }

        public void AddValidationErrorMessages(List<ValidationFailure> errorMessages)
        {
            errorMessages.ForEach(message =>
            {
                AddErrorMessage(message.ErrorMessage);
            });
        }

        public IReadOnlyList<NotificationDTO> GetNotifications() => _notifications.ToList();

        public bool HasErrors() => _notifications.Any(x => x.Type == NotificationTypeEnum.Error);

        public bool HasNotifications() => _notifications.Any();

    
    }
}
