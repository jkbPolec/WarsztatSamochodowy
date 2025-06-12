using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF;
using WarsztatSamochodowyApp.DTO;

namespace WarsztatSamochodowyApp.Services.Pdf;

public class MonthlyRepairPdfExporter
{
    public byte[] Generate(int year, int month, List<MonthlyRepairSummaryDto> entries)
    {
        // Ustaw licencję (Community, jeśli używasz darmowej wersji)
        QuestPDF.Settings.License = LicenseType.Community;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);

                page.Header().Text($"Raport napraw - {month:00}/{year}")
                    .FontSize(20).Bold().AlignCenter();

                page.Content().PaddingVertical(10).Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(); // Klient
                        cols.RelativeColumn(); // Pojazd
                        cols.ConstantColumn(80); // Liczba zleceń
                        cols.ConstantColumn(100); // Koszt
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("Klient").Bold();
                        header.Cell().Text("Pojazd").Bold();
                        header.Cell().AlignRight().Text("Zleceń").Bold();
                        header.Cell().AlignRight().Text("Koszt").Bold();
                    });

                    foreach (var item in entries)
                    {
                        table.Cell().Text(item.ClientFullName);
                        table.Cell().Text(item.VehicleIdentifier);
                        table.Cell().AlignRight().Text(item.OrdersCount.ToString());
                        table.Cell().AlignRight().Text($"{item.TotalCost:C}");
                    }
                });

                page.Footer()
                    .AlignCenter()
                    .Text($"Wygenerowano: {DateTime.Now:yyyy-MM-dd HH:mm}");
            });
        }).GeneratePdf();
    }
}
