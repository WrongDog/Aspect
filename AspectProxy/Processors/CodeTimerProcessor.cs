using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Remoting.Messaging;
using System.Diagnostics;

namespace AspectProxy
{
    public class TimeWatcher
    {
        private Stopwatch watcher= new Stopwatch();
        private string operation;
        public TimeWatcher()
        {
        }

        public void Start(string operation)
        {
            this.operation = operation;
            this.watcher.Start();
        }

        public void Finish()
        {
            this.watcher.Stop();
            Trace.WriteLine(string.Format( "Total time for {0}:{1}ms", this.operation, watcher.ElapsedMilliseconds));
        }
    }
		
    public class CodeTimerProcessor : IProcessor{
        private TimeWatcher _timer;

        public CodeTimerProcessor()
        {

        }



        public void PreProcess(ref IMethodCallMessage msg, MarshalByRefObject target)
        {
            _timer = new TimeWatcher();
            msg.Properties.Add("codeTimer", _timer);
            _timer.Start(string.Format("{0}.{1}",msg.MethodBase.DeclaringType.FullName, msg.MethodName));
        }





        public void PostProcess(IMethodCallMessage callMsg, ref IMethodReturnMessage retMsg, MarshalByRefObject target)
        {
            _timer = (TimeWatcher)callMsg.Properties["codeTimer"];
            _timer.Finish();
        }

       
    }
}
