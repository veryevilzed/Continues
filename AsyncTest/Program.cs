using System;
using System.Threading;
using Async;


namespace AsyncTest
{


	class MainClass
	{
		static Async.NamedAsyncQueue nq = new Async.NamedAsyncQueue();
		static Thread th;

		static bool run = true;

		public static void Run()
		{
			while (run) {
				nq.Update(0.1f);
				Thread.Sleep(100);
			}
		}



		public static void Main(string[] args)
		{


			th = new Thread(new ThreadStart(Run));
			th.Start();

			nq.AddPeriodic("", (Async.NamedActionState state) => {
				Async.NamedActionPeriodicState _state = (NamedActionPeriodicState)state;
				Console.WriteLine("- {0} {1} {2}",state.IntegerCounter.ToString(), _state.PeriodicDelay, _state.PeriodicCounter);
				return Async.Statuses.Continue;
			}, 2.1f, 5);

			nq.AddPeriodic("", (Async.NamedActionState state) => {
				Async.NamedActionPeriodicState _state = (NamedActionPeriodicState)state;
				Console.WriteLine("--- {0} {1} {2}",state.IntegerCounter.ToString(), _state.PeriodicDelay, _state.PeriodicCounter);
				return Async.Statuses.Continue;
			}, 1.3f, 10);



			Console.ReadKey();

			th.Abort();



		}
	}
}
