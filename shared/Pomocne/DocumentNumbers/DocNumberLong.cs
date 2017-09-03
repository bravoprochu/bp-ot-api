using System;

namespace bp.Pomocne.DocumentNumbers
{
    public class DocNumberLong
    {
        public DocNumberLong()
        {
            this.Separator = '/';
        }

        public DocNumberLong(char separator)
        {
            this.Separator = separator;
        }
        public int Nr { get; set; }

        private int _miesiac;

        public int Miesiac
        {
            get { return _miesiac; }
            set { if (value < 13 && value > 0)
                {
                    _miesiac = value;
                }
                else {
                    _miesiac = 1;
                } }
        }
        public string Przedrostek { get; set; }
        public int Rok { get; set; }
        public string FullString() {
            var _przedrostek = String.IsNullOrWhiteSpace(Przedrostek) ? null : Przedrostek + Separator;
            return $"{_przedrostek}{Nr}{Separator}{Miesiac}{Separator}{Rok}";
            }
        private char Separator { get; set; }
    }
}