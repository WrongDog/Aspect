using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace AspectIntercept
{
    public class ConsoleTraceListener : TextWriterTraceListener
    {
        public ConsoleTraceListener()
            : base()
        {
            this.Writer = Console.Out;
        }
    }
}
