using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting.Messaging;

namespace AspectContext
{
    public class NotificationCatcherProcessor : IPreProcessor, IPostProcessor
    {

        public void Process(ref System.Runtime.Remoting.Messaging.IMethodCallMessage msg)
        {
            
            //msg.LogicalCallContext.SetData(this.GetType().FullName, this);
            CallContext.SetData(this.GetType().FullName, this);
            //throw new NotImplementedException();
        }
        public void Notify(string content)
        {
            Console.WriteLine(content);
        }
        public void Process(System.Runtime.Remoting.Messaging.IMethodCallMessage callMsg, ref System.Runtime.Remoting.Messaging.IMethodReturnMessage retMsg)
        {
            //throw new NotImplementedException();
        }
    }
    public class NotificationSenderProcessor : IPostProcessor
    {
        public void Process(IMethodCallMessage callMsg, ref IMethodReturnMessage retMsg)
        {
            Object obj = CallContext.GetData(typeof(NotificationCatcherProcessor).FullName);
            NotificationCatcherProcessor aspect = (NotificationCatcherProcessor)obj;
            aspect.Notify(String.Format("TracePostProcessor {0} Return:{1}", retMsg.MethodName, retMsg.ReturnValue));
        }
    }
 
    
}
