using System;
using System.Collections.Generic;
using System.Linq;

namespace bp.Pomocne.DocumentNumbers
{
    public class DocNumbersShort
    {
        public DocNumbersShort(List<string> numeryDokumentow, char separator='/')
        {
            this.Docs = new List<DocNumberShort>();
            this.Separator = separator;
            this.PrepStringArr(numeryDokumentow);

        }

        public List<DocNumberShort> Docs { get; set; }
        private void PrepStringArr(List<string> numeryDokumentow)
        {
            foreach (var numer in numeryDokumentow)
            {
                Docs.Add(DocNumbersMethods.ParseDocNumberShort(numer,Separator));
            }
        }
        public DocNumberShort LastNumberInYear(DateTime dataWystawienia)
        {
            var docsInYear = this.Docs.Where(w => w.Rok == dataWystawienia.Year).OrderByDescending(o => o.Nr).Select(s => s).ToList();
            if (docsInYear.Count>0) return docsInYear.FirstOrDefault();

            var _przedrostek = Docs.Count > 0 ? Docs.FirstOrDefault().Przedrostek : null;
            return new DocNumberShort(Separator)
            {
                Przedrostek = _przedrostek,
                Nr = 0,
                Rok = dataWystawienia.Year
            };
        }
        private char Separator { get; set; }
    }
}