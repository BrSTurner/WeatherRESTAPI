using FluentValidation;
using Microsoft.Extensions.Options;
using Nexer.Domain.Models.Configurations;
using Nexer.Domain.Models.ValidationModels;

namespace Nexer.Domain.Validations
{
    public class GetDataValidation : AbstractValidator<GetDataValidationModel>
    {
        public GetDataValidation(IOptions<WeatherConfiguration> weatherConfigurationOptions)
        {
            RuleFor(x => x.Date)
                .NotEmpty().WithMessage("Date cannot be empty")
                .Matches(@"^(\d{4})-(\d{2})-(\d{2})$").WithMessage("Date is not in correct format, try yyyy-mm-dd");                          

            RuleFor(x => x.DeviceId)
                .NotEmpty().WithMessage("Device Id cannot be empty");

            RuleSet("GetDataBySensorType", () =>
            {
                RuleFor(x => x.SensorType)
                   .NotEmpty().WithMessage("Sensor Type cannot be empty")
                   .Must(x => weatherConfigurationOptions.Value.SensorTypes.Contains(x.ToLower())).WithMessage("The Sensor Type is invalid for this operation");
            });
        }

    }
}
