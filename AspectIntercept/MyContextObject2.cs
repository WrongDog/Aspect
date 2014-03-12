using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting.Messaging;
using AspectProxy;
namespace AspectIntercept
{
    [Process(typeof(TraceExceptionProcessor))]
    [Process(typeof(NotificationProcessor))]
    [AspectProxy]
    public class MyContextObject2 : ContextBoundObject
    {
        public MyContextObject2()
            : base()
        {
        }
        public Dictionary<string, string> Settings = new Dictionary<string, string>();
        public void UpdateSetting(string key, string value)
        {
            if (Settings.ContainsKey(key)) Settings.Remove(key);
            Settings.Add(key, value);
        }
        public int GetValue()
        {
            return 4;
        }
        public string GetSetting(string s, int i) {
    
            string value = s + i.ToString();
            FireSetting(string.Format("Setting index {0}-{1}",s,i),value);

            return value; 
        }
        private void FireSetting(string key, string value)
        {
            Object obj = CallContext.GetData(typeof(NotificationProcessor).FullName);
            if (obj != null)
            {

                foreach (NotificationProcessor aspect in (List<NotificationProcessor>)obj)
                {
                    aspect.UpdateSetting(key, value);
                }
            }
        }
    }
}
