using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Runtime.Remoting.Messaging;

namespace AspectProxy
{
    public abstract class ExceptionHandlingProcessor : IProcessor
    {

        public ExceptionHandlingProcessor()
        {
        }

        public abstract void HandleException(Exception e);

        public virtual Exception GetNewException(Exception oldException)
        {
            return oldException;
        }

        public virtual void PreProcess(ref IMethodCallMessage msg, MarshalByRefObject target)
        {
            //throw new NotImplementedException();
        }

        public virtual void PostProcess(IMethodCallMessage callMsg, ref IMethodReturnMessage retMsg, MarshalByRefObject target)
        {
            Exception e = retMsg.Exception;
            if (e != null)
            {
                this.HandleException(e);

                Exception newException = this.GetNewException(e);
                if (!object.ReferenceEquals(e, newException))
                    retMsg = new ReturnMessage(newException, callMsg);
            }
        }
    }

    public abstract class ExceptionTraceHandlingProcessor : ExceptionHandlingProcessor
    {
        public ExceptionTraceHandlingProcessor()
        { }
        public override void PreProcess(ref IMethodCallMessage msg, MarshalByRefObject target)
        {
            List<string> infoList = new List<string>();
            string name = this.GetType().FullName;
            if (msg.Properties.Contains(name))
            {
                infoList = (List<string>)msg.Properties[name];
                msg.Properties.Remove(name);
            }
            infoList.Add(TraceHelper.FormatInMessage(msg));
            msg.Properties.Add(name, infoList);
        }
       
        public override void PostProcess(IMethodCallMessage callMsg, ref IMethodReturnMessage retMsg, MarshalByRefObject target)
        {


            Exception e = retMsg.Exception;
            if (e != null)
            {
                List<string> infoList = new List<string>();
                string name = this.GetType().FullName;
                if (callMsg.Properties.Contains(name))
                {
                    infoList = (List<string>)callMsg.Properties[name];
                    callMsg.Properties.Remove(name);
                }
                foreach (string content in infoList)
                {
                    Trace.WriteLine(content);
                }
                this.HandleException(e);

                Exception newException = this.GetNewException(e);
                if (!object.ReferenceEquals(e, newException))
                    retMsg = new ReturnMessage(newException, callMsg);
            }

        }
        
       
    }
    public class TraceExceptionProcessor : ExceptionTraceHandlingProcessor
    {

        public override void HandleException(Exception e)
        {
            //TraceException(e);
        }
        private void TraceException(Exception e)
        {
            Trace.WriteLine(string.Format("-->{0}", e.ToString()));
            Trace.WriteLine(e.StackTrace);
            if (e.InnerException != null) TraceException(e.InnerException);
        }
    }
    
    public class ChangeExceptionProcessor : ExceptionHandlingProcessor
    {
        public ChangeExceptionProcessor()
            : base()
        { }

        public override void HandleException(Exception e)
        {
        }

        public override Exception GetNewException(Exception oldException)
        {
            return new ApplicationException("Different");
        }


    }

}
