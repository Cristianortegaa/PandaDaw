using PandaBack.Dtos.Ventas;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PandaBack.Services.Factura;

public class FacturaService : IFacturaService
{
    public byte[] GenerarFacturaPdf(VentaResponseDto venta)
    {
        var subtotal = venta.Lineas.Sum(l => l.Subtotal);
        var iva = subtotal * 0.21m;
        var total = subtotal + iva;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Grey.Darken3));

                // Header
                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(left =>
                        {
                            left.Item().Text("PandaDaw").Bold().FontSize(24).FontColor(Colors.Green.Darken2);
                            left.Item().Text("Tu tienda de confianza").FontSize(9).FontColor(Colors.Grey.Medium);
                        });

                        row.RelativeItem().AlignRight().Column(right =>
                        {
                            right.Item().Text("FACTURA").Bold().FontSize(18).FontColor(Colors.Grey.Darken2);
                            right.Item().Text($"Nº {venta.Id:D6}").FontSize(10).FontColor(Colors.Grey.Medium);
                            right.Item().Text($"Fecha: {venta.FechaCompra:dd/MM/yyyy HH:mm}").FontSize(9).FontColor(Colors.Grey.Medium);
                        });
                    });

                    col.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                // Content
                page.Content().Column(col =>
                {
                    // Datos del cliente
                    col.Item().PaddingBottom(15).Column(datos =>
                    {
                        datos.Item().Text("Datos del cliente").Bold().FontSize(11).FontColor(Colors.Grey.Darken2);
                        datos.Item().PaddingTop(5).Text($"Nombre: {venta.UsuarioNombre}").FontSize(9);
                        datos.Item().Text($"Email: {venta.UsuarioEmail}").FontSize(9);
                        datos.Item().Text($"ID Pedido: #{venta.Id}").FontSize(9);
                    });

                    // Tabla de productos
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(4); // Producto
                            columns.RelativeColumn(1); // Cantidad
                            columns.RelativeColumn(1.5f); // Precio Unit.
                            columns.RelativeColumn(1.5f); // Subtotal
                        });

                        // Header de tabla
                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Green.Darken2).Padding(8)
                                .Text("Producto").Bold().FontSize(9).FontColor(Colors.White);
                            header.Cell().Background(Colors.Green.Darken2).Padding(8)
                                .AlignCenter().Text("Cant.").Bold().FontSize(9).FontColor(Colors.White);
                            header.Cell().Background(Colors.Green.Darken2).Padding(8)
                                .AlignRight().Text("Precio Unit.").Bold().FontSize(9).FontColor(Colors.White);
                            header.Cell().Background(Colors.Green.Darken2).Padding(8)
                                .AlignRight().Text("Subtotal").Bold().FontSize(9).FontColor(Colors.White);
                        });

                        // Filas
                        var isAlternate = false;
                        foreach (var linea in venta.Lineas)
                        {
                            var bg = isAlternate ? Colors.Grey.Lighten4 : Colors.White;

                            table.Cell().Background(bg).Padding(8)
                                .Text(linea.ProductoNombre).FontSize(9);
                            table.Cell().Background(bg).Padding(8)
                                .AlignCenter().Text(linea.Cantidad.ToString()).FontSize(9);
                            table.Cell().Background(bg).Padding(8)
                                .AlignRight().Text($"{linea.PrecioUnitario:C}").FontSize(9);
                            table.Cell().Background(bg).Padding(8)
                                .AlignRight().Text($"{linea.Subtotal:C}").FontSize(9);

                            isAlternate = !isAlternate;
                        }
                    });

                    // Totales
                    col.Item().PaddingTop(15).AlignRight().Width(200).Column(totales =>
                    {
                        totales.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Subtotal:").FontSize(10);
                            row.RelativeItem().AlignRight().Text($"{subtotal:C}").FontSize(10);
                        });
                        totales.Item().PaddingVertical(3).Row(row =>
                        {
                            row.RelativeItem().Text("IVA (21%):").FontSize(10);
                            row.RelativeItem().AlignRight().Text($"{iva:C}").FontSize(10);
                        });
                        totales.Item().PaddingTop(5).BorderTop(1).BorderColor(Colors.Grey.Lighten2)
                            .PaddingTop(5).Row(row =>
                            {
                                row.RelativeItem().Text("TOTAL:").Bold().FontSize(12).FontColor(Colors.Green.Darken2);
                                row.RelativeItem().AlignRight().Text($"{total:C}").Bold().FontSize(12).FontColor(Colors.Green.Darken2);
                            });
                    });

                    // Estado
                    col.Item().PaddingTop(25).Column(estado =>
                    {
                        estado.Item().Text($"Estado del pedido: {venta.Estado}").Bold().FontSize(10).FontColor(Colors.Grey.Darken1);
                    });
                });

                // Footer
                page.Footer().Column(col =>
                {
                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    col.Item().PaddingTop(8).Row(row =>
                    {
                        row.RelativeItem().Text("PandaDaw © 2026 — Gracias por tu compra")
                            .FontSize(8).FontColor(Colors.Grey.Medium);
                        row.RelativeItem().AlignRight()
                            .Text($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}")
                            .FontSize(8).FontColor(Colors.Grey.Medium);
                    });
                });
            });
        });

        return document.GeneratePdf();
    }
}
