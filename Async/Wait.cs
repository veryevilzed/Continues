using System;

namespace Async
{
	public class Wait
	{
		public static string CreateWait(float time)
		{
			return CreateWait(time, System.Guid.NewGuid().ToString());
		}

		public static string CreateWait(float time, string name)
		{

			return name;
		}


	}
}

