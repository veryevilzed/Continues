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
				if (path != null && path.Count > 0) {
					path.Update();
					Console.WriteLine("       cnt:{0} {1}", path.Count, GC.GetTotalMemory(false) );
				}
				Thread.Sleep(100);
			}
		}

		public static ContinuesPath CreatePath()
		{
			ContinuesPath __path = ContinuesPath.Create();
			__path
				.Add(() => {
				Console.WriteLine(">");
					return Statuses.OK;
			})

				.Add(() => {
				Console.WriteLine(">");
				return Statuses.Immediately;
			})


				.Wait(() => { 
				Console.WriteLine("Hello");
			}, 1.0f, () => {
				Console.WriteLine("World");
			})

				.Add(() => {
				Console.WriteLine("!");
					return Statuses.Error;
			})

				.Branch((_path) => {
				_path.Wait(
					() => {
						Console.WriteLine("Play Sound");
					}, 
					1.0f, 
					() => {
						Console.WriteLine("Stop Sound");
					}
				);
				return _path;
			})
				.Add(() => {
				path.CopyActions(CreatePath());
				return Statuses.OK;
			}).Error(() => {
				Console.WriteLine("EROR!");
				return Statuses.OK;
			}).Error(() => {
					Console.WriteLine("EROR! ++ ");
					return Statuses.Error;
			}).Error(() => {
					Console.WriteLine("EROR! ++++++");
					return Statuses.OK;
			});
			return __path;
		}

		public static void Main(string[] args)
		{

			th = new Thread(new ThreadStart(Run));

			path = new ContinuesPath();

			path.Branch(CreatePath());

			th.Start();

			Console.ReadKey();

			th.Abort();

		}
	}
}
