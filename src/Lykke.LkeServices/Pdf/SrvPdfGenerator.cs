using System;
using System.IO;
using System.Threading.Tasks;
using Core.Clients;

namespace LkeServices.Pdf
{
    //TODO: fix SrvPdfGenerator
    public class SrvPdfGenerator
    {
        private readonly IPersonalDataRepository _personalDataRepository;

        public SrvPdfGenerator(IPersonalDataRepository personalDataRepository)
        {
            _personalDataRepository = personalDataRepository;
        }

        private const string RichardAcc =
            "Richard Olsen, Lykke Corp, Eierbrechtstrasse 50, 8053, Zurich, +41 79 336 89 50, richard.olsen@lykkex.com";
        
        public async Task PrintInvoice(Stream stream, string clientId, double amount, string assetId)
        {
            throw new NotImplementedException();
        }

        //public async Task PrintInvoice(Stream stream, string clientId, double amount, string assetId)
        //{
        //    if (string.IsNullOrEmpty(clientId))
        //        throw new ArgumentNullException(nameof(clientId));
        //    if (amount <= 0)
        //        throw new ArgumentException(nameof(amount));

        //    var account = await _personalDataRepository.GetAsync(clientId);

        //    var pageSize = PageSize.A4;

        //    var doc = new Document(pageSize);
        //    var pdfWriter = PdfWriter.GetInstance(doc, stream);
        //    pdfWriter.CloseStream = false;
        //    doc.Open();

        //    //fonts
        //    BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false);
        //    var titleFont = new Font(bf, 28);
        //    var regularFont = new Font(bf, 9);
        //    var smallRegular = new Font(bf, 7);

        //    //logo
        //    var logoTable = new PdfPTable(1) {WidthPercentage = 85f };

        //    var logoImg = Image.GetInstance(PdfResources.LykkeLogo, ImageFormat.Png);
        //    logoImg.ScaleToFit(120f, 60f);
        //    var cellLogo = new PdfPCell {Border = Rectangle.NO_BORDER};
        //    cellLogo.AddElement(logoImg);
        //    logoTable.AddCell(cellLogo);
        //    doc.Add(logoTable);

        //    //header
        //    var headerTable = new PdfPTable(2);
        //    headerTable.DefaultCell.Border = Rectangle.NO_BORDER;
        //    headerTable.WidthPercentage = 85f;
        //    headerTable.SetWidths(new[] { 70f, 30f });

        //    var titleCell = new PdfPCell(new Phrase("Invoice Summary", titleFont))
        //    {
        //        FixedHeight = 80f,
        //        VerticalAlignment = Element.ALIGN_MIDDLE,
        //        Border = Rectangle.NO_BORDER
        //    };
        //    var invoiceNumCell = new PdfPCell(new Phrase($"# MN_{DateTime.UtcNow.Year}-{DateTime.UtcNow.Month}"))
        //    {
        //        FixedHeight = 80f,
        //        VerticalAlignment = Element.ALIGN_MIDDLE,
        //        HorizontalAlignment = Element.ALIGN_RIGHT,
        //        Border = Rectangle.NO_BORDER
        //    };
        //    headerTable.AddCell(titleCell);
        //    headerTable.AddCell(invoiceNumCell);
        //    doc.Add(headerTable);

        //    //account details
        //    var accountsTable = new PdfPTable(2);
        //    accountsTable.DefaultCell.Border = Rectangle.NO_BORDER;
        //    accountsTable.WidthPercentage = 85f;
        //    accountsTable.SetWidths(new[] { 22f, 78f });

        //    var respondentCell = new PdfPCell(new Phrase("Respondent", smallRegular))
        //    {
        //        Border = Rectangle.NO_BORDER,
        //        FixedHeight = 40f,
        //        VerticalAlignment = Element.ALIGN_MIDDLE
        //    };
        //    var accDetails = $"{account.FullName}, {account.Address}, {account.Country}, {account.Zip}";
        //    var respondentAccDetails = new PdfPCell(new Phrase(accDetails, regularFont))
        //    {
        //        Border = Rectangle.NO_BORDER,
        //        FixedHeight = 40f,
        //        VerticalAlignment = Element.ALIGN_MIDDLE
        //    };
        //    accountsTable.AddCell(respondentCell);
        //    accountsTable.AddCell(respondentAccDetails);

