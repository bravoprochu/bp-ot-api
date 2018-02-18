using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using iText.IO.Font;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Layout.Splitting;
using iText.IO.Font.Otf;
using iText.Kernel.Events;
using bp.ot.s.API.Models.Load;
using Microsoft.AspNetCore.Hosting;
using System.Text;
using bp.ot.s.API.Entities.Dane.Company;
using bp.ot.s.API.Entities.Dane.Invoice;


namespace bp.PomocneLocal.Pdf
{
    public class PdfRaports
    {
        private readonly IHostingEnvironment _env;

        public PdfRaports(IHostingEnvironment env)
        {
            this._env = env;
        }

        public MemoryStream LoadOfferPdf(LoadDTO loadDTO)
        {
            var ms = new MemoryStream();
            var doc = this.DefaultPdfDoc(ms);
            


            var tblNaglowek = this.HederCompanyGen(loadDTO.Sell.Principal, "Zleceniodawca", loadDTO.Sell.Selling_info.Company, "Zleceniobiorca/Przewoźnik", $"Zlecenie przewozowe nr {loadDTO.LoadNo}");

            var tblRoutes = new Table(UnitValue.CreatePercentArray(new float[] { 2, 2, 2, 2, 1, 1, 2, 2 })); //8 cols


            int routeIdx = 1;
            float routeFontSize = 8f;
            foreach (var route in loadDTO.Buy.Routes)
            {
                var palletsCount = route.Pallets.Count;

                tblRoutes.AddCell(FakCell($"{routeIdx}) {route.Loading_type}", null, routeFontSize * 1f, TextAlignment.LEFT, 1 + palletsCount, 1));
                tblRoutes.AddCell(FakCell($"{route.Loading_date.ToShortDateString()} {route.Loading_date.ToShortTimeString()}", null, routeFontSize, TextAlignment.LEFT, 1, 1));
                tblRoutes.AddCell(FakCell(route.Address.AddressCombined, null, routeFontSize, TextAlignment.LEFT, 1, 2));
                //pallets count>0 1st row
                if (route.Pallets.Count > 0)
                {
                    tblRoutes.AddCell(FakCell("Typ palety", null, routeFontSize * 0.5f, TextAlignment.CENTER, 1, 1).SetFontColor(ColorConstants.GRAY));
                    tblRoutes.AddCell(FakCell("Ilość", null, routeFontSize * 0.5f, TextAlignment.CENTER, 1, 1).SetFontColor(ColorConstants.GRAY));
                    tblRoutes.AddCell(FakCell("Info", null, routeFontSize * 0.5f, TextAlignment.CENTER, 1, 2).SetFontColor(ColorConstants.GRAY));
                }
                else
                {
                    //tblNaglowek.AddCell(PustaCell(1, 3));
                    tblRoutes.AddCell(FakCell(route.Info, null, routeFontSize * 0.8f, TextAlignment.LEFT, 1, 5));
                }


                //pallets count>0 2nd row
                if (route.Pallets.Count > 0)
                {
                    int palletIdx = 0;
                    foreach (var pallet in route.Pallets)
                    {
                        List<string> routeInfoArr = new List<string>();
                        string rInfo;
                        string routeInfo;

                        if (!string.IsNullOrWhiteSpace(pallet.Info)) { routeInfoArr.Add(pallet.Info); }
                        if (pallet.Type.Value == "other") { routeInfoArr.Add(pallet.Dimmension); }
                        if (pallet.Type.Value == "other" && pallet.Is_exchangeable.HasValue && pallet.Is_exchangeable.Value) { routeInfoArr.Add("wymienialna"); }
                        if (pallet.Type.Value == "other" && pallet.Is_stackable.HasValue && pallet.Is_stackable.Value) { routeInfoArr.Add("piętrowalna"); }
                        rInfo = string.Join(", ", routeInfoArr);
                        routeInfo = palletIdx == 0 ? route.Info : null;

                        tblRoutes.AddCell(FakCell(routeInfo, null, routeFontSize * 0.8f, TextAlignment.LEFT, 1, 3));
                        tblRoutes.AddCell(FakCell(pallet.Type.Value, null, routeFontSize * 0.8f, TextAlignment.CENTER, 1, 1));
                        tblRoutes.AddCell(FakCell(pallet.Amount.ToString(), null, routeFontSize * 0.8f, TextAlignment.CENTER, 1, 1));
                        tblRoutes.AddCell(FakCell(rInfo, null, routeFontSize * 0.8f, TextAlignment.CENTER, 1, 2));

                        palletIdx++;
                    }
                }

                routeIdx++;
                tblRoutes.AddCell(EmptyCell(1, 8).SetBorderTop(new SolidBorder(ColorConstants.LIGHT_GRAY, 1, 0.5f)));
            }

            var extraInfo = loadDTO.Buy.Load_info.ExtraInfo;
            List<string> infoArr = new List<string>();

            infoArr.Add(extraInfo.Required_truck_body.ViewValue);
            if (extraInfo.Is_for_clearence.HasValue && extraInfo.Is_for_clearence.Value) { infoArr.Add("do oclenia (for clearence)"); }
            if (extraInfo.Is_ltl.HasValue && extraInfo.Is_ltl.Value) { infoArr.Add("doładunek (LTL)"); }
            if (extraInfo.Is_lift_required.HasValue && extraInfo.Is_lift_required.Value) { infoArr.Add("wymagana winda (lift)"); }
            if (extraInfo.Is_tir_cable_required.HasValue && extraInfo.Is_tir_cable_required.Value) { infoArr.Add("wymagana linka celna (tir cable)"); }
            if (extraInfo.Is_tracking_system_required.HasValue && extraInfo.Is_tracking_system_required.Value) { infoArr.Add("wymagany GPS"); }
            if (extraInfo.Is_truck_crane_required.HasValue && extraInfo.Is_truck_crane_required.Value) { infoArr.Add("gotowy do załadunku (truck crane)"); }
            if (extraInfo.Required_adr_classes.Count > 0)
            {
                var adr = extraInfo.Required_adr_classes.Select(s => s.ViewValue.Trim().ToLower()).ToList();
                infoArr.Add($"ADR:  ({string.Join(", ", adr)})");
            }
            if (extraInfo.Type_of_load != null) { infoArr.Add(extraInfo.Type_of_load.ViewValue); }
            var loadInfo = loadDTO.Buy.Load_info;
            if (loadInfo.Load_height.HasValue && loadInfo.Load_length.HasValue && loadInfo.Load_volume.HasValue) {
                infoArr.Add($"Wymiary ładunku: wys/dł/obj/ciężar ({loadInfo.Load_height.Value}/{loadInfo.Load_length.Value}/{loadInfo.Load_volume.Value}/{loadInfo.Load_weight})");
            }

            







            float headerFontSize = 8f;
            string wartosc = loadDTO.Sell.Selling_info.Price.Price.ToString("0.00");

            doc.Add(tblNaglowek);
            doc.Add(HeaderCell($"Stawka frachtowa: {wartosc} {loadDTO.Sell.Selling_info.Price.Currency.Name}, termin płatności: {loadDTO.Sell.Selling_info.PaymentTerms.PaymentDays.Value} dni, ALL-IN", headerFontSize));

            doc.Add(HeaderCell("Kontakt", headerFontSize));
            doc.Add(FakCell($"PO ZAŁADUNKU I ROZŁADUNKU PROSZĘ O SMS NA NR {loadDTO.Sell.ContactPersonsList.FirstOrDefault().Telephone} !!!! Przewoźnik ma obowiązek udzielania informacji o aktualnym położeniu samochodu,  brak tych informacji spowoduje obniżenie frachtu o 50 euro ! Kierowca ma obowiązek zweryfikować towar podczas załadunku pod względem jakościowym i ilościowym, a wszystkie zastrzeżenia co do jego jakości muszą być wpisane w dokument CMR. ", null, 6f, TextAlignment.LEFT, 1, 1));
            foreach (var contactPerson in loadDTO.Sell.ContactPersonsList.ToList())
            {
                doc.Add(FakCell(contactPerson.CompanyEmployeeInfo, null, routeFontSize, TextAlignment.LEFT, 1, 1));
            }

            doc.Add(HeaderCell("Trasa:", headerFontSize));
            doc.Add(tblRoutes);
            doc.Add(HeaderCell("Info", headerFontSize));
            doc.Add(FakCell($"[{string.Join(", ", infoArr)}]", "extra info", routeFontSize, TextAlignment.LEFT, 1, 1));

            //regulamin
            int regIdx = 1;
            float regFontSize = 6f;
            doc.Add(HeaderCell("Ogólne warunki zlecenia", headerFontSize));
            foreach (var reg in this.Regulamin())
            {
                doc.Add(FakCell(reg, $"{ regIdx})", regFontSize, TextAlignment.LEFT,1,1));
                regIdx++;
            }


            
                
            doc.Close();
            return ms;
        }



