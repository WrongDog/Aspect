using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Runtime.Remoting.Messaging;

namespace AspectContext
{
    public abstract class ExceptionHandlingProcessor : IPostProcessor
    {

        public ExceptionHandlingProcessor()
        {
        }

        public void Process(IMethodCallMessage callMsg, ref IMethodReturnMessage retMsg)
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

        public abstract void HandleException(Exception e);

        public virtual Exception GetNewException(Exception oldException)
        {
            return oldException;
        }
    }

    public class TraceExceptionProcessor : ExceptionHandlingProcessor
    {
        public TraceExceptionProcessor()
        { }

        public override void HandleException(Exception e)
        {
            Trace.WriteLine(e.ToString());
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
