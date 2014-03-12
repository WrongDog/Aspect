using System;
using System.Collections;
using System.Collections.Generic;
namespace AspectIntercept
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class Class1
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			try
			{
				System.Diagnostics.Trace.Listeners.Add( new ConsoleTraceListener() );
                MyContextObject o = new MyContextDerivative();
                string refv = "something";
                int i=0;

                Console.WriteLine("Setting count:{0}", o.Settings.Count);
                foreach (KeyValuePair<string, string> setting in o.Settings)
                {
                    Console.WriteLine(String.Format(" Setting key {0},value {1}", setting.Key, setting.Value));
                }

				Console.WriteLine(o.DoSomething("str",10,ref refv,out i));

                Console.WriteLine("Setting count:{0}", o.Settings.Count);
                foreach (KeyValuePair<string, string> setting in o.Settings)
                {
                    Console.WriteLine(String.Format(" Setting key {0},value {1}",setting.Key,setting.Value));
                }
              
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
			}
			Console.ReadLine();
		}
	}
}
