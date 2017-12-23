using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bp.Pomocne.DocumentNumbers
{
    public partial class DocNumber
    {
        public DocNumberDTO GenNumberMonthYearNumber(string LastDocNumber, DateTime date, char separator='/')
        {
            var lastDoc = this.ParseDocNumber(LastDocNumber, separator);
            return this.NextNumber(lastDoc, date);


        }

        private DocNumberDTO ParseDocNumber(string actDocNo, char separator = '/')
        {

            var res = new DocNumberDTO(separator);
            if (string.IsNullOrWhiteSpace(actDocNo)) return this.ZeroDocNumber(res);

            string[] arr = actDocNo.Split(separator);
            int arrCount = arr.Length > 3 ? 1 : 0;

            if (arr.Length == 0)
            {
                return this.ZeroDocNumber(res);
            }

            string prefix = arr.Length > 3 ? arr[0] : null;
            int no = 0;
            bool resNo = Int32.TryParse(arr[0 + arrCount], out no);
            int month = 0;
            bool resMonth = Int32.TryParse(arr[1 + arrCount], out month);
            int year = 0;
            bool resYear = Int32.TryParse(arr[2 + arrCount], out year);

            if (resNo && resMonth && resYear)
            {
                res.DocNumber = no;
                res.DocMonth = month;
                res.Prefix = prefix;
                res.DocYear = year;
            }

            return res;
        }

        private DocNumberDTO ZeroDocNumber(DocNumberDTO res)
        {
            res.DocNumber = 0;
            res.DocMonth = DateTime.Now.Month;
            res.DocYear = DateTime.Now.Year;

            return res;
        }

        private DocNumberDTO NextNumber(DocNumberDTO res, DateTime date) {

            //next yaar
            if (res.DocYear < date.Year) {
                res.DocMonth = 1;
                res.DocNumber = 1;
                res.DocYear = date.Year;
                return res;
            }
            //next month
            if (res.DocMonth < date.Month)
            {
                res.DocMonth = date.Month;
                //res.DocNumber = 1;
                res.DocNumber++;
                return res;
            }
            res.DocNumber++; 
            return res;
        }

    }
}
