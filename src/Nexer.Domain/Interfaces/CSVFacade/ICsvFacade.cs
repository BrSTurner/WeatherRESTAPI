using System.Collections.Generic;
using System.IO;

namespace Nexer.Domain.Interfaces.CSVFacade
{
    public interface ICsvFacade
    {
        IEnumerable<T> GetRecords<T>(Stream fileContent, bool hasHeaderRecord = false, bool hasMapping = true, string delimiter = ";");
    }
}
