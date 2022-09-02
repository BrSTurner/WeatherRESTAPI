using System;
using System.Collections.Generic;
using System.Linq;

namespace Nexer.Domain.Helpers
{
    public static class ReflectionHelper
    {
        public static IEnumerable<Type> GetInheritedClasses<T>()
        {
            return GetInheritedClasses(typeof(T));
        }

        public static IEnumerable<Type> GetInheritedClasses(Type type)
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(p => type.IsAssignableFrom(p));
        }
    }
}
