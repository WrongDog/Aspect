using System;

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
				MyContextObject o = new MyContextObject();
				Console.WriteLine(o.DoSomething("str",10));
				o.ThrowException();
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
			}
			Console.ReadLine();
		}
	}
}
