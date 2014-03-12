using System;
using System.Diagnostics;
using System.Collections;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Messaging;

namespace AspectIntercept
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
			Console.WriteLine("Total time for {0}:{1}ms",this.op,ts.TotalMilliseconds);
		}
	}
		
	public class ConsoleTraceListener : TextWriterTraceListener
	{
		public ConsoleTraceListener() : base()
		{
			this.Writer = Console.Out;
		}
	}

	

	
	public interface IPreProcessor
	{
		void Process(ref IMethodCallMessage msg);
	}

	public interface IPostProcessor
	{
		void Process(IMethodCallMessage callMsg,ref IMethodReturnMessage retMsg);
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
			msg.Properties.Add("codeTimer",_timer);
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

	public class TracePreProcessor : IPreProcessor
	{

		public TracePreProcessor()
		{}
		#region IPreProcessor Members

		public void Process(ref IMethodCallMessage msg)
		{
			this.TraceMethod(msg.MethodName);
		}

		#endregion
		[Conditional("DEBUG")]
		private void TraceMethod(string method)
		{
			Trace.WriteLine(String.Format("PreProcessing:{0}",method));
		}
	}

	public class TracePostProcessor : IPostProcessor
	{
		public TracePostProcessor()
		{}
		public void Process(IMethodCallMessage callMsg,ref IMethodReturnMessage retMsg)
		{
			Trace.WriteLine(String.Format("Return:{0}",retMsg.ReturnValue));
		}
	}

	public abstract class ExceptionHandlingProcessor : IPostProcessor
	{

		public ExceptionHandlingProcessor()
		{
		}

		public void Process(IMethodCallMessage callMsg,ref IMethodReturnMessage retMsg)
		{
			Exception e = retMsg.Exception;
			if(e != null)
			{
				this.HandleException(e);

				Exception newException = this.GetNewException(e);
				if(!object.ReferenceEquals(e,newException))
					retMsg = new ReturnMessage(newException,callMsg);
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
		{}

		public override void HandleException(Exception e)
		{
			Trace.WriteLine(e.ToString());
		}
	}

	public class ChangeExceptionProcessor : ExceptionHandlingProcessor
	{
		public ChangeExceptionProcessor() : base()
		{}

		public override void HandleException(Exception e)
		{			
		}

		public override Exception GetNewException(Exception oldException)
		{
			return new ApplicationException("Different");
		}


	}

	public class MyNonContextObject //: ContextBoundObject
	{
		public MyNonContextObject() //: base()
		{
		}

		public string DoSomething(string s, int i)
		{
			return s + i.ToString();
		}

		public void ThrowException()
		{
			throw new ApplicationException("Fuckup");
		}
		

	}
	
	[Intercept]
	public class MyContextObject : ContextBoundObject
	{
		public MyContextObject() : base()
		{
		}

		public int MyProperty
		{
			[PreProcess(typeof(TracePreProcessor))]
			get
			{
				return 5;
			}
			[PreProcess(typeof(TracePreProcessor))]
			set
			{
			}
		}

		[PreProcess(typeof(TracePreProcessor))]
		[PostProcess(typeof(TraceExceptionProcessor))]
		public string DoSomething(string s, int i)
		{
			return s + i.ToString();
		}

		[PreProcess(typeof(TracePreProcessor))]
		[PostProcess(typeof(ChangeExceptionProcessor))]
		[PostProcess(typeof(TraceExceptionProcessor))]
		public void ThrowException()
		{
			throw new ApplicationException("An error");
		}
		public override object InitializeLifetimeService()
		{
			return null;
		}

	}

	

	[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property,AllowMultiple=true)]
	public class PreProcessAttribute : Attribute
	{
		private IPreProcessor p;
		public PreProcessAttribute(Type preProcessorType)
		{
			this.p = Activator.CreateInstance(preProcessorType) as IPreProcessor;
			if(this.p == null)
				throw new ArgumentException(String.Format("The type '{0}' does not implement interface IPreProcessor",preProcessorType.Name,"processorType"));
		}

		public IPreProcessor Processor
		{
			get{ return p; }
		}
	}

	[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property,AllowMultiple=true)]
	public class PostProcessAttribute : Attribute
	{
		private IPostProcessor p;
		public PostProcessAttribute(Type postProcessorType)
		{
			this.p = Activator.CreateInstance(postProcessorType) as IPostProcessor;
			if(this.p == null)
				throw new ArgumentException(String.Format("The type '{0}' does not implement interface IPostProcessor",postProcessorType.Name,"processorType"));
		}

		public IPostProcessor Processor
		{
			get{ return p; }
		}
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class InterceptAttribute : ContextAttribute
	{
		
		public InterceptAttribute() : base("Intercept")
		{
		}

		public override void Freeze(Context newContext)
		{			
		}

		public override void GetPropertiesForNewContext(System.Runtime.Remoting.Activation.IConstructionCallMessage ctorMsg)
		{
			ctorMsg.ContextProperties.Add( new InterceptProperty() );
		}

		public override bool IsContextOK(Context ctx, System.Runtime.Remoting.Activation.IConstructionCallMessage ctorMsg)
		{
			InterceptProperty p = ctx.GetProperty("Intercept") as InterceptProperty;
			if(p == null)
				return false;
			return true;
		}

		public override bool IsNewContextOK(Context newCtx)
		{
			InterceptProperty p = newCtx.GetProperty("Intercept") as InterceptProperty;
			if(p == null)
				return false;
			return true;
		}

		
	}

	//IContextProperty, IContributeServerContextSink
	public class InterceptProperty : IContextProperty, IContributeObjectSink
	{
		public InterceptProperty() : base()
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
			if(p == null)
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
			this.PostProcess(msg as IMethodCallMessage,ref mrm);
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
			IMessageCtrl rtnMsgCtrl = nextSink.AsyncProcessMessage(msg,replySink);
			return rtnMsgCtrl;
		}

		#endregion

		private void PreProcess(ref IMethodCallMessage msg)
		{
			PreProcessAttribute[] attrs 
				= (PreProcessAttribute[])msg.MethodBase.GetCustomAttributes(typeof(PreProcessAttribute),true);
			for(int i=0;i<attrs.Length;i++)
				attrs[i].Processor.Process(ref msg);
		}

		private void PostProcess(IMethodCallMessage callMsg, ref IMethodReturnMessage rtnMsg)
		{
			PostProcessAttribute[] attrs 
				= (PostProcessAttribute[])callMsg.MethodBase.GetCustomAttributes(typeof(PostProcessAttribute),true);
			for(int i=0;i<attrs.Length;i++)
				attrs[i].Processor.Process(callMsg,ref rtnMsg);
			
		}

	}



	
}
