using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Remoting.Messaging;

namespace AspectContext
{
    public class CodeTimer
    {
        private DateTime start;
        private string op;
        public CodeTimer()
        {
        }

        public void Start(string op)
        {
            this.op = op;
            this.start = DateTime.Now;
        }

        public void Finish()
        {
            TimeSpan ts = DateTime.Now.Subtract(start);
            Console.WriteLine("Total time for {0}:{1}ms", this.op, ts.TotalMilliseconds);
        }
    }
		
    public class CodeTimerProcessor : IPreProcessor, IPostProcessor
    {
        private CodeTimer _timer;

        public CodeTimerProcessor()
        {

        }

        #region IPreProcessor Members

        void IPreProcessor.Process(ref IMethodCallMessage msg)
        {
            _timer = new CodeTimer();
            msg.Properties.Add("codeTimer", _timer);
            _timer.Start(msg.MethodName);
        }

        #endregion

        #region IPostProcessor Members

        void IPostProcessor.Process(IMethodCallMessage callMsg, ref IMethodReturnMessage retMsg)
        {
            _timer = (CodeTimer)callMsg.Properties["codeTimer"];
            _timer.Finish();
        }

        #endregion
    }
}
