using System;
using System.Threading.Tasks;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Frosting;

namespace FrostingConfigTest
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            return new CakeHost()
                .UseContext<BuildContext>()
                .InstallTool(new Uri("nuget:?package=ExcelDnaPack&version=1.1.1"))
                .Run(args);
        }
    }

    public class BuildContext : FrostingContext
    {
        public bool Delay { get; set; }

        public BuildContext(ICakeContext context)
            : base(context)
        {
            Delay = context.Arguments.HasArgument("delay");
        }
    }

    [TaskName("Hello")]
    public sealed class HelloTask : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext context)
        {
            context.Log.Information("Hello");
        }
    }

    [TaskName("Dependee")]
    [IsDependeeOf(typeof(WorldTask))]
    public sealed class DependeeTask : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext context)
        {
            context.Log.Information("Dependee of World");
        }
    }

    [TaskName("World")]
    [IsDependentOn(typeof(HelloTask))]
    public sealed class WorldTask : AsyncFrostingTask<BuildContext>
    {
        // Tasks can be asynchronous
        public override async Task RunAsync(BuildContext context)
        {
            if (context.Delay)
            {
                context.Log.Information("Waiting...");
                await Task.Delay(1500);
            }

            context.Log.Information("World");
        }
    }

    [TaskName("Default")]
    [IsDependentOn(typeof(WorldTask))]
    public class DefaultTask : FrostingTask
    {
    }
}