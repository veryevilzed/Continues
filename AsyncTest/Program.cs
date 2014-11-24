using System;
using System.Threading;
using Async;


namespace AsyncTest
{


	class MainClass
	{
		static Thread th;

		static bool run = true;
		static ContinuesPath path;


		public static void Run()
		{
			while (run) {
				NamedAsyncQueue.Instance.Update(0.1f);
				if (path != null)
					path.Update();
				Thread.Sleep(100);
			}
		}



		public static void Main(string[] args)
		{

			th = new Thread(new ThreadStart(Run));


			path = ContinuesPath.Create();

			path.Add(() => {
				Console.WriteLine(">");
				return Statuses.OK;
			});


			path.Wait(() => { 
				Console.WriteLine("Hello");
			}, 2.0f, () => {
				Console.WriteLine("World");
			});

			path.Add(() => {
				Console.WriteLine("!");
				return Statuses.OK;
			});

			th.Start();

			Console.ReadKey();

			th.Abort();



		}
	}
}
