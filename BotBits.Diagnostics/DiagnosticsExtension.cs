using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BotBits.Diagnostics
{
    public sealed class DiagnosticsExtension : Extension<DiagnosticsExtension>
    {
        public static bool LoadInto(BotBitsClient client)
        {
            return LoadInto(client, null);
        }
    }
}
