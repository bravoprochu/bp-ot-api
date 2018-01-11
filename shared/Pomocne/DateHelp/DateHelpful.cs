using bp.Pomocne.DTO;
using System;

namespace bp.Pomocne.DateHelp
{
    public static partial class DateHelpful
    {
        public static string DateFormatYYYYMMDD(DateTime date)
        {
            return date.ToString("yyyy-MM-dd");
        }
        public static DateTime DataStalaGodzina(DateTime d, int godz = 9)
        {
            return new DateTime(d.Year, d.Month, d.Day, godz, 0, 0);
        }

        public static DateTime Kwartal()
        {
            DateTime kwartalTemu = DateTime.Now.AddMonths(-3);
            return new DateTime(kwartalTemu.Year, kwartalTemu.Month, 1);
        }

        public static DateTime DateRangeDateTo(DateTime dateEnd)
        {
            return new DateTime(dateEnd.Year, dateEnd.Month, dateEnd.Day, 23, 59, 59);
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