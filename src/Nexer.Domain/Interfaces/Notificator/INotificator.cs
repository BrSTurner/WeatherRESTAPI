using FluentValidation.Results;
using Nexer.Domain.Models.DataTransferObjects;
using Nexer.Domain.Models.Enumerations;
using System.Collections.Generic;

namespace Nexer.Domain.Interfaces.Notificator
{
    public interface INotificator
    {
        void AddMessage(NotificationDTO notification);
        void AddMessage(string message, NotificationTypeEnum notificationType = NotificationTypeEnum.Error);
        void AddSuccessMessage(string message);
        void AddErrorMessage(string message);
        void AddInformationMessage(string message);
        void AddWarningMessage(string message);
        void AddValidationErrorMessages(List<ValidationFailure> errorMessages);
        IReadOnlyList<NotificationDTO> GetNotifications();
        bool HasErrors();
        bool HasNotifications();
    }
}
