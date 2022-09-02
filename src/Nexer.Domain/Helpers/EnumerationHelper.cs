using System;

namespace Nexer.Domain.Helpers
{
    public static class EnumerationHelper
    {
        public static T GetEnumValue<T>(string name) where T : struct, IConvertible
        {
            return Enum.TryParse(name, true, out T val) ? val : default;
        }
    }
}