        public MemoryStream InvoicePdf(InvoiceSellDTO inv) {
            MemoryStream ms = new MemoryStream();
            var doc = this.DefaultPdfDoc(ms);
            
            

            //doc.GetPdfDocument().AddEventHandler(PdfDocumentEvent.INSERT_PAGE, new InvoiceFooter(doc));
            
            bool isCorr = inv.IsCorrection;
            string invoiceTypeName = inv.IsCorrection ? "Faktura korygująca" : "Faktura VAT";
            string subTitle = isCorr ? $"Do dokumentu: {inv.InvoiceOriginalNo}": null;
            var headerCompany = this.HederCompanyGen(inv.CompanySeller, "Sprzedawca", inv.CompanyBuyer, "Nabywca", $"{invoiceTypeName} {inv.InvoiceNo}", subTitle);

            float posFontSize = 9f;

           


            doc.Add(FakCell(inv.DateOfSell.ToShortDateString(), "Data sprzedaży", posFontSize, TextAlignment.RIGHT, 1, 1));
            doc.Add(FakCell(inv.DateOfIssue.ToShortDateString(), "Data wystawienia", posFontSize, TextAlignment.RIGHT, 1, 1));
            doc.Add(headerCompany);

            doc.Add(EmptyCell(1, 1));
            //if (isCorr) {
            //    doc.Add(FakCell("Po korekcie", null, posFontSize * 1.3f, TextAlignment.LEFT, 1, 1));
            //}
            var invListTable = this.InvoiceLinesTable(inv.InvoiceLines, posFontSize, isCorr);
            doc.Add(invListTable);
            doc.Add(EmptyCell(1, 1));

            var rates = this.InvoiceRatesTable(inv.Rates.OrderByDescending(o=>o.Current.Vat_rate).ToList(), posFontSize, isCorr);
            rates.SetHorizontalAlignment(HorizontalAlignment.RIGHT);
            rates.SetKeepTogether(true);
            rates.SetMarginTop(posFontSize);

            doc.Add(rates);

            var totalTable = this.InvoiceTotalTable(inv.InvoiceTotal, isCorr, posFontSize);
            totalTable.SetMarginTop(posFontSize);
            totalTable.SetMarginBottom(posFontSize);
            totalTable.SetHorizontalAlignment(HorizontalAlignment.RIGHT);
            doc.Add(totalTable);



            if (isCorr) {
                //var leftToPayValue = inv.invoiceOriginalPaid? 
                
                var leftToPay = FakCell(inv.GetCorrectionPaymenntInfo, null, posFontSize * 2f, TextAlignment.RIGHT, 1, 1);
                doc.Add(leftToPay);
            }


            doc.Add(FakCell($"{inv.Currency.Name} ({inv.Currency.Description})", "Waluta", posFontSize * 1.3f, TextAlignment.LEFT, 1, 1));
            if (isCorr)
            {
                var terms = inv.PaymentTerms;

                var dayDays = terms.PaymentDays > 1 ? "dni" : "dzień";
                var isDays = terms.PaymentTerm.IsPaymentDate ? $", {terms.PaymentDays} {dayDays}" : null;
                doc.Add(FakCell($"{terms.PaymentTerm.Name}{isDays}", "Forma płatności", posFontSize * 1.3f, TextAlignment.LEFT, 1, 1));
            }
            else {
                doc.Add(FakCell(inv.PaymentTerms.PaymentTermsCombined, "Forma płatności, termin", posFontSize * 1.3f, TextAlignment.LEFT, 1, 1));
            }
            

            if (inv.ExtraInfo.Is_in_words) {
                doc.Add(FakCell(inv.ExtraInfo.Total_brutto_in_words, "Słownie brutto", posFontSize * 1.3f, TextAlignment.LEFT,  1, 1));
            }
            if (inv.ExtraInfo.Is_load_no) {
                doc.Add(FakCell(inv.ExtraInfo.LoadNo, "Zlecenie nr", posFontSize * 1.3f, TextAlignment.LEFT, 1, 1));
            }
            if (inv.ExtraInfo.Is_tax_nbp_exchanged) {
                doc.Add(FakCell(inv.ExtraInfo.Tax_exchanged_info, "Przelicznik", posFontSize * 1.3f, TextAlignment.LEFT, 1, 1));
            }
            if (!string.IsNullOrWhiteSpace(inv.Info)) {
                doc.Add(FakCell(inv.Info, "Uwagi", posFontSize * 1.3f, TextAlignment.LEFT, 1, 1));
            }

            if (inv.ExtraInfo.IsSigningPlace) {
                var signingTable = new Table(UnitValue.CreatePercentArray(new float[] { 3, 2, 3 })); //3 cols
                signingTable.SetWidth(UnitValue.CreatePercentValue(100));

                signingTable.AddCell(FakCell("...................................................................", null, posFontSize, TextAlignment.CENTER, 1, 1));
                signingTable.AddCell(EmptyCell());
                signingTable.AddCell(FakCell("...................................................................", null, posFontSize, TextAlignment.CENTER, 1, 1));

                signingTable.AddCell(FakCell("Podpis osoby upoważnionej do odebrania faktury", null, posFontSize*0.8f, TextAlignment.CENTER, 1, 1));
                signingTable.AddCell(EmptyCell());
                signingTable.AddCell(FakCell("Podpis osoby upoważnionej do wystawienia faktury", null, posFontSize*0.8f, TextAlignment.CENTER, 1, 1));

                signingTable.SetMarginTop(posFontSize * 10f);
                doc.Add(signingTable);
            }


            if (doc.GetPdfDocument().GetNumberOfPages() > 1)
            {
                doc.GetPdfDocument().AddEventHandler(PdfDocumentEvent.END_PAGE, new InvoiceFooter(doc, this.FontExoThin, inv.GetInvoiceNo));
            }
            doc.Close();
            return ms;
        }


