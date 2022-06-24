// See https://aka.ms/new-console-template for more information
using System.Text;
using log4net;
using U2.SharpTracker.Core;
using U2.SharpTracker.Core.Storage;
using U2.SharpTracker.Utils;

var runner = new Runner();
runner.Run();

Console.WriteLine("Press Enter to finish.");
Console.ReadLine();

