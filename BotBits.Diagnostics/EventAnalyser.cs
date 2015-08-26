using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BotBits.Diagnostics
{
    public sealed class EventAnalyser : Package<EventAnalyser>
    {
        private static readonly MethodInfo _bindMethod =
            typeof(EventAnalyser).GetMethod("ScanEvent", BindingFlags.NonPublic | BindingFlags.Instance);


        public void AnalyseAssembly(Assembly assembly)
        {
            var events = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a
                    .GetTypes()
                    .Where(t => !t.IsAbstract)
                    .Where(t => !t.IsGenericType)
                    .Where(t => IsSubclassOfRawGeneric(typeof(Event<>), t)));
            foreach (var type in events)
            {
                MethodInfo genericBind = _bindMethod.MakeGenericMethod(type);
                genericBind.Invoke(this, new object[]{assembly});
            }
        }

        private void ScanEvent<TEvent>(Assembly assembly) where TEvent : Event<TEvent>
        {
            var eventHandle = Event<TEvent>.Of(this.BotBits);
            var eventHandlersField = typeof(EventHandle<TEvent>).GetField("_eventHandlers",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var eventHandlers =
                (SortedDictionary<GlobalPriority,
                SortedDictionary<int,
                    SortedDictionary<EventPriority,
                        IList<EventRaiseHandler<TEvent>>>>>)
                    eventHandlersField.GetValue(eventHandle);

            foreach (var priority in eventHandlers.Values.SelectMany(ex => ex.Values.SelectMany(pr => pr)))
            {
                EventRaiseHandler<TEvent> last = null;
                foreach (var handler in priority.Value)
                {
                    if (handler.GetMethodInfo().DeclaringType.Assembly == assembly)
                    {
                        if (last != null)
                            Console.WriteLine(
                                "Duplicate usage for {0}({1}): {2} vs {3}",
                                typeof(TEvent),
                                priority.Key,
                                GetString(last),
                                GetString(handler));
                    }
                    last = handler;
                }
            }
        }

        private static string GetString(Delegate del)
        {
            var info = del.GetMethodInfo();
            var type = info.DeclaringType;
            return type.FullName + "." + info.Name;
        }

        static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }
    }
}
