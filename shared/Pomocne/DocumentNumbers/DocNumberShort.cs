using System;

namespace bp.Pomocne.DocumentNumbers
{
    public class DocNumberShort
    {
        public DocNumberShort()
        {
            this.Separator = '/';
        }
        public DocNumberShort(char _separator)
        {
            this.Separator = _separator;
        }
        public int Nr { get; set; }
        public string Przedrostek { get; set; }
        public int Rok { get; set; }
        public char Separator { get; set; }
        public string NumerDokumentuFullName { get {
                var _przedrostek = String.IsNullOrWhiteSpace(Przedrostek) ? null : Przedrostek + Separator;
                return $"{_przedrostek}{this.Nr}{this.Separator}{this.Rok}";
            } }
    }
}