        #region CellsGen

        private Document DefaultPdfDoc(MemoryStream ms) {

            var pdfWriter = new PdfWriter(ms);
            var pdf = new PdfDocument(pdfWriter);
            var doc = new Document(pdf, PageSize.A4);
            doc.SetMargins(30, 20, 40, 20);
            doc.SetFont(this.FontExoRegular);

            return doc;
        }

        private Table HederCompanyGen (CompanyDTO companyOnLeft, string leftHeaderTitle, CompanyDTO companyOnRight, string rightHeaderTitle, string title, string subTitle=null)
        {
            var tblNaglowek = new Table(UnitValue.CreatePercentArray(new float[] { 4, 4, 1, 4, 4 }));

            //var companyOnLeft=

            //doc.Add(tblNaglowek);
            float fSize = 9f;
            tblNaglowek.AddCell(FakCell(title, null, fSize * 1.8f, TextAlignment.CENTER, 1, 5));
            if (!string.IsNullOrWhiteSpace(subTitle)) {
                tblNaglowek.AddCell(FakCell(subTitle, null, fSize * 0.9f, TextAlignment.CENTER, 1, 5));
            }
            tblNaglowek.AddCell(EmptyCell(2, 5));
            tblNaglowek.AddCell(FakCell(leftHeaderTitle, null, fSize * 1.5f, TextAlignment.CENTER, 2, 2).SetBold());
            tblNaglowek.AddCell(EmptyCell(2, 1));
            tblNaglowek.AddCell(FakCell(rightHeaderTitle, null, fSize * 1.5f, TextAlignment.CENTER, 2, 2).SetBold());
            tblNaglowek.AddCell(FakCell(companyOnLeft.Legal_name, null, fSize * 1.2f, TextAlignment.CENTER, 2, 2));
            tblNaglowek.AddCell(EmptyCell(2, 1));
            tblNaglowek.AddCell(FakCell(companyOnRight.Legal_name, null, fSize * 1.2f, TextAlignment.CENTER, 2, 2));
            tblNaglowek.AddCell(FakCell(companyOnLeft.AddressList[0].AddressCombined, null, fSize * 0.9f, TextAlignment.CENTER, 1, 2));
            tblNaglowek.AddCell(EmptyCell(1, 1));
            tblNaglowek.AddCell(FakCell(companyOnRight.AddressList[0].AddressCombined, null, fSize * 0.9f, TextAlignment.CENTER, 1, 2));
            tblNaglowek.AddCell(FakCell("NIP: " + companyOnLeft.Vat_id, null, fSize * 1.1f, TextAlignment.CENTER, 1, 2));
            tblNaglowek.AddCell(EmptyCell(1, 1));
            tblNaglowek.AddCell(FakCell("NIP: " + companyOnRight.Vat_id, null, fSize * 1.1f, TextAlignment.CENTER, 1, 3));
            tblNaglowek.AddCell(FakCell(companyOnLeft.ContactInfo, null, fSize * 0.7f, TextAlignment.CENTER, 1, 2));
            tblNaglowek.AddCell(EmptyCell(1, 1));
            tblNaglowek.AddCell(FakCell(companyOnRight.ContactInfo, null, fSize * 0.7f, TextAlignment.CENTER, 1, 2));


            var bankAccountsleft = companyOnLeft.BankAccountList.Count;
            var bankAccountsRight = companyOnRight.BankAccountList.Count;
            int bankAccountsLength = bankAccountsleft >= bankAccountsRight ? bankAccountsleft : bankAccountsRight;


            if (bankAccountsLength > 0)
            {
                for (int i = 0; i < bankAccountsLength; i++)
                {
                    if (bankAccountsleft-1 >= i && bankAccountsleft>0)
                    {
                        var bankAccount = companyOnLeft.BankAccountList[i];
                        tblNaglowek.AddCell(FakCell($"{bankAccount.Swift} {bp.Pomocne.StringHelp.StringHelpful.SeparatorEveryBeginningEnd(bankAccount.Account_no)}", bankAccount.Type, fSize*0.9f, TextAlignment.LEFT, 1, 2));
                    }
                    else
                    {
                        tblNaglowek.AddCell(EmptyCell(1, 2));
                    }

                    tblNaglowek.AddCell(EmptyCell(1, 1));

                    if (bankAccountsRight-1 >= i && bankAccountsRight>0)
                    {
                        var bankAccount = companyOnRight.BankAccountList[i];
                        tblNaglowek.AddCell(FakCell($"{bankAccount.Swift} {bp.Pomocne.StringHelp.StringHelpful.SeparatorEveryBeginningEnd(bankAccount.Account_no)}", bankAccount.Type, fSize*0.9f, TextAlignment.RIGHT, 1, 2));
                    }
                    else
                    {
                        tblNaglowek.AddCell(EmptyCell(1, 2));
                    }
                }
            }



            tblNaglowek.AddCell(EmptyCell(1, 5));
            return tblNaglowek;
        }

