using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting.Messaging;
using System.Reflection;

namespace AspectProxy
{
    public class NotificationProcessor : IProcessor
    {
        protected IMethodCallMessage msghandle;
        public NotificationProcessor()
        {

        }
        public Dictionary<string, string> Settings = new Dictionary<string, string>();
        public void UpdateSetting(string key, string value)
        {
            if (Settings.ContainsKey(key)) Settings.Remove(key);
            Settings.Add(key, value);
            if (msghandle.Properties.Contains("_settings")) msghandle.Properties.Remove("_settings");
            msghandle.Properties.Add("_settings", Settings);
        }

        public void PreProcess(ref IMethodCallMessage msg, MarshalByRefObject target)
        {
            msghandle = msg;
            object list = CallContext.GetData(this.GetType().FullName);
            if (list != null)
            {
                List<NotificationProcessor> notificationlist = (List<NotificationProcessor>)list;
                if (notificationlist != null)
                {
                    notificationlist.Add(this);
                    CallContext.SetData(this.GetType().FullName, notificationlist);
                }
                else
                {
                    CallContext.SetData(this.GetType().FullName, new List<NotificationProcessor>() { this });
                }
            }
            else
            {
                CallContext.SetData(this.GetType().FullName,new List<NotificationProcessor>(){ this});
            }
        }

        public void PostProcess(IMethodCallMessage callMsg, ref IMethodReturnMessage retMsg, MarshalByRefObject target)
        {
            Type type = target.GetType();
            MethodInfo mi = type.GetMethod("UpdateSetting");
            if (mi == null) return;
            Settings = (Dictionary<string, string>)callMsg.Properties["_settings"];
            if (Settings != null)
            {
                foreach (KeyValuePair<string, string> setting in Settings)
                {
                    mi.Invoke(target, new object[] { setting.Key, setting.Value });
                }
                
            }
           
           
            
        }
    }
   
 
    
}
