using Sentry;
using Sentry.Protocol;
using Serilog;
using Serilog.Events;
using System;
using System.Windows;

namespace SentrySerilogWpfTest
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // TODO: set your DSN
        private const string Dsn = "";

        private IDisposable sentry;

        protected override void OnStartup(StartupEventArgs e)
        {
            ConfigureSerilog(@"C:\Temp\wpftest.log");
            ConfigureSentry();

            Log.Logger.Error("OnStartup (user.id and tag are fine)");
        }

        private void ConfigureSerilog(string filePath)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.File
                (
                    path: filePath,
                    shared: true,
                    restrictedToMinimumLevel: LogEventLevel.Verbose,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                )
                .WriteTo.Sentry(o =>
                {
                    o.MinimumBreadcrumbLevel = LogEventLevel.Verbose;
                    o.MinimumEventLevel = LogEventLevel.Error;
                })
                .CreateLogger();
        }

        private void ConfigureSentry()
        {
            sentry = SentrySdk.Init(o =>
            {
                o.Dsn = new Dsn(Dsn);
            });

            SentrySdk.ConfigureScope(scope =>
            {
                scope.User = new User() { Id = "MyUniqueUserId" };
                scope.SetTag("myTagKey", "myTagValue");
            });
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Logger.Error("OnExit: user.id and custom tag are missing!");
            sentry.Dispose();
        }
    }
}
