using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Runtime.Remoting.Messaging;

namespace AspectProxy
{
    public abstract class TraceHelper
    {
        public static string FormatInMessage(IMethodCallMessage msg)
        {
            string message = String.Format("Trace {0}.{1} is called",msg.MethodBase.DeclaringType.FullName, msg.MethodName) + System.Environment.NewLine;
            if (msg.InArgCount > 0)
            {
                message += String.Format("In Argument Count {0}", msg.InArgCount) + System.Environment.NewLine;
                for (int idx = 0; idx < msg.InArgCount; idx++)
                {
                    message += String.Format("{0} = {1}", msg.GetInArgName(idx), msg.GetInArg(idx)) + System.Environment.NewLine;
                }
            }
            return message;
        }
        public static string FormatReturnMessage(IMethodCallMessage callMsg,  IMethodReturnMessage retMsg)
        {
            string message = String.Format("Trace {0}.{1} Return:{2}", retMsg.MethodBase.DeclaringType.FullName, retMsg.MethodName, retMsg.ReturnValue) + System.Environment.NewLine;
            if (retMsg.OutArgCount > 0)
            {
                message += String.Format("Out Argument Count {0}", retMsg.OutArgCount) + System.Environment.NewLine;
                for (int idx = 0; idx < retMsg.OutArgCount; idx++)
                {
                    message += String.Format("{0} = {1}", retMsg.GetOutArgName(idx), retMsg.GetOutArg(idx)) + System.Environment.NewLine;
                }
            }
            return message;
        }
    }
    public class TraceProcessor : IProcessor
    {

        [Conditional("DEBUG")]
        protected void PreProcess(ref IMethodCallMessage msg)
        {
            Trace.WriteLine(TraceHelper.FormatInMessage(msg));
          
        }
 
        [Conditional("DEBUG")]
        protected void PostProcess(IMethodCallMessage callMsg, ref IMethodReturnMessage retMsg)
        {
            Trace.WriteLine(TraceHelper.FormatReturnMessage(callMsg,retMsg));
           
        }


        public void PreProcess(ref IMethodCallMessage msg, MarshalByRefObject target)
        {
            PreProcess(ref msg);
        }

        public void PostProcess(IMethodCallMessage callMsg, ref IMethodReturnMessage retMsg, MarshalByRefObject target)
        {
            PostProcess(callMsg, ref retMsg);
        }
    }
  
}
