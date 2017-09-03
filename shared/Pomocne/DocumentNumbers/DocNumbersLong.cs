using System;
using System.Collections.Generic;
using System.Linq;

namespace bp.Pomocne.DocumentNumbers
{
    public class DocNumbersLong
    {
        public DocNumbersLong(string[] numeryDokumentow, char separator='/')
        {
            this.Docs = new List<DocNumberLong>();
            this.Separator = separator;
            this.PrepStringArr(numeryDokumentow);

        }

        public List<DocNumberLong> Docs { get; set; }
        private void PrepStringArr(string[] numeryDokumentow)
        {
            foreach (var numer in numeryDokumentow)
            {
                Docs.Add(DocNumbersMethods.ParseDocNumberLong(numer,Separator));
            }
        }
        public DocNumberLong LastNumberInMonth(DateTime dataWystawienia)
        {
            var monthDocs = new List<DocNumberLong>();
            foreach (var doc in Docs)
            {
                if (doc.Miesiac == dataWystawienia.Month && doc.Rok == dataWystawienia.Year)
                {
                    monthDocs.Add(doc);
                }
            }

            if (monthDocs.Count > 0) {
                return monthDocs.OrderByDescending(o => o.Nr).FirstOrDefault();
            }

            var _przedrostek = Docs.Count > 0 ? Docs.FirstOrDefault().Przedrostek : null;

            return new DocNumberLong(Separator)
            {
                Przedrostek = _przedrostek,
                Nr = 0,
                Miesiac = dataWystawienia.Month,
                Rok = dataWystawienia.Year
            };
        }
        private char Separator { get; set; }
    }
}