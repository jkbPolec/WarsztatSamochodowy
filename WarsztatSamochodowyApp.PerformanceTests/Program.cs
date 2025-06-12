using NBomber.Contracts.Stats;
using NBomber.CSharp;

namespace WarsztatSamochodowyApp.PerformanceTests;

internal class Program
{
    private static void Main(string[] args)
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };

        using var httpClient = new HttpClient(handler);

        var scenario = Scenario.Create("get_clients", async context =>
            {
                var response = await httpClient.GetAsync("http://localhost:5090/api/clientapi/all");

                return response.IsSuccessStatusCode
                    ? Response.Ok()
                    : Response.Fail();
            })
            .WithWarmUpDuration(TimeSpan.FromSeconds(1))
            .WithLoadSimulations(
                Simulation.Inject(50, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2))
            );

        NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFileName("performance_report_final")
            .WithReportFormats(ReportFormat.Html, ReportFormat.Md)
            .Run();

        Console.WriteLine("Test zakończony");
        Console.ReadKey();
    }
}