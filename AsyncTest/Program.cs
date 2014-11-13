using System;
using System.Threading;


namespace AsyncTest
{


	class MainClass
	{
		static Async.ContinuesPath cp = new Async.ContinuesPath();
		static Thread th;
		static int i = 0;
		public static void Run()
		{
			while (cp.Count > 0) {
				cp.Update();
				Thread.Sleep(500);
			}

		}

		public static Async.ContinuesStatuses Tick(Async.ContinuesPath p)
		{
			Console.WriteLine("Hello {0}", i);
			i++;
			if (i < 5)
				return Async.ContinuesStatuses.Continue;
			else {
				return Async.ContinuesStatuses.OK;
			}
		}

		public static Async.ContinuesStatuses Finish(Async.ContinuesPath p)
		{
			Console.WriteLine("Finish");
			p.Add(Tick).Add(Finish);
			i = 0;
			return Async.ContinuesStatuses.OK;
		}


		public static void Main(string[] args)
		{

			cp.Add(Tick).Add(Finish);

			th = new Thread(new ThreadStart(Run));
			th.Start();

			Console.ReadKey();
			cp.Next();
			Console.ReadKey();
			cp.Next();
			Console.ReadKey();
			cp.Stop();

			Console.WriteLine("I = {0}", i);


		}
	}
}