        private static Cell HeaderCell(string text, float fontSize = 6f, int rowSpan = 1, int colspan = 1)
        {
            return new Cell(rowSpan, colspan)
                .Add(new Paragraph(text)
                //.SetFont(font)
                .SetFontSize(fontSize)
                .SetBold()
                .SetTextAlignment(TextAlignment.LEFT)
                .SetMarginLeft(-fontSize * 0.7f)
                .SetBorderBottom(new SolidBorder(ColorConstants.DARK_GRAY, 0.3f))
                )
            //.SetPadding(2f)
            //.SetMaxHeight(fontSize*1.3f)
            .SetVerticalAlignment(VerticalAlignment.MIDDLE);
            //.SetBackgroundColor(ColorConstants.LIGHT_GRAY);
            //.SetBorderBottom(new SolidBorder(ColorConstants.BLUE, 2f));

        }

        private static Cell TableHeaderCell(string text, float fontSize, int rowSpan = 1, int colSpan = 1)
        {
            text = text ?? "";
            return new Cell(rowSpan, colSpan)
            .Add(new Paragraph().Add(new Text(text)))
            .SetFontSize(fontSize)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
            .SetBackgroundColor(ColorConstants.DARK_GRAY,0.25f)
            .SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 0.5f));
        }

        private static Cell PozCell(string text, float fontSize, TextAlignment textAlignment, int rowSpan = 1, int colSpan = 1)
        {
            text = text ?? "";
            return new Cell(rowSpan, colSpan)
            .Add(new Paragraph().Add(new Text(text)))
            .SetFontSize(fontSize)
            .SetTextAlignment(textAlignment)
            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
            .SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 0.5f));
        }

        private static Cell FakCell(string text, string caption, float fontSize, TextAlignment textAlignment, int rowSpan = 1, int colSpan = 1)
        {
            return new Cell(rowSpan, colSpan)
              .Add(new Paragraph()
                .Add(new Text(String.IsNullOrEmpty(caption) ? "" : caption + ": ")
                .SetFontSize(fontSize / 1.5f))
                .Add(new Text(String.IsNullOrEmpty(text) ? "" : text))
                .SetFontSize(fontSize))
            //.Add(new Paragraph().Add(new Text(String.IsNullOrEmpty(text)?"": text)).SetFontSize(fontSize).Add(new Text("\n"+caption).SetFontSize(fontSize/2)))
            //.Add(new Paragraph().Add(new Text("data wystawienia").SetFontSize(fontSize/2)))
            
            .SetPadding(0)
            .SetTextAlignment(textAlignment)
            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
            .SetBorder(Border.NO_BORDER);
        }

        private static Cell EmptyCell(int rowSpan = 1, int colSpan = 1)
        {
            return new Cell(rowSpan, colSpan)
                .Add(new Paragraph().Add(new Text(" ")))
                .SetBorder(Border.NO_BORDER);
        }


        private Table InvoiceLinesTable(List<InvoiceLinesGroupDTO> posList, float posFontSize, bool isInvoiceCorrection=false)
        {
            var posListTable = new Table(UnitValue.CreatePercentArray(new float[] { 7, 1, 1, 1, 2, 2, 2, 2, 2 })); //cols: 9
 
            posListTable.AddCell(TableHeaderCell("Nazwa towaru/usługi", posFontSize * 0.8f, 1, 1));
            posListTable.AddCell(TableHeaderCell("PKWiU", posFontSize * 0.8f, 1, 1));
            posListTable.AddCell(TableHeaderCell("Ilość", posFontSize * 0.8f, 1, 1));
            posListTable.AddCell(TableHeaderCell("Jedn.", posFontSize * 0.8f, 1, 1));
            posListTable.AddCell(TableHeaderCell("Cena jednostkowa", posFontSize * 0.8f, 1, 1));
            posListTable.AddCell(TableHeaderCell("Wartość netto", posFontSize * 0.8f, 1, 1));
            posListTable.AddCell(TableHeaderCell("Stawka %", posFontSize * 0.8f, 1, 1));
            posListTable.AddCell(TableHeaderCell("Kwota podatku", posFontSize * 0.8f, 1, 1));
            posListTable.AddCell(TableHeaderCell("Wartość brutto", posFontSize * 0.8f, 1, 1));

            int lineNo = 1;
            foreach (var pos in posList)
            {
                var isCorr = pos.Current.IsCorrected;
                var corrFontSize = posFontSize * 0.8f;
                var corrHeaderFontSize= posFontSize * 0.9f;

                var posCurrent = pos.Current;
                var posCorrections = pos.Corrections;
                var posOriginal = pos.Original;
                
                var currName = $"{lineNo.ToString()}) {posCurrent.Name}";
                var orgName= $"{lineNo.ToString()}) {posOriginal.Name}";


                //if Invoice line is corrected
                if (isCorr) {
                    posListTable.AddCell(FakCell("Przed korektą: ", null, corrHeaderFontSize, TextAlignment.LEFT, 1, 9));
                    posListTable.AddCell(PozCell(orgName, corrFontSize, TextAlignment.LEFT, 1, 1));
                    posListTable.AddCell(PozCell(posOriginal.Pkwiu, corrFontSize*0.8f, TextAlignment.CENTER, 1, 1));
                    posListTable.AddCell(PozCell(posOriginal.Quantity.ToString(), corrFontSize, TextAlignment.CENTER, 1, 1));
                    posListTable.AddCell(PozCell(posOriginal.Measurement_unit, corrFontSize * 0.8f, TextAlignment.CENTER, 1, 1));
                    posListTable.AddCell(PozCell(posOriginal.Unit_price.ToString("# ##0.00"), corrFontSize, TextAlignment.RIGHT, 1, 1));
                    posListTable.AddCell(PozCell(posOriginal.Netto_value.ToString("# ##0.00"), corrFontSize, TextAlignment.RIGHT, 1, 1));
                    posListTable.AddCell(PozCell(posOriginal.Vat_rate, corrFontSize * 0.8f, TextAlignment.CENTER, 1, 1));
                    posListTable.AddCell(PozCell(posOriginal.Vat_value.ToString("# ##0.00"), corrFontSize, TextAlignment.RIGHT, 1, 1));
                    posListTable.AddCell(PozCell(posOriginal.Brutto_value.ToString("# ##0.00"), corrFontSize * 1.1f, TextAlignment.RIGHT, 1, 1));


                    //correction
                    posListTable.AddCell(FakCell("Korekta: ", null, corrHeaderFontSize, TextAlignment.LEFT, 1, 9)).SetKeepWithNext(true);
                    posListTable.AddCell(PozCell($"Przyczyna korekty: {posCurrent.CorrectionInfo}", corrFontSize, TextAlignment.LEFT, 1, 2));
                    posListTable.AddCell(PozCell(posCorrections.Quantity==0 ? "-" : posCorrections.Quantity.ToString(), corrFontSize, TextAlignment.CENTER, 1, 1));
                    posListTable.AddCell(PozCell(posCorrections.Measurement_unit, corrFontSize * 0.8f, TextAlignment.CENTER, 1, 1));
                    posListTable.AddCell(PozCell(posCorrections.Unit_price == 0 ? "-" : posCorrections.Unit_price.ToString("# ##0.00"), corrFontSize, TextAlignment.RIGHT, 1, 1));
                    posListTable.AddCell(PozCell(posCorrections.Netto_value==0 ? "-" : posCorrections.Netto_value.ToString("# ##0.00"), corrFontSize, TextAlignment.RIGHT, 1, 1));
                    posListTable.AddCell(PozCell(posCorrections.Vat_rate, corrFontSize * 0.8f, TextAlignment.CENTER, 1, 1));
                    posListTable.AddCell(PozCell(posCorrections.Vat_value==0? "-" : posCorrections.Vat_value.ToString("# ##0.00"), corrFontSize, TextAlignment.RIGHT, 1, 1));
                    posListTable.AddCell(PozCell(posCorrections.Brutto_value==0? "-" : posCorrections.Brutto_value.ToString("# ##0.00"), corrFontSize *1.1f, TextAlignment.RIGHT, 1, 1));

                    //original
                }


                if (isInvoiceCorrection)
                {
                    if (isCorr)
                    {
                        posListTable.AddCell(FakCell("Po korekcie: ", null, corrHeaderFontSize, TextAlignment.LEFT, 1, 9));
                    }
                    else {
                        posListTable.AddCell(FakCell("Bez zmian: ", null, corrHeaderFontSize, TextAlignment.LEFT, 1, 9));
                    }
                        posListTable.AddCell(PozCell(currName, posFontSize, TextAlignment.LEFT, 1, 1).SetBorderBottom(this.TableLineBottomBorder()));
                        posListTable.AddCell(PozCell(posCurrent.Pkwiu, corrFontSize * 0.8f, TextAlignment.CENTER, 1, 1).SetBorderBottom(this.TableLineBottomBorder()));
                        posListTable.AddCell(PozCell(posCurrent.Quantity.ToString(), posFontSize, TextAlignment.CENTER, 1, 1).SetBorderBottom(this.TableLineBottomBorder()));
                        posListTable.AddCell(PozCell(posCurrent.Measurement_unit, posFontSize * 0.8f, TextAlignment.CENTER, 1, 1).SetBorderBottom(this.TableLineBottomBorder()));
                        posListTable.AddCell(PozCell(posCurrent.Unit_price.ToString("# ##0.00"), posFontSize, TextAlignment.RIGHT, 1, 1).SetBorderBottom(this.TableLineBottomBorder()));
                        posListTable.AddCell(PozCell(posCurrent.Netto_value.ToString("# ##0.00"), posFontSize, TextAlignment.RIGHT, 1, 1).SetBorderBottom(this.TableLineBottomBorder()));
                        posListTable.AddCell(PozCell(posCurrent.Vat_rate, posFontSize * 0.8f, TextAlignment.CENTER, 1, 1).SetBorderBottom(this.TableLineBottomBorder()));
                        posListTable.AddCell(PozCell(posCurrent.Vat_value > 0 ? posCurrent.Vat_value.ToString("# ##0.00") : "-", posFontSize, TextAlignment.RIGHT, 1, 1).SetBorderBottom(this.TableLineBottomBorder()));
                        posListTable.AddCell(PozCell(posCurrent.Brutto_value.ToString("# ##0.00"), posFontSize * 1.1f, TextAlignment.RIGHT, 1, 1).SetBorderBottom(this.TableLineBottomBorder()));
                    
                }
                if(!isInvoiceCorrection) {
                    posListTable.AddCell(PozCell(currName, posFontSize, TextAlignment.LEFT, 1, 1));
                    posListTable.AddCell(PozCell(posCurrent.Pkwiu, corrFontSize * 0.8f, TextAlignment.CENTER, 1, 1));
                    posListTable.AddCell(PozCell(posCurrent.Quantity.ToString(), posFontSize, TextAlignment.CENTER, 1, 1));
                    posListTable.AddCell(PozCell(posCurrent.Measurement_unit, posFontSize * 0.8f, TextAlignment.CENTER, 1, 1));
                    posListTable.AddCell(PozCell(posCurrent.Unit_price.ToString("# ##0.00"), posFontSize, TextAlignment.RIGHT, 1, 1));
                    posListTable.AddCell(PozCell(posCurrent.Netto_value.ToString("# ##0.00"), posFontSize, TextAlignment.RIGHT, 1, 1));
                    posListTable.AddCell(PozCell(posCurrent.Vat_rate, posFontSize * 0.8f, TextAlignment.CENTER, 1, 1));
                    posListTable.AddCell(PozCell(posCurrent.Vat_value > 0 ? posCurrent.Vat_value.ToString("# ##0.00") : "-", posFontSize, TextAlignment.RIGHT, 1, 1));
                    posListTable.AddCell(PozCell(posCurrent.Brutto_value.ToString("# ##0.00"), posFontSize * 1.1f, TextAlignment.RIGHT, 1, 1));
                }
                lineNo++;
            }

            

            return posListTable;

        }


        private Table InvoiceRatesTable(List<InvoiceRatesGroupDTO> rates, float posFontSize, bool isCorrection=false)
        {
            
            var smallFontSize = posFontSize * 0.75f;
            var rowSpan = isCorrection ? 2 : 1;
            var tableWidth = isCorrection ? 60 : 50;
            var tableCols = new float[] { 1, 2, 2, 2 };
            var tableColsCorrection = new float[] {1, 1, 2, 2, 2 };

            var tbl = new Table(UnitValue.CreatePercentArray(isCorrection ? tableColsCorrection : tableCols));
            tbl.SetWidth(UnitValue.CreatePercentValue(tableWidth));

            tbl.AddCell(TableHeaderCell("Stawka", posFontSize, 1, isCorrection? 2:1));
            tbl.AddCell(TableHeaderCell("Netto", posFontSize, 1, 1));
            tbl.AddCell(TableHeaderCell("Podatek", posFontSize, 1, 1));
            tbl.AddCell(TableHeaderCell("Brutto", posFontSize, 1, 1));

            foreach (var taxpos in rates.OrderBy(o => o.Current.Vat_value))
            {
                var vatRate = string.IsNullOrEmpty(taxpos.Current.Vat_rate) ? taxpos.Original.Vat_rate : taxpos.Current.Vat_rate;
                if (isCorrection)
                {
                    tbl.AddCell(PozCell(vatRate, posFontSize, TextAlignment.CENTER, isCorrection ? 3 : 1, 1).SetBorderBottom(this.TableLineBottomBorder()));
                    tbl.AddCell(PozCell("przed korektą: ",smallFontSize*0.7f,TextAlignment.CENTER,1,1));
                    tbl.AddCell(PozCell(taxpos.Original.Netto_value.ToString("# ##0.00"), smallFontSize, TextAlignment.CENTER, 1, 1));
                    tbl.AddCell(PozCell(taxpos.Original.Vat_value == 0 ? "-": taxpos.Original.Vat_value.ToString("# ##0.00"), smallFontSize, TextAlignment.CENTER, 1, 1));
                    tbl.AddCell(PozCell(taxpos.Original.Brutto_value.ToString("# ##0.00"), smallFontSize, TextAlignment.CENTER, 1, 1));

                    tbl.AddCell(PozCell("korekta: ", smallFontSize * 0.7f, TextAlignment.CENTER, 1, 1));
                    tbl.AddCell(PozCell(taxpos.Corrections.Netto_value==0 ? "-": taxpos.Corrections.Netto_value.ToString("# ##0.00"), smallFontSize, TextAlignment.CENTER, 1, 1));
                    tbl.AddCell(PozCell(taxpos.Corrections.Vat_value == 0 ? "-": taxpos.Corrections.Vat_value.ToString("# ##0.00"), smallFontSize, TextAlignment.CENTER, 1, 1));
                    tbl.AddCell(PozCell(taxpos.Corrections.Brutto_value==0 ? "-" : taxpos.Corrections.Brutto_value.ToString("# ##0.00"), smallFontSize, TextAlignment.CENTER, 1, 1));
                
                    tbl.AddCell(PozCell("po korekcie: ", smallFontSize * 0.7f, TextAlignment.CENTER, 1, 1).SetBorderBottom(this.TableLineBottomBorder()));
                    tbl.AddCell(PozCell(taxpos.Current.Netto_value.ToString("# ##0.00"), posFontSize, TextAlignment.CENTER, 1, 1).SetBorderBottom(this.TableLineBottomBorder()));
                    tbl.AddCell(PozCell(taxpos.Current.Vat_value > 0 ? taxpos.Current.Vat_value.ToString("# ##0.00") : "-", posFontSize, TextAlignment.CENTER, 1, 1).SetBorderBottom(this.TableLineBottomBorder()));
                    tbl.AddCell(PozCell(taxpos.Current.Brutto_value.ToString("# ##0.00"), posFontSize, TextAlignment.CENTER, 1, 1).SetBorderBottom(this.TableLineBottomBorder()));
                }
                else {
                    tbl.AddCell(PozCell(vatRate, posFontSize, TextAlignment.CENTER, isCorrection ? 3 : 1, 1));
                    tbl.AddCell(PozCell(taxpos.Current.Netto_value.ToString("# ##0.00"), posFontSize, TextAlignment.CENTER, 1, 1));
                    tbl.AddCell(PozCell(taxpos.Current.Vat_value > 0 ? taxpos.Current.Vat_value.ToString("# ##0.00") : "-", posFontSize, TextAlignment.CENTER, 1, 1));
                    tbl.AddCell(PozCell(taxpos.Current.Brutto_value.ToString("# ##0.00"), posFontSize, TextAlignment.CENTER, 1, 1));
                }
            }
            return tbl;
        }


        private Table InvoiceTotalTable(InvoiceTotalGroupDTO total, bool isCorrection, float posFontSize) {

            float headerFontSize = posFontSize * 1.3f;
            var tableCols = isCorrection ? new float[] { 2, 2, 2, 2 } : new float[] { 2, 2, 2 };
            var tbl = new Table(UnitValue.CreatePercentArray(tableCols));
            tbl.SetWidth(UnitValue.CreatePercentValue(50));


            if (isCorrection) {
                tbl.AddCell(TableHeaderCell("RAZEM: ", headerFontSize, 1, 1));
                tbl.AddCell(TableHeaderCell("Netto", headerFontSize, 1, 1));
                tbl.AddCell(TableHeaderCell("Podatek", headerFontSize, 1, 1));
                tbl.AddCell(TableHeaderCell("Brutto", headerFontSize, 1, 1));

                //second row
                tbl.AddCell(PozCell("przed korektą", posFontSize * 0.8f, TextAlignment.CENTER, 1, 1));
                tbl.AddCell(PozCell(total.Original.Total_netto.ToString("# ##0.00"), posFontSize * 0.8f, TextAlignment.CENTER, 1, 1));
                tbl.AddCell(PozCell(total.Original.Total_tax.ToString("# ##0.00"), posFontSize * 0.8f, TextAlignment.CENTER, 1, 1));
                tbl.AddCell(PozCell(total.Original.Total_brutto.ToString("# ##0.00"), posFontSize * 0.9f, TextAlignment.CENTER, 1, 1));

                tbl.AddCell(PozCell("korekta", posFontSize * 0.8f, TextAlignment.CENTER, 1, 1));
                tbl.AddCell(PozCell(total.Corrections.Total_netto==0? "-": total.Corrections.Total_netto.ToString("# ##0.00"), posFontSize * 0.8f, TextAlignment.CENTER, 1, 1));
                tbl.AddCell(PozCell(total.Corrections.Total_tax== 0? "-" : total.Corrections.Total_tax.ToString("# ##0.00"), posFontSize * 0.8f, TextAlignment.CENTER, 1, 1));
                tbl.AddCell(PozCell(total.Corrections.Total_brutto==0? "-" : total.Corrections.Total_brutto.ToString("# ##0.00"), posFontSize * 0.9f, TextAlignment.CENTER, 1, 1));

                tbl.AddCell(PozCell("po korekcie", posFontSize * 0.8f, TextAlignment.CENTER, 1, 1));
                tbl.AddCell(PozCell(total.Current.Total_netto.ToString("# ##0.00"), posFontSize * 1.2f, TextAlignment.CENTER, 1, 1));
                tbl.AddCell(PozCell(total.Current.Total_tax.ToString("# ##0.00"), posFontSize * 1.2f, TextAlignment.CENTER, 1, 1));
                tbl.AddCell(PozCell(total.Current.Total_brutto.ToString("# ##0.00"), posFontSize * 1.4f, TextAlignment.CENTER, 1, 1));
            } 


            //doc.Add(FakCell(inv.InvoiceTotal.Current.Total_netto.ToString("# ##0.00"), "Razem netto", posFontSize * 1.3f, TextAlignment.RIGHT, 1, 1).SetKeepWithNext(true));
            //doc.Add(FakCell(inv.InvoiceTotal.Current.Total_tax > 0 ? inv.InvoiceTotal.Current.Total_tax.ToString("# ##0.00") : "-", "Razem podatek", posFontSize * 1.3f, TextAlignment.RIGHT, 1, 1).SetKeepWithNext(true));
            //doc.Add(FakCell(inv.InvoiceTotal.Current.Total_brutto.ToString("# ##0.00") + $" {inv.Currency.Name}", "Razem brutto", posFontSize * 1.5f, TextAlignment.RIGHT, 1, 1));

            return tbl;
        }

        private Border TableLineBottomBorder(float borderSize=2f)
        {
            return new DoubleBorder(borderSize);
        }

        private List<string> Regulamin()
        {
            var res = new List<string>();

            res.Add(@"Przewoźnik oświadcza, że posiada wszelkie uprawnienia niezbędne do wykonywania przewozu objętego zleceniem, w tym aktualną koncesję na wykonywanie transportu międzynarodowego, aktualne zezwolenie na wjazd na teren poszczególnych państw.");
            res.Add(@"Przewoźnik zobowiązuje się do wykonywania przewozu, zgodnie z treścią zlecenia, obowiązującymi przepisami prawa oraz należytą starannością. Do obowiązków przewoźnika należy w szczególności: dobór i zapewnienie pojazdu właściwego do wykonania przewozu, podstawienie pojazdu w miejscu załadunku w określonym oknie czasowym, dopilnowanie prawidłowego załadunku i rozmieszczenia ładunku na pojeździe, wypełnienie dokumentów przewozowych, wprowadzenie do listu przewozowego odpowiednich wpisów w wypadku uzasadnionych zastrzeżeń odnośnie stanu ładunku, sposobu załadunku albo rozbieżnościami pomiędzy deklaracjami nadawcy a przesyłką.");
            res.Add(@"Przewoźnikowi nie wolno powierzyć wykonania przewozu objętego zleceniem osobie trzeciej bez pisemnej zgody zleceniodawcy.");
            res.Add(@"Przewoźnik pozostawia w miejscu załadunku i dostawy po jednym egzemplarzu dokumentu przewozowego. Dokument przewozowy musi w każdym przypadku zawierać oznaczenie i adres przewoźnika.");
            res.Add(@"Przewoźnik jest zobowiązany uzyskać zgodę zleceniodawcy na przewóz tym samym pojazdem przesyłek zleceniodawcy wraz z przesyłkami należącymi do osób trzecich. Również przeładunek wymaga zgody zleceniodawcy.");
            res.Add(@"Zabrania się pozostawiania samochodu z towarem poza parkingami strzeżonymi.");
            res.Add(@"Wszelkie przestoje muszą być podpisane i podstemplowane przez załadowcę lub wyładowcę w karcie postoju.");
            res.Add(@"Zleceniobiorca zobowiązuje się do wystawienia faktury natychmiast po wykonaniu usługi, w miesiącu wykonania usługi, data rozładunku musi być jednocześnie datą wystawienia faktury, oraz przesłania do OFFER TRANS S.C. wraz z następującymi dokumentami: CMR potwierdzony: pieczątką, datą i podpisem przez nadawcę i odbiorcę towaru. Bez tych potwierdzeń odmówimy odbioru faktury i CMR. Termin przesłania faktury; do 14 dni od dnia wykonania usługi. Przesłanie faktury w terminie późniejszym spowoduje obniżenie frachtu o 20% oraz przesunięcie płatności o dalsze 30 dni. Upoważniamy do wystawienia faktury VAT bez naszego podpisu.");
            res.Add(@"Zapłata uzgodnionego wynagrodzenia następuje w terminie określonym w zleceniu, a w razie nie określenia go w zleceniu w terminie 60 dni. Termin zapłaty liczony jest od dnia doręczenia zleceniodawcy prawidłowo wystawionej faktury VAT wraz z dokumentami określonymi w pkt.7 bądź innymi dokumentami określonymi w treści zlecenia.");
            res.Add(@"Wynagrodzenie wyrażone w walucie obcej podlega przeliczeniu na PLN według kursu średniego NBP ogłoszonego w dniu poprzedzającym dzień rozładunku. Konieczne jest podanie dwóch kont: walutowego wraz z numerem SWIFT oraz konta w PLN. Zastrzegamy sobie możliwość zapłaty w walucie PLN.");
            res.Add(@"Warunki przewozu: ALL IN.");
            res.Add(@"Przewoźnik jest zobowiązany do potwierdzenia przyjęcia zlecenia do realizacji lub odmowie przyjęcia zlecenia w ciągu 1 godziny od złożenia zlecenia przez zleceniodawcę. Dowodem złożenia zlecenia przez zleceniodawcę jest raport transmisji wygenerowany z faksu zleceniodawcy albo potwierdzenie wysłania zlecenia pocztą elektroniczną. Informacja o przyjęciu lub odmowie przyjęcia zlecenia przekazywana jest zwrotnie przez przewoźnika za pośrednictwem faksu lub poczty ");
            res.Add(@"elektronicznej. Zleceniodawca przestaje być związany zleceniem jeżeli przewoźnik nie potwierdzi przyjęcia zlecenia w wyżej wskazanym czasie. W takim przypadku uznaje się, że przewoźnik odmówił przyjęcia zlecenia.");
            res.Add(@"Przewoźnik oświadcza, że posiada i podczas wykonywania przewozu będzie posiadał ważne ubezpieczenie odpowiedzialności cywilnej przewoźnika drogowego w ruchu międzynarodowym, obejmujące odpowiedzialność za szkody mogące wyniknąć z niewykonania lub nienależytego wykonania zlecenia, w tym na skutek rażącego niedbalstwa i winy umyślnej przewoźnika. Na żądanie zleceniodawcy przewoźnik zobowiązany jest do okazania dokumentu potwierdzającego posiadanie takiego ubezpieczenia.");
            res.Add(@"Zleceniodawca nie ponosi wobec przewoźnika jakiejkolwiek odpowiedzialności w wypadku postoju pojazdu przewoźnika spowodowanego opóźnieniem przy załadunku trwające do 24 godzin oraz do 48 godzin przy rozładunku oraz w dni świąteczne i soboty. ");
            res.Add(@"Z zastrzeżeniem odmiennych postanowień zlecenia, za niewykonanie lub nienależyte wykonanie (w tym w szczególności za utratę, ubytek lub uszkodzenie przesyłki, opóźnienie w przewozie, utratę lub nieprawidłowe posłużenie się dokumentami) Przewoźnik ponosi odpowiedzialność zgodnie z obowiązującymi przepisami prawa, w tym przede wszystkim: przepisami Konwencji o umowie międzynarodowego przewozu drogowego towarów (CMR) z dnia 19 maja 1956 r. oraz przepisami ustawy z dnia 15 listopada 1984 r. Prawo przewozowe.");
            res.Add(@"W razie niewykonania lub nienależytego wykonania zlecenia zastrzegamy sobie prawo obciążenia zleceniobiorcy wszelkimi kosztami wynikającymi z tego zlecenia.");
            res.Add(@"W przypadku realizacji dostawy w terminie późniejszym niż wynika to ze zlecenia transportowego lub opóźnienia w podstawieniu samochodu do załadunku, zleceniodawca jest upoważniony do wyznaczenia kary umownej do wysokości frachtu za każdą rozpoczętą godzinę opóźnienia(wyjątek stanowią okoliczności, nie spowodowane wina przewoźnika). Wszelkie koszty oraz kary będące następstwem nie wywiązania się z niniejszego zlecenia oraz związane z uszkodzeniem, zaginięciem towaru ponosi przewoźnik który musi zapłacić firmie OFFER TRANS S.C. bez wezwania kwotę obciążenia w ciągu 15 dni od daty otrzymania noty obciążeniowej. W przypadku braku zapłaty przewoźnik wyraża zgodę na automatyczną kompensatę tej kwoty.");
            res.Add(@"Przewoźnik zobowiązuje się do niepodejmowania działań na terenie zleceniodawcy w ramach konkurencji pod rygorem kary 50 000 euro.");
            res.Add(@"Wszelkie spory związane z niniejszym zleceniem będą rozstrzygane przez sąd powszechny właściwy dla siedziby Zleceniodawcy.");
            res.Add(@"Zlecenie podlega prawu polskiemu.");
            res.Add(@"Pod pojęciem „Przewoźnik” rozumie się także kierowcę lub inną osobę faktycznie wykonującą czynności wymienione w zleceniu.");

            return res;
        }

        #endregion



        #region Fonts

        private PdfFont FontExoRegular
        {
            get
            {
                return PdfFontFactory.CreateFont(new Uri(_env.WebRootPath + "\\fonts\\Exo-Regular.otf").LocalPath, "Identity-H", true);
            }
        }
        private PdfFont FontExoBold
        {
            get
            {
                return PdfFontFactory.CreateFont(new Uri(_env.WebRootPath + "\\fonts\\Exo-Bold.otf").LocalPath, "Identity-H", true);
            }
        }

        private PdfFont FontExoExtraBold
        {
            get
            {
                return PdfFontFactory.CreateFont(new Uri(_env.WebRootPath + "\\fonts\\Exo-ExtraBold.otf").LocalPath, "Identity-H", true);
            }
        }

        private PdfFont FontExoThin
        {
            get
            {
                return PdfFontFactory.CreateFont(new Uri(_env.WebRootPath + "\\fonts\\Exo-Thin.otf").LocalPath, "Identity-H", true);
            }
        }

        #endregion
    }


    public class InvoiceCorrectionPaymentInfo
    {
        private InvoiceSellDTO _inv;

        public InvoiceCorrectionPaymentInfo(InvoiceSellDTO inv)
        {
            this._inv = inv;
        }

        public double MyProperty { get; set; }

        public void isToPayOrToReturn()
        {

        }
    }



    public class InvoiceFooter : IEventHandler
    {
        private Document _doc;
        private PdfFont _font;
        private float _fontSize;
        private string _footerText;

        public InvoiceFooter(Document doc, PdfFont font, string footerText, float fontSize=8f)
        {
            this._doc = doc;
            this._font = font;
            this._fontSize = fontSize;
            this._footerText = footerText;
        }

        public void HandleEvent(Event @event)
        {
            PdfDocumentEvent docEvent = (PdfDocumentEvent)@event;
            PdfCanvas canvas = new PdfCanvas(docEvent.GetPage());
            Rectangle pageSize = docEvent.GetPage().GetPageSize();
            canvas.BeginText();

            canvas.SetFontAndSize(this._font, this._fontSize);

            canvas.MoveText((0+_doc.GetLeftMargin()), (0+ _doc.GetBottomMargin()-10))
                .ShowText($"{this._footerText}, strona {_doc.GetPdfDocument().GetPageNumber(docEvent.GetPage())} z {_doc.GetPdfDocument().GetNumberOfPages()}")
                .EndText()
                .Release();
        }
    }
}


