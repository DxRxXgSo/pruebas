using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Pdf.API.Services;

public static class PdfTicketGenerator
{
    static PdfTicketGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public static byte[] GenerateOrderTicket(OrderDto order, string storeName, decimal taxRate)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A6);
                page.Margin(16);
                page.DefaultTextStyle(style => style.FontSize(9).FontColor(Colors.Grey.Darken3));

                page.Header().Column(column =>
                {
                    column.Item().AlignCenter().Text(storeName).FontSize(16).Bold().FontColor(Colors.Blue.Darken2);
                    column.Item().AlignCenter().Text("Ticket de compra").FontSize(10).SemiBold();
                    column.Item().AlignCenter().Text($"Orden: {order.Id}").FontSize(7).FontColor(Colors.Grey.Darken2);
                    column.Item().AlignCenter().Text($"Fecha: {order.CreatedAt:dd/MM/yyyy HH:mm}").FontSize(7).FontColor(Colors.Grey.Darken2);
                    column.Item().AlignCenter().Text($"Cliente: {order.CustomerId}").FontSize(7).FontColor(Colors.Grey.Darken2);
                    column.Item().PaddingVertical(6).LineHorizontal(0.8f).LineColor(Colors.Grey.Lighten1);
                });

                page.Content().Column(column =>
                {
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.ConstantColumn(34);
                            columns.ConstantColumn(42);
                            columns.ConstantColumn(50);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderCellStyle).Text("Producto").Bold();
                            header.Cell().Element(HeaderCellStyle).AlignRight().Text("Cant.").Bold();
                            header.Cell().Element(HeaderCellStyle).AlignRight().Text("P. unit").Bold();
                            header.Cell().Element(HeaderCellStyle).AlignRight().Text("Importe").Bold();
                        });

                        foreach (var item in order.Items)
                        {
                            table.Cell().PaddingVertical(2).Text(item.ProductName).FontSize(8);
                            table.Cell().PaddingVertical(2).AlignRight().Text(item.Quantity.ToString()).FontSize(8);
                            table.Cell().PaddingVertical(2).AlignRight().Text($"${item.UnitPrice:F2}").FontSize(8);
                            table.Cell().PaddingVertical(2).AlignRight().Text($"${item.LineTotal:F2}").FontSize(8).Bold();
                        }
                    });

                    column.Item().PaddingVertical(4).LineHorizontal(0.8f).LineColor(Colors.Grey.Lighten1);

                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Subtotal").SemiBold();
                        row.ConstantItem(90).AlignRight().Text($"${order.Subtotal:F2}");
                    });
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Text($"Impuestos ({taxRate:P0})").SemiBold();
                        row.ConstantItem(90).AlignRight().Text($"${order.Tax:F2}");
                    });
                    column.Item().PaddingVertical(2).Row(row =>
                    {
                        row.RelativeItem().Text("TOTAL").FontSize(11).Bold().FontColor(Colors.Blue.Darken2);
                        row.ConstantItem(90).AlignRight().Text($"${order.Total:F2}").FontSize(11).Bold().FontColor(Colors.Blue.Darken2);
                    });

                    column.Item().PaddingTop(6).Row(row =>
                    {
                        row.RelativeItem().Text("Estado").SemiBold();
                        row.ConstantItem(90).AlignRight().Text(order.Status);
                    });
                });

                page.Footer().Column(column =>
                {
                    column.Item().PaddingVertical(4).LineHorizontal(0.8f).LineColor(Colors.Grey.Lighten1);
                    column.Item().AlignCenter().Text("¡Gracias por su compra!").FontSize(8).SemiBold();
                    column.Item().AlignCenter().Text($"Artículos: {order.Items.Sum(i => i.Quantity)}").FontSize(7).FontColor(Colors.Grey.Darken2);
                });
            });
        }).GeneratePdf();
    }

    public static byte[] GenerateCustomerSummaryTicket(List<OrderDto> orders, string customerId, string storeName)
    {
        var totalGeneral = orders.Sum(o => o.Total);

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A5);
                page.Margin(16);
                page.DefaultTextStyle(style => style.FontSize(9).FontColor(Colors.Grey.Darken3));

                page.Header().Column(column =>
                {
                    column.Item().AlignCenter().Text(storeName).FontSize(16).Bold().FontColor(Colors.Blue.Darken2);
                    column.Item().AlignCenter().Text("Resumen de compras").FontSize(10).SemiBold();
                    column.Item().AlignCenter().Text($"Cliente: {customerId}").FontSize(7).FontColor(Colors.Grey.Darken2);
                    column.Item().AlignCenter().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(7).FontColor(Colors.Grey.Darken2);
                    column.Item().PaddingVertical(6).LineHorizontal(0.8f).LineColor(Colors.Grey.Lighten1);
                });

                page.Content().Column(column =>
                {
                    foreach (var order in orders)
                    {
                        column.Item().PaddingBottom(8).Column(orderColumn =>
                        {
                            orderColumn.Item().Text($"Orden {order.Id}").Bold().FontSize(10).FontColor(Colors.Blue.Darken2);
                            orderColumn.Item().Text($"Fecha: {order.CreatedAt:dd/MM/yyyy HH:mm}  ·  Estado: {order.Status}").FontSize(7).FontColor(Colors.Grey.Darken2);

                            orderColumn.Item().PaddingVertical(3).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.ConstantColumn(34);
                                    columns.ConstantColumn(42);
                                    columns.ConstantColumn(50);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(HeaderCellStyle).Text("Producto").Bold();
                                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("Cant.").Bold();
                                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("P. unit").Bold();
                                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("Importe").Bold();
                                });

                                foreach (var item in order.Items)
                                {
                                    table.Cell().PaddingVertical(2).Text(item.ProductName).FontSize(8);
                                    table.Cell().PaddingVertical(2).AlignRight().Text(item.Quantity.ToString()).FontSize(8);
                                    table.Cell().PaddingVertical(2).AlignRight().Text($"${item.UnitPrice:F2}").FontSize(8);
                                    table.Cell().PaddingVertical(2).AlignRight().Text($"${item.LineTotal:F2}").FontSize(8).Bold();
                                }
                            });

                            orderColumn.Item().AlignRight().Text($"Total: ${order.Total:F2}").SemiBold();
                        });
                    }
                });

                page.Footer().Column(column =>
                {
                    column.Item().PaddingVertical(4).LineHorizontal(0.8f).LineColor(Colors.Grey.Lighten1);
                    column.Item().AlignCenter().Text($"Órdenes: {orders.Count}  ·  TOTAL GENERAL: ${totalGeneral:F2}")
                        .FontSize(9).Bold().FontColor(Colors.Blue.Darken2);
                });
            });
        }).GeneratePdf();
    }

    private static IContainer HeaderCellStyle(IContainer container) =>
        container.Background(Colors.Grey.Lighten3).PaddingVertical(2).PaddingHorizontal(3).BorderBottom(0.8f);
}