using System.Linq;
using Serilog.Core;
using Serilog.Events;

namespace Exira.Serilog
{
    class TypeTagRemover : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory factory)
        {
            // The ToArray() might be avoidable...
            foreach (var prop in logEvent.Properties.ToArray())
            {
                var val = prop.Value as StructureValue;
                if (val?.TypeTag != null)
                {
                    var replacement = new LogEventProperty(prop.Key, new StructureValue(val.Properties));
                    logEvent.AddOrUpdateProperty(replacement);
                }
            }
        }
    }

}
