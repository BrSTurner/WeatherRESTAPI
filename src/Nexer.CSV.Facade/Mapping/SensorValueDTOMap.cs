using CsvHelper.Configuration;
using Nexer.Domain.Models.DataTransferObjects;

namespace Nexer.CSV.Facade.Mapping
{
    public class SensorValueDTOMap : ClassMap<SensorValueDTO>, IClassMap
    {
        public SensorValueDTOMap()
        {
            MapClass();
        }

        public void MapClass()
        {
            Map(x => x.Date).Index(0);
            Map(x => x.Value).Index(1);
            Map(x => x.Type).Ignore();
            Map(x => x.NumericValue).Ignore();
        }
    }
}
