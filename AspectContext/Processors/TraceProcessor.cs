using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Runtime.Remoting.Messaging;

namespace AspectContext
{
    public class TraceProcessor : IPreProcessor, IPostProcessor
    {
        #region IPreProcessor Members

        void IPreProcessor.Process(ref IMethodCallMessage msg)
        {
            
            PreProcess(ref msg);
        }
        [Conditional("DEBUG")]
        protected void PreProcess(ref IMethodCallMessage msg)
        {
            Trace.WriteLine(String.Format("TracePreProcessor {0} is called", msg.MethodName));
            if (msg.InArgCount > 0) Trace.WriteLine(String.Format("In Argument Count {0}", msg.InArgCount));
            for (int idx = 0; idx < msg.InArgCount; idx++)
            {
                Trace.WriteLine(String.Format("{0}={1}", msg.GetInArgName(idx), msg.GetInArg(idx)));
            }
        }
        #endregion

        #region IPostProcessor Members

        void IPostProcessor.Process(IMethodCallMessage callMsg, ref IMethodReturnMessage retMsg)
        {
            PostProcess(callMsg, ref retMsg);
        }
        [Conditional("DEBUG")]
        protected void PostProcess(IMethodCallMessage callMsg, ref IMethodReturnMessage retMsg)
        {
            Trace.WriteLine(String.Format("TracePostProcessor {0} Return:{1}", retMsg.MethodName, retMsg.ReturnValue));
            if (retMsg.OutArgCount > 0) Trace.WriteLine(String.Format("Out Argument Count {0}",retMsg.OutArgCount));
            for (int idx = 0; idx < retMsg.OutArgCount; idx++)
            {
                Trace.WriteLine(String.Format("{0}={1}", retMsg.GetOutArgName(idx), retMsg.GetOutArg(idx)));
            }
        }
        #endregion
    }
  
}
