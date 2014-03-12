using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting.Messaging;
using AspectProxy;
namespace AspectIntercept
{
    class ClassNormal
    {
        public static void Something()
        {
            Object obj = CallContext.GetData(typeof(NotificationProcessor).FullName);
            if (obj != null)
            {
                NotificationProcessor aspect = (NotificationProcessor)obj;
                aspect.UpdateSetting(string.Format("Inside Static MethodCall "), "");
            }
        }
    }
}