        //    var billToCell = new PdfPCell(new Phrase("Bill to", smallRegular))
        //    {
        //        Border = Rectangle.NO_BORDER,
        //        FixedHeight = 40f,
        //        VerticalAlignment = Element.ALIGN_MIDDLE
        //    };
        //    var billToAccDetails = new PdfPCell(new Phrase(RichardAcc, regularFont))
        //    {
        //        Border = Rectangle.NO_BORDER,
        //        FixedHeight = 40f,
        //        VerticalAlignment = Element.ALIGN_MIDDLE
        //    };
        //    accountsTable.AddCell(billToCell);
        //    accountsTable.AddCell(billToAccDetails);
        //    doc.Add(accountsTable);
        //    doc.Add(new Paragraph("\n"));

        //    //summary
        //    var summaryTable = new PdfPTable(3);
        //    summaryTable.DefaultCell.Border = Rectangle.BOTTOM_BORDER;
        //    summaryTable.WidthPercentage = 85f;
        //    summaryTable.SetWidths(new [] { 60f, 20f, 20f });

        //    var thDescription = new PdfPCell(new Phrase("Description", smallRegular))
        //    {
        //        Border = Rectangle.BOTTOM_BORDER,
        //        FixedHeight = 40f,
        //        VerticalAlignment = Element.ALIGN_MIDDLE
        //    };
        //    var thAmount = new PdfPCell(new Phrase("Amount", smallRegular))
        //    {
        //        Border = Rectangle.BOTTOM_BORDER,
        //        FixedHeight = 40f,
        //        VerticalAlignment = Element.ALIGN_MIDDLE,
        //        HorizontalAlignment = Element.ALIGN_RIGHT
        //    };
        //    var thTotal = new PdfPCell(new Phrase("Total", smallRegular))
        //    {
        //        Border = Rectangle.BOTTOM_BORDER,
        //        FixedHeight = 40f,
        //        VerticalAlignment = Element.ALIGN_MIDDLE,
        //        HorizontalAlignment = Element.ALIGN_RIGHT
        //    };
        //    summaryTable.AddCell(thDescription);
        //    summaryTable.AddCell(thAmount);
        //    summaryTable.AddCell(thTotal);
        //    var tdDescription = new PdfPCell(new Phrase("Received from you a payment", regularFont))
        //    {
        //        Border = Rectangle.BOTTOM_BORDER,
        //        FixedHeight = 35f,
        //        VerticalAlignment = Element.ALIGN_MIDDLE
        //    };
        //    var tdAmount = new PdfPCell(new Phrase($"{amount} {assetId}", regularFont))
        //    {
        //        Border = Rectangle.BOTTOM_BORDER,
        //        FixedHeight = 35f,
        //        VerticalAlignment = Element.ALIGN_MIDDLE,
        //        HorizontalAlignment = Element.ALIGN_RIGHT
        //    };
        //    var tdTotal = new PdfPCell(new Phrase($"{amount} {assetId}", regularFont))
        //    {
        //        Border = Rectangle.BOTTOM_BORDER,
        //        FixedHeight = 35f,
        //        VerticalAlignment = Element.ALIGN_MIDDLE,
        //        HorizontalAlignment = Element.ALIGN_RIGHT
        //    };
        //    summaryTable.AddCell(tdDescription);
        //    summaryTable.AddCell(tdAmount);
        //    summaryTable.AddCell(tdTotal);
        //    doc.Add(summaryTable);

        //    doc.Add(new Paragraph("\n"));

        //    //signature
        //    var signTable = new PdfPTable(2);
        //    signTable.DefaultCell.Border = Rectangle.NO_BORDER;
        //    signTable.WidthPercentage = 85f;
        //    signTable.SetWidths(new [] { 6f, 4f });

        //    var signatureCell = new PdfPCell(new Phrase("Signature", smallRegular))
        //    {
        //        FixedHeight = 50f,
        //        HorizontalAlignment = Element.ALIGN_CENTER,
        //        VerticalAlignment = Element.ALIGN_BOTTOM,
        //        Border = Rectangle.BOX,
        //        PaddingBottom = 10f
        //    };
        //    signTable.AddCell("");
        //    signTable.AddCell(signatureCell);
        //    doc.Add(signTable);

        //    doc.Close();
        //} 
    }
}
