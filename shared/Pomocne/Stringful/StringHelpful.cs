﻿using System.Collections.Generic;
using System.Linq;

namespace bp.Pomocne.StringHelp
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

        public static string SeparatorEvery(string sourceString, int separatorEvery = 4, char separatorType = ' ')
        {
            string result = "";
            var sourceArr = sourceString.ToCharArray();

            int counter = 0;
            while (counter <= sourceArr.Length - 1)
            {
                for (int i = 0; i < separatorEvery + 1; i++)
                {
                    if (counter == sourceArr.Length - 1) {
                        counter++;
                        break;
                    }
                    if (i == separatorEvery)
                    {
                        result += separatorType;
                    }
                    else
                    {
                        result += sourceArr[counter];
                        counter++;
                    }

                }
            }
            return result;
        }

    }
}