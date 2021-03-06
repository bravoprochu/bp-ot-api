﻿using bp.kpir.DAO.Addresses;
using bp.kpir.DAO.Contractor;
using bp.shared.StringHelp;
using System.Linq;

namespace bp.sharedLocal
{
    public static class EntitiesConv
    {
        public static string CompanyAddressCombined(Address add)
        {
            return $"({add.Country}) {add.Postal_code} {add.Locality}, {add.Street_address} {add.Street_number}";
        }

        public static string CompanyBankAccountCombined(BankAccount bank)
        {
            string acc_no = "";
            string swift = "";
            //string accNoFormated = StringHelpful.SeparatorEvery(bank.Account_no, 4);

            if (bank.Account_no.Length == 28)
            {
                acc_no = StringHelpful.SeparatorEveryBeginningEnd(bank.Account_no);
            }

            if (bank.Account_no.Length == 26)
            {
                var firstTwo = string.Join("", bank.Account_no.ToCharArray().Take(2));
                var last = string.Join("", bank.Account_no.ToCharArray().TakeLast(24));

                acc_no =firstTwo+" "+ StringHelpful.SeparatorEveryBeginningEnd(last);
            }

            swift = string.IsNullOrWhiteSpace(bank.Swift) ? null : " "+ bank.Swift;

            return $"{acc_no}{swift} ({bank.Type})";
        }

        public static string CompayContactCombined(Company comp)
        {
            var result = string.IsNullOrWhiteSpace(comp.Telephone) ? "" : $"tel: {comp.Telephone}";
            result += result.Length > 2 ? ", " : "";
            result += string.IsNullOrWhiteSpace(comp.Email) ? "" : $"email: {comp.Email}";
            return result;
        }
    }
}
