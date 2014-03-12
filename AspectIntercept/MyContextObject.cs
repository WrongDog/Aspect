using System;
using System.Collections.Generic;
using System.Text;
using AspectProxy;

namespace AspectIntercept
{
    //[Process(typeof(TraceExceptionProcessor))]
    //[Process(typeof(TraceProcessor))]
     [AspectProxy]
    public class MyContextObject : ContextBoundObject
    {
        public Dictionary<string, string> Settings = new Dictionary<string, string>();
        public void UpdateSetting(string key, string value)
        {
            if (Settings.ContainsKey(key)) Settings.Remove(key);
            Settings.Add(key, value);
        }
        public MyContextObject()
            : base()
        {
        }

        public int MyProperty
        {
            
            get
            {
                return 5;
            }
            set
            {
            }
        }
        [Process(typeof(NotificationProcessor))]
        [Process(typeof(CodeTimerProcessor))]
        //[PostProcess(typeof(TraceExceptionProcessor))]
        public virtual string DoSomething(string s, int i,ref string refv,out int outv)
        {
            refv = refv + i.ToString();
            outv = new MyContextObject2().GetValue();
            return new MyContextObject2().GetSetting(refv, outv);
        }
       
        [Process(typeof(ChangeExceptionProcessor))]
        [Process(typeof(TraceExceptionProcessor))]
        public void ThrowException()
        {
            throw new ApplicationException("An error");
        }
        public override object InitializeLifetimeService()
        {
            return null;
        }

    }
}
