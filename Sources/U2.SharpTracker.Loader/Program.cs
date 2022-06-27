// See https://aka.ms/new-console-template for more information

using U2.SharpTracker.Loader;

var runner = new Runner();
var ctx = new CancellationTokenSource();

var task = Task.Run(() => runner.RunAsync(ctx.Token));
task.Wait();

ctx.Cancel();
