using bp.Pomocne.DTO;
using System;

namespace bp.Pomocne.DateHelp
{
    public static partial class DateHelpful
    {
            public static DateTime DataStalaGodzina(DateTime d, int godz = 9)
            {
                return new DateTime(d.Year, d.Month, d.Day, godz, 0, 0);
            }

            public static DateTime Kwartal()
            {
                DateTime kwartalTemu = DateTime.Now.AddMonths(-3);
                return new DateTime(kwartalTemu.Year, kwartalTemu.Month, 1);
            }

            public static DateRangeDTO DateRangeOstKwartal()
            {
                return new DateRangeDTO
                {
                    DateEnd = DateTime.Now,
                    DateStart = Kwartal()
                };
            }
    }
}