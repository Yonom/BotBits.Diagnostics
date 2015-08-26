using System.Reflection;

namespace BotBits.Diagnostics.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            var bot = new BotBitsClient();
            DiagnosticsExtension.LoadInto(bot);
            EventAnalyzer.Of(bot).AnalyzeAssembly(Assembly.GetAssembly(typeof(BotBitsClient)));
        }
    }

    public sealed class TestEvent : Event<TestEvent>
    {
        
    }
}
