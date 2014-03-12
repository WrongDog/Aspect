using System;
using System.Diagnostics;
using System.Collections;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Messaging;
using System.Threading;

namespace AspectContext
{
    public interface IPreProcessor
    {
        void Process(ref IMethodCallMessage msg);
    }

    public interface IPostProcessor
    {
        void Process(IMethodCallMessage callMsg, ref IMethodReturnMessage retMsg);
    }

    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class PreProcessAttribute : Attribute
    {
        private IPreProcessor preProcesser;
        public PreProcessAttribute(Type preProcessorType)
        {
            this.preProcesser = Activator.CreateInstance(preProcessorType) as IPreProcessor;
            if (this.preProcesser == null)
                throw new ArgumentException(String.Format("The type '{0}' does not implement interface IPreProcessor", preProcessorType.Name, "processorType"));
        }

        public IPreProcessor Processor
        {
            get { return preProcesser; }
        }
    }

    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class PostProcessAttribute : Attribute
    {
        private IPostProcessor postProcessor;
        public PostProcessAttribute(Type postProcessorType)
        {
            this.postProcessor = Activator.CreateInstance(postProcessorType) as IPostProcessor;
            if (this.postProcessor == null)
                throw new ArgumentException(String.Format("The type '{0}' does not implement interface IPostProcessor", postProcessorType.Name, "processorType"));
        }

        public IPostProcessor Processor
        {
            get { return postProcessor; }
        }
    }

    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class ProcessAttribute : Attribute
    {
        private IPreProcessor preProcesser;
        private IPostProcessor postProcessor;
        public ProcessAttribute(Type processorType)
        {
            this.postProcessor = Activator.CreateInstance(processorType) as IPostProcessor;
            if (this.postProcessor == null)
                throw new ArgumentException(String.Format("The type '{0}' does not implement interface IPostProcessor", processorType.Name, "processorType"));
            this.preProcesser = Activator.CreateInstance(processorType) as IPreProcessor;
            if (this.preProcesser == null)
                throw new ArgumentException(String.Format("The type '{0}' does not implement interface IPreProcessor", processorType.Name, "processorType"));
        }

        public IPostProcessor PostProcessor
        {
            get { return postProcessor; }
        }
        public IPreProcessor PreProcessor
        {
            get { return preProcesser; }
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class InterceptAttribute : ContextAttribute
    {
        //protected Type defaultProcessor;
        //public InterceptAttribute(Type defaultProcessor)
        //    : base("Intercept")
        //{
        //    if (!typeof(IPreProcessor).IsAssignableFrom(defaultProcessor)) throw new ArgumentException(String.Format("The type '{0}' does not implement interface IPreProcessor", defaultProcessor.Name, "processorType"));
        //    if (!typeof(IPostProcessor).IsAssignableFrom(defaultProcessor)) throw new ArgumentException(String.Format("The type '{0}' does not implement interface IPostProcessor", defaultProcessor.Name, "processorType"));
        //    this.defaultProcessor = defaultProcessor;
        //}
        public InterceptAttribute()
            : base("Intercept")
        {
        }

        public override void Freeze(Context newContext)
        {
        }

        public override void GetPropertiesForNewContext(System.Runtime.Remoting.Activation.IConstructionCallMessage ctorMsg)
        {
            ctorMsg.ContextProperties.Add(new InterceptProperty());
        }

        public override bool IsContextOK(Context ctx, System.Runtime.Remoting.Activation.IConstructionCallMessage ctorMsg)
        {
            InterceptProperty p = ctx.GetProperty("Intercept") as InterceptProperty;
            if (p == null)
                return false;
            return true;
        }

        public override bool IsNewContextOK(Context newCtx)
        {
            InterceptProperty p = newCtx.GetProperty("Intercept") as InterceptProperty;
            if (p == null)
                return false;
            return true;
        }


    }

    //IContextProperty, IContributeServerContextSink
    public class InterceptProperty : IContextProperty, IContributeObjectSink
    {
        public InterceptProperty()
            : base()
        {
        }
        #region IContextProperty Members

        public string Name
        {
            get
            {
                return "Intercept";
            }
        }

        public bool IsNewContextOK(Context newCtx)
        {
            InterceptProperty p = newCtx.GetProperty("Intercept") as InterceptProperty;
            if (p == null)
                return false;
            return true;
        }

        public void Freeze(Context newContext)
        {
        }

        #endregion

        #region IContributeObjectSink Members

        public System.Runtime.Remoting.Messaging.IMessageSink GetObjectSink(MarshalByRefObject obj, System.Runtime.Remoting.Messaging.IMessageSink nextSink)
        {
            return new InterceptSink(nextSink);
        }

        #endregion
    }

    public class InterceptSink : IMessageSink
    {
        private IMessageSink nextSink;

        public InterceptSink(IMessageSink nextSink)
        {
            this.nextSink = nextSink;
        }

        #region IMessageSink Members

        public IMessage SyncProcessMessage(IMessage msg)
        {
            IMethodCallMessage mcm = (msg as IMethodCallMessage);
            this.PreProcess(ref mcm);
            IMessage rtnMsg = nextSink.SyncProcessMessage(msg);
            IMethodReturnMessage mrm = (rtnMsg as IMethodReturnMessage);
            this.PostProcess(msg as IMethodCallMessage, ref mrm);
            return mrm;
        }

        public IMessageSink NextSink
        {
            get
            {
                return this.nextSink;
            }
        }

        public IMessageCtrl AsyncProcessMessage(IMessage msg, IMessageSink replySink)
        {
            IMessageCtrl rtnMsgCtrl = nextSink.AsyncProcessMessage(msg, replySink);
            return rtnMsgCtrl;
        }

        #endregion

        private void PreProcess(ref IMethodCallMessage msg)
        {
            foreach (var process in (PreProcessAttribute[])msg.MethodBase.DeclaringType.GetCustomAttributes(typeof(PreProcessAttribute), true)) process.Processor.Process(ref msg);
            foreach (var process in (ProcessAttribute[])msg.MethodBase.DeclaringType.GetCustomAttributes(typeof(ProcessAttribute), true)) process.PreProcessor.Process(ref msg);
            foreach (var process in (PreProcessAttribute[])msg.MethodBase.GetCustomAttributes(typeof(PreProcessAttribute), true)) process.Processor.Process(ref msg);
            foreach (var process in (ProcessAttribute[])msg.MethodBase.GetCustomAttributes(typeof(ProcessAttribute), true)) process.PreProcessor.Process(ref msg);

        }

        private void PostProcess(IMethodCallMessage callMsg, ref IMethodReturnMessage rtnMsg)
        {
            foreach (var process in (PostProcessAttribute[])callMsg.MethodBase.DeclaringType.GetCustomAttributes(typeof(PostProcessAttribute), true)) process.Processor.Process(callMsg, ref rtnMsg);
            foreach (var process in (ProcessAttribute[])callMsg.MethodBase.DeclaringType.GetCustomAttributes(typeof(ProcessAttribute), true)) process.PostProcessor.Process(callMsg, ref rtnMsg);
            foreach (var process in (PostProcessAttribute[])callMsg.MethodBase.GetCustomAttributes(typeof(PostProcessAttribute), true)) process.Processor.Process(callMsg, ref rtnMsg);
            foreach (var process in (ProcessAttribute[])callMsg.MethodBase.GetCustomAttributes(typeof(ProcessAttribute), true)) process.PostProcessor.Process(callMsg, ref rtnMsg);

        }

    }
}
