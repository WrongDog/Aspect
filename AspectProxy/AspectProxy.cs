using System;
using System.Diagnostics;
using System.Collections;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Messaging;

using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Services;

namespace AspectProxy
{
    #region aspect interface

    public interface IProcessor
    {
        void PreProcess(ref IMethodCallMessage msg, MarshalByRefObject target);
        void PostProcess(IMethodCallMessage callMsg, ref IMethodReturnMessage retMsg, MarshalByRefObject target);
    }
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class ProcessAttribute : Attribute
    {
        private IProcessor processor;
        public ProcessAttribute(Type processorType)
        {
            this.processor = Activator.CreateInstance(processorType) as IProcessor;
            if (this.processor == null)
                throw new ArgumentException(String.Format("The type '{0}' does not implement interface IProcessor", processorType.Name, "processorType"));
        }

        public IProcessor Processor
        {
            get { return processor; }
        }

    }
    #endregion


    #region real proxy
    class AspectProxy : RealProxy
    {
        MarshalByRefObject _target = null;

        public AspectProxy(Type type, MarshalByRefObject target)
            : base(type)
        {
            this._target = target;
        }

        public override IMessage Invoke(IMessage msg)
        {
            IMethodCallMessage call = (IMethodCallMessage)msg;
            IConstructionCallMessage ctr = call as IConstructionCallMessage;

            IMethodReturnMessage back = null;


            if (ctr != null)
            {
                //Trace.WriteLine("on " + ctr.ActivationType.Name + " construction");

                RealProxy defaultProxy = RemotingServices.GetRealProxy(_target);


                defaultProxy.InitializeServerObject(ctr);

                back = EnterpriseServicesHelper.CreateConstructionReturnMessage(ctr, (MarshalByRefObject)GetTransparentProxy());
            }

            else
            {
                this.PreProcess(ref call, _target);
                back = RemotingServices.ExecuteMessage(_target, call);
                this.PostProcess(call as IMethodCallMessage, ref back, _target);
            }

            return back;

        }
        private void PreProcess(ref IMethodCallMessage msg, MarshalByRefObject target)
        {

            foreach (var process in (ProcessAttribute[])msg.MethodBase.DeclaringType.GetCustomAttributes(typeof(ProcessAttribute), true))
                process.Processor.PreProcess(ref msg, target);

            foreach (var process in (ProcessAttribute[])msg.MethodBase.GetCustomAttributes(typeof(ProcessAttribute), true))
                process.Processor.PreProcess(ref msg, target);

        }


        private void PostProcess(IMethodCallMessage callMsg, ref IMethodReturnMessage rtnMsg, MarshalByRefObject target)
        {

            foreach (var process in (ProcessAttribute[])callMsg.MethodBase.DeclaringType.GetCustomAttributes(typeof(ProcessAttribute), true))
                process.Processor.PostProcess(callMsg, ref rtnMsg, target);

            foreach (var process in (ProcessAttribute[])callMsg.MethodBase.GetCustomAttributes(typeof(ProcessAttribute), true))
                process.Processor.PostProcess(callMsg, ref rtnMsg, target);

        }


    }

    [AttributeUsage(AttributeTargets.Class)]
    public class AspectProxyAttribute : ProxyAttribute
    {


        public override MarshalByRefObject CreateInstance(Type serverType)
        {
            MarshalByRefObject obj = base.CreateInstance(serverType);
            AspectProxy proxy = new AspectProxy(serverType, obj);
            return (MarshalByRefObject)proxy.GetTransparentProxy();
        }

    }
    #endregion
}
