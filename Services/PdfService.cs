using InvoiceSystem.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace InvoiceSystem.Services
{
    public class PdfService
    {
        public byte[] GenerateInvoicePdf(Invoice invoice)
        {
            // Set QuestPDF license (free for open source)
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header()
                        .AlignCenter()
                        .Text("INVOICE")
                        .SemiBold().FontSize(24).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(column =>
                        {
                            column.Spacing(20);

                            // Invoice header
                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text($"Invoice #: {invoice.InvoiceNumber}");
                                    col.Item().Text($"Issue Date: {invoice.IssueDate:dd/MM/yyyy}");
                                    col.Item().Text($"Due Date: {invoice.DueDate:dd/MM/yyyy}");
                                    col.Item().Text($"Status: {invoice.Status}");
                                });

                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("Bill To:").SemiBold();
                                    col.Item().Text(invoice.CustomerName);
                                });
                            });

                            // Line separator
                            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                            // Invoice items table
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(25); // #
                                    columns.RelativeColumn(3); // Description
                                    columns.ConstantColumn(80); // Quantity
                                    columns.ConstantColumn(80); // Unit Price
                                    columns.ConstantColumn(80); // Total
                                });

                                // Header
                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("#").SemiBold();
                                    header.Cell().Element(CellStyle).Text("Description").SemiBold();
                                    header.Cell().Element(CellStyle).AlignRight().Text("Qty").SemiBold();
                                    header.Cell().Element(CellStyle).AlignRight().Text("Unit Price").SemiBold();
                                    header.Cell().Element(CellStyle).AlignRight().Text("Total").SemiBold();

                                    static IContainer CellStyle(IContainer container)
                                    {
                                        return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                                    }
                                });

                                // Items
                                int itemNumber = 1;
                                foreach (var item in invoice.Items)
                                {
                                    table.Cell().Element(CellStyle).Text(itemNumber.ToString());
                                    table.Cell().Element(CellStyle).Text(item.Description);
                                    table.Cell().Element(CellStyle).AlignRight().Text(item.Quantity.ToString());
                                    table.Cell().Element(CellStyle).AlignRight().Text($"${item.UnitPrice:F2}");
                                    table.Cell().Element(CellStyle).AlignRight().Text($"${item.TotalPrice:F2}");

                                    itemNumber++;

                                    static IContainer CellStyle(IContainer container)
                                    {
                                        return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                                    }
                                }
                            });

                            // Total amount
                            column.Item().AlignRight().Text($"Total Amount: ${invoice.TotalAmount:F2}").SemiBold().FontSize(14);

                            // Notes
                            column.Item().PaddingTop(20).Column(col =>
                            {
                                col.Item().Text("Notes:").SemiBold();
                                col.Item().Text("Thank you for your business!");
                            });
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("Page ");
                            text.CurrentPageNumber();
                            text.Span(" of ");
                            text.TotalPages();
                        });
                });
            });

            return document.GeneratePdf();
        }
    }
}