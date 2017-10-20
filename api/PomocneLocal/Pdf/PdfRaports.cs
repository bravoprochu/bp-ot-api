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

            var pdfWriter = new PdfWriter(ms);
            var pdf = new PdfDocument(pdfWriter);
            var doc = new Document(pdf, PageSize.A4);
            doc.SetMargins(30, 20, 40, 20);
            doc.SetFont(this.FontExoRegular);

            //cols no: 5
            var tblNaglowek = new Table(new float[] { 4, 4, 1, 4, 4 })
                .SetWidthPercent(100)
                .SetFixedLayout();

                

            //doc.Add(tblNaglowek);
            float fSize = 9f;
            tblNaglowek.AddCell(FakCell("Zlecenie przewozowe nr: " + loadDTO.LoadNo, null, fSize * 1.8f, TextAlignment.CENTER, 1, 5));
            tblNaglowek.AddCell(PustaCell(2, 5));
            tblNaglowek.AddCell(FakCell("Zleceniodawca", null, fSize * 1.5f, TextAlignment.CENTER, 2, 2).SetBold());
            tblNaglowek.AddCell(PustaCell(2, 1));
            tblNaglowek.AddCell(FakCell("Zleceniobiorca", null, fSize * 1.5f, TextAlignment.CENTER, 2, 2).SetBold());
            tblNaglowek.AddCell(FakCell(loadDTO.Sell.Principal.Legal_name, null, fSize * 1.2f, TextAlignment.CENTER, 2, 2));
            tblNaglowek.AddCell(PustaCell(2, 1));
            tblNaglowek.AddCell(FakCell(loadDTO.Sell.Selling_info.Company.Legal_name, null, fSize * 1.2f, TextAlignment.CENTER, 2, 2));
            tblNaglowek.AddCell(FakCell(loadDTO.Sell.Principal.AddressList[0].AddressCombined, null, fSize * 0.9f, TextAlignment.CENTER, 1, 2));
            tblNaglowek.AddCell(PustaCell(1, 1));
            tblNaglowek.AddCell(FakCell(loadDTO.Sell.Selling_info.Company.AddressList[0].AddressCombined, null, fSize * 0.9f, TextAlignment.CENTER, 1, 2));
            tblNaglowek.AddCell(FakCell("VAT: " + loadDTO.Sell.Principal.Vat_id, null, fSize * 0.7f, TextAlignment.CENTER, 1, 2));
            tblNaglowek.AddCell(PustaCell(1, 1));
            tblNaglowek.AddCell(FakCell("VAT: " + loadDTO.Sell.Selling_info.Company.Vat_id, null, fSize * 0.7f, TextAlignment.CENTER, 1, 3));
            tblNaglowek.AddCell(FakCell(loadDTO.Sell.Principal.ContactInfo, null, fSize * 0.7f, TextAlignment.CENTER, 1, 2));
            tblNaglowek.AddCell(PustaCell(1, 1));
            tblNaglowek.AddCell(FakCell(loadDTO.Sell.Selling_info.Company.ContactInfo, null, fSize * 0.7f, TextAlignment.CENTER, 1, 2));
            tblNaglowek.AddCell(PustaCell(1, 5));

            var tblRoutes = new Table(new float[] { 2, 2, 2, 2, 1, 1, 2, 2 }) //8 cols
                .SetWidthPercent(100)
                .SetFixedLayout();

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
                    tblRoutes.AddCell(FakCell("Typ palety", null, routeFontSize * 0.5f, TextAlignment.CENTER, 1, 1).SetFontColor(Color.GRAY));
                    tblRoutes.AddCell(FakCell("Ilość", null, routeFontSize * 0.5f, TextAlignment.CENTER, 1, 1).SetFontColor(Color.GRAY));
                    tblRoutes.AddCell(FakCell("Info", null, routeFontSize * 0.5f, TextAlignment.CENTER, 1, 2).SetFontColor(Color.GRAY));
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
                        if (pallet.Type == "Other") { routeInfoArr.Add(pallet.Dimmension); }
                        if (pallet.Type == "Other" && pallet.Is_exchangeable.HasValue && pallet.Is_exchangeable.Value) { routeInfoArr.Add("wymienialna"); }
                        if (pallet.Type == "Other" && pallet.Is_stackable.HasValue && pallet.Is_stackable.Value) { routeInfoArr.Add("piętrowalna"); }
                        rInfo = string.Join(", ", routeInfoArr);
                        routeInfo = palletIdx == 0 ? route.Info : null;

                        tblRoutes.AddCell(FakCell(routeInfo, null, routeFontSize * 0.8f, TextAlignment.LEFT, 1, 3));
                        tblRoutes.AddCell(FakCell(pallet.Type, null, routeFontSize * 0.8f, TextAlignment.CENTER, 1, 1));
                        tblRoutes.AddCell(FakCell(pallet.Amount.ToString(), null, routeFontSize * 0.8f, TextAlignment.CENTER, 1, 1));
                        tblRoutes.AddCell(FakCell(rInfo, null, routeFontSize * 0.8f, TextAlignment.CENTER, 1, 2));

                        palletIdx++;
                    }
                }

                routeIdx++;
                tblRoutes.AddCell(PustaCell(1, 8).SetBorderTop(new SolidBorder(Color.LIGHT_GRAY, 1, 0.5f)));
            }

            var extraInfo = loadDTO.Buy.Load_info.Extra_info;
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
            if (loadInfo.Load_height.HasValue && loadInfo.Load_Length.HasValue && loadInfo.Load_volume.HasValue && loadInfo.Load_weight.HasValue) {
                infoArr.Add($"Wymiary ładunku: wys/dł/obj/ciężar ({loadInfo.Load_height.Value}/{loadInfo.Load_Length.Value}/{loadInfo.Load_volume.Value}/{loadInfo.Load_weight.Value})");
            }

            







            float headerFontSize = 8f;
            string wartosc = loadDTO.Sell.Selling_info.Price.Price.ToString("0.00");

            doc.Add(tblNaglowek);
            doc.Add(HeaderCell($"Wartość: {wartosc} {loadDTO.Sell.Selling_info.Price.Currency_name}", headerFontSize));
            doc.Add(HeaderCell("Trasa:", headerFontSize));
            doc.Add(tblRoutes);
            doc.Add(HeaderCell("Info", headerFontSize));
            doc.Add(FakCell($"[{string.Join(", ", infoArr)}]", "extra info", routeFontSize, TextAlignment.LEFT, 1, 1));
            doc.Add(HeaderCell("Kontakt", headerFontSize));
            foreach (var contactPerson in loadDTO.Sell.Contact_persons.ToList())
            {
                doc.Add(FakCell(contactPerson.CompanyEmployeeInfo, null, routeFontSize, TextAlignment.LEFT, 1, 1));
            }



            doc.Close();
            return ms;
        }




        #region CellsGen

        private static Cell HeaderCell(string text, float fontSize = 6f, int rowSpan = 1, int colspan = 1)
        {
            return new Cell(rowSpan, colspan)
                .Add(new Paragraph(text)
                //.SetFont(font)
                .SetFontSize(fontSize)
                .SetBold()
                .SetTextAlignment(TextAlignment.LEFT)
                .SetMarginLeft(-fontSize * 0.7f)
                .SetBorderBottom(new SolidBorder(Color.DARK_GRAY, 0.3f))
                )
            //.SetPadding(2f)
            //.SetMaxHeight(fontSize*1.3f)
            .SetVerticalAlignment(VerticalAlignment.MIDDLE);
            //.SetBackgroundColor(Color.LIGHT_GRAY);
            //.SetBorderBottom(new SolidBorder(Color.BLUE, 2f));

        }

        private static Cell PozCell(string text, float fontSize, TextAlignment textAlignment, int rowSpan = 1, int colSpan = 1)
        {
            return new Cell(rowSpan, colSpan)
            .Add(new Paragraph().Add(new Text(text)))
            .SetFontSize(fontSize)
            .SetTextAlignment(textAlignment)
            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
            .SetBorder(new SolidBorder(Color.LIGHT_GRAY, 1, 0.5f));
        }

        private static Cell FakCell(string text, string caption, float fontSize, TextAlignment textAlignment, int rowSpan = 1, int colSpan = 1)
        {
            return new Cell(rowSpan, colSpan)
              .Add(new Paragraph().Add(new Text(String.IsNullOrEmpty(caption) ? "" : caption + ": ").SetFontSize(fontSize / 2)).Add(new Text(String.IsNullOrEmpty(text) ? "" : text)).SetFontSize(fontSize))
            //.Add(new Paragraph().Add(new Text(String.IsNullOrEmpty(text)?"": text)).SetFontSize(fontSize).Add(new Text("\n"+caption).SetFontSize(fontSize/2)))
            //.Add(new Paragraph().Add(new Text("data wystawienia").SetFontSize(fontSize/2)))
            .SetPadding(0)
            .SetTextAlignment(textAlignment)
            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
            .SetBorder(Border.NO_BORDER);
        }

        private static Cell PustaCell(int rowSpan = 1, int colSpan = 1)
        {
            return new Cell(rowSpan, colSpan)
                .SetBorder(Border.NO_BORDER);
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
}
