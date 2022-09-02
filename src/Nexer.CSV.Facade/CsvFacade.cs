using CsvHelper;
using CsvHelper.Configuration;
using Nexer.CSV.Facade.Mapping;
using Nexer.Domain.Helpers;
using Nexer.Domain.Interfaces.CSVFacade;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Nexer.CSV.Facade
{
    public class CsvFacade : ICsvFacade
    {
        public IEnumerable<T> GetRecords<T>(Stream fileContent, bool hasHeaderRecord = false, bool hasMapping = true, string delimiter = ";")
        {
            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = hasHeaderRecord,
                Delimiter = delimiter
            };

            IList<T> records = null;

            using (var memStream = new MemoryStream())
            {
                fileContent.CopyTo(memStream);

                memStream.Seek(0, SeekOrigin.Begin);

                using (var streamReader = new StreamReader(memStream))
                using (var csvReader = new CsvReader(streamReader, configuration))
                {
                    RegisterMap<T>(csvReader);
                    records = csvReader.GetRecords<T>().ToList();
                }
            }

            return records;
        }


        private void RegisterMap<T>(CsvReader csvReader)
        {
            var childClasses = ReflectionHelper.GetInheritedClasses<IClassMap>();
            var classToBeMapped = childClasses.FirstOrDefault(x => x.Name.ToLower().StartsWith(typeof(T).Name.ToLower()));

            if (classToBeMapped != null)
                csvReader.Context.RegisterClassMap(classToBeMapped);
        } 
    }
}
