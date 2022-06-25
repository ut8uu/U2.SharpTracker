using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using U2.SharpTracker.Core;

namespace U2.SharpTracker.Utils;

public class Runner
{
    private Task _task;
    private CancellationTokenSource _cancellationTokenSource;

    public void Run()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        var startKey = ConsoleKey.D1;

        var mainMenu = new List<ConsoleManagementElement>
            {
                new()
                {
                    Function = RuTrackerFunctions,
                    Key = startKey++,
                    Title = "RuTracker",
                },

            };

        ManageFlow(mainMenu);
    }

    public void Stop()
    {
        _cancellationTokenSource.Cancel();
    }

    void Log(string message)
    {
        LogManager.GetLogger(typeof(Runner)).Info(message);
    }

    void ManageFlow(List<ConsoleManagementElement> input)
    {
        var output = new StringBuilder();
        foreach (var el in input)
        {
            output.Append($"{el.Key}. {el.Title}");
            output.AppendLine();
        }

        output.AppendLine();
        output.AppendLine("X. Exit");

        while (true)
        {
            Console.Clear();
            Console.WriteLine(output);

            var key = Console.ReadKey();
            if (input.Any(o => o.Key == key.Key))
            {
                var func = input.FirstOrDefault(o => o.Key == key.Key);
                if (func != null)
                {
                    Console.Clear();
                    Console.WriteLine(func.Title);
                    Console.WriteLine("---------------------------------------");

                    func.Function(func.FunctionParameters);

                    Console.WriteLine("---------------------------------------");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                }
            }
            else if (key.Key == ConsoleKey.X)
            {
                break;
            }
            else
            {
                Console.WriteLine("Unrecognized input");
                Console.WriteLine("");
            }
        }
    }

    bool RuTrackerFunctions(object[] parameters)
    {
        var rutrackerMenu = new List<ConsoleManagementElement>
    {
        new()
        {
            Function = RTPerBranchStrategy,
            Key = ConsoleKey.A,
            Title = "Per-branch strategy",
        },

    };
        ManageFlow(rutrackerMenu);

        return true;
    }

    void StrategyOnUserInputRequired(object sender, UserInputRequiredEventArgs eventArgs)
    {
        Console.WriteLine("------------------------------------");
        Console.Write(eventArgs.MessageToUser);
        eventArgs.UserInput = "84";//Console.ReadLine();
        Console.WriteLine("------------------------------------");
    }

    void StrategyOnProgressReported(object sender, ProgressReportedEventArgs eventArgs)
    {
        Console.WriteLine($"{eventArgs.Progress}% {eventArgs.Text}");
    }

    void StrategyOnInternetResourceContentRequired(object sender, InternetResourceContentRequiredEventArgs eventArgs)
    {
        var task = Task.Run(async () =>
        {
            eventArgs.ResourceContent = await DownloadUrlAsync(eventArgs.UrlInfo);
            eventArgs.UrlInfo.UrlLoadStatusCode = UrlLoadStatusCode.Success;
            eventArgs.UrlInfo.UrlLoadState = UrlLoadState.Loaded;
        });
        task.Wait();
    }

    async Task<string> DownloadUrlAsync(UrlInfo url)
    {
        try
        {
            var client = new HttpClient();
            var response = await client.GetAsync(url.Url);
            var content = await response.Content.ReadAsByteArrayAsync();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var win1251 = Encoding.GetEncoding("windows-1251");
            var responseString = win1251.GetString(content, 0, content.Length);
            return responseString;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    bool RTPerBranchStrategy(object[] parameters)
    {
        _task = Task.Run(() =>
            RTPerBranchStrategyAsync(null),
            _cancellationTokenSource.Token);

        return true;
    }

    async Task<bool> RTPerBranchStrategyAsync(object[] parameters)
    {
        var dbDirectory = FileSystemHelper.GetDatabaseFolderPath();
        var runner = new MongoDBRunner(dbDirectory);
        runner.Start();

        var settings = new DatabaseSettings
        {
            ConnectionString = UtilsAppSettings.Default.ConnectionString,
            DatabaseName = "RuTracker",
        };
        var database = MongoDatabaseHelper.CreateDatabase(settings);
        var strategy = new RTPerBranchStrategy(database);

        strategy.UserInputRequired += StrategyOnUserInputRequired;
        strategy.ProgressReported += StrategyOnProgressReported;
        strategy.InternetResourceContentRequired += StrategyOnInternetResourceContentRequired;
        await strategy.StartAsync();

        runner.Stop();

        return true;
    }
}
