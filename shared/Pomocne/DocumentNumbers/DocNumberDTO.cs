using System;
using System.Collections.Generic;
using System.Text;

namespace bp.Pomocne.DocumentNumbers
{
    public class DocNumberDTO
    {
        public DocNumberDTO(char separator='/')
        {
            this.Separator = separator;
        }
        public string DocNumberCombined { get {
                var resList = new List<string>();
                if (!string.IsNullOrWhiteSpace(this.Prefix)) { resList.Add(Prefix); }
                if (this.DocNumber > 0) { resList.Add(DocNumber.ToString()); }
                if (this.DocMonth > 0){ resList.Add(DocMonth.ToString()); }
                if (this.DocYear > 0) { resList.Add(DocYear.ToString());}

                return string.Join(this.Separator.ToString(), resList);
            }
        }
        public string Prefix { get; set; }
        public int DocNumber { get; set; }
        public int DocMonth { get; set; }
        public int DocYear { get; set; }
        public char Separator { get; set; }

    }
}
