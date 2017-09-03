using System.Collections.Generic;
using System.Linq;

namespace Andpol.Dane.Pomocne
{
    public static partial class StringHelpful
    {
        public static List<string> StringListGroup(List<string> nazwy)
        {
            var result = new List<string>();

            var grupy = nazwy.GroupBy(g => g).Select(s => new {
                Ilosc = s.Count(),
                Nazwa = s.FirstOrDefault()
            }).ToList();

            foreach (var item in grupy)
            {
                result.Add($"{item.Ilosc}x {item.Nazwa}");
            }
            return result;
        }
    }
}