using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

namespace Async {

	/// <summary>
	/// Класс параллельных заданий
	/// </summary>
	public class Parallel
	{
		Queue<Action> queue;

		public void Add(Action action){

		}

		public Parallel()
		{
			queue = new Queue<Action>();
		}
	}
}

