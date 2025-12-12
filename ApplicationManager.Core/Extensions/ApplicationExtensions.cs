using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationManager.Core.Enums;
using ApplicationManager.Core.Models;

// REIKALAVIMAS: Praplėsti C# tipai (0.5 t.)
// Sukurti ir naudojami extension metodai.

// REIKALAVIMAS: Pritaikytas savas bendrasis tipas naudojant 'where' (1 t.)
// Naudojami 'where T : class' ir 'where T : ICloneable' apribojimai.

namespace ApplicationManager.Core.Extensions
{
    public static class ApplicationExtensions
    {
        public static IEnumerable<T> WithFlag<T>(this IEnumerable<T> source, Func<T, bool> predicate)
            where T : class
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            return source.Where(predicate);
        }

        public static IEnumerable<Application> WithFlag(this IEnumerable<Application> source, ApplicationFlags flag)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            return source.Where(a => (a.Flags & flag) == flag);
        }

        public static List<T> CloneAll<T>(this IEnumerable<T> source)
            where T : ICloneable
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return source.Select(x => (T)x.Clone()).ToList();
        }
    }
}
