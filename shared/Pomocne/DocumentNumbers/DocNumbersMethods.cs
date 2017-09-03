using System;
using System.Collections.Generic;
using System.Linq;

namespace bp.Pomocne.DocumentNumbers
{
    public static class DocNumbersMethods
    {

        public static DocNumberLong NextDocNumberLong(string[] actDocsNumbers, DateTime dataWystawienia, char separator='/')
            {
            var docs = new DocNumbersLong(actDocsNumbers, separator);
            var last=docs.LastNumberInMonth(dataWystawienia);
            last.Nr ++;

            return last;
        }

        public static DocNumberLong ParseDocNumberLong(string actDocNumber, char separator='/')
        {

            var res = new DocNumberLong(separator);
            string[] arr = actDocNumber.Split(separator);
            int arrCount = arr.Length > 3 ? 1 : 0;

            if (arr.Length == 0) {
                return new DocNumberLong(separator)
                {
                    Miesiac = 1,
                    Nr = 0,
                    Przedrostek = null,
                    Rok = 1900
                };
            }

            string przedrostek = arr.Length > 3 ? arr[0] : null;
            int numer = 0;
            bool resNnumer = Int32.TryParse(arr[0+arrCount], out numer);
            int miesiac = 0;
            bool resMiesiac = Int32.TryParse(arr[1+arrCount], out miesiac);
            int rok = 0;
            bool resRok = Int32.TryParse(arr[2+arrCount], out rok);

            if (resNnumer && resMiesiac && resRok) {

                res.Nr = numer;
                res.Miesiac = miesiac;
                res.Przedrostek = przedrostek;
                res.Rok = rok;
            }
            return res;
        }

        public static DocNumberShort NextDocNumberShort(List<string> actDocsNumbers, DateTime dataWystawienia, char separator = '/', string przedrostek=null)
        {

            if (actDocsNumbers==null || actDocsNumbers.Count==0) {
                actDocsNumbers = new List<string>();
                actDocsNumbers.Add($"{przedrostek}/0/{dataWystawienia.Year}");
            }

            var docs = new DocNumbersShort(actDocsNumbers.ToList(), separator);
            var last = docs.LastNumberInYear(dataWystawienia);
            last.Nr++;

            return last;
        }

        public static DocNumberShort ParseDocNumberShort(string actDocNumber, char separator = '/')
        {
            var res = new DocNumberShort(separator);
            string[] arr = actDocNumber.Split(separator);
            int arrCount = arr.Length > 2 ? 1 : 0;

            string przedrostek = arr.Length > 2 ? arr[0] : null;
            int numer = 0;
            bool resNnumer = Int32.TryParse(arr[0 + arrCount], out numer);
            int rok = 0;
            bool resRok = Int32.TryParse(arr[1 + arrCount], out rok);

            if (resNnumer && resRok)
            {

                res.Nr = numer;
                res.Przedrostek = przedrostek;
                res.Rok = rok;
            }
            return res;


        }
        

    }
}