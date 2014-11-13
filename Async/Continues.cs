using System;
using System.Collections.Generic;

namespace Async
{

	public enum ContinuesStatuses
	{
		OK,
		Error,
		Continue
	}

	public delegate ContinuesStatuses DContinueActionWithPath(ContinuesPath path);
	public delegate ContinuesStatuses DContinueAction();

	public delegate void DContinuesPathEvent(ContinuesPath path);

	public class ContinuesPath
	{
		List<Delegate> actions;

		public event DContinuesPathEvent OnNext;
		public event DContinuesPathEvent OnSuccess;
		public event DContinuesPathEvent OnFinish;
		public event DContinuesPathEvent OnError;

		public ContinuesPath()
		{
			actions = new List<Delegate>();
		}

		public int Count { get { return actions.Count; } }

		public ContinuesPath Add(DContinueActionWithPath action)
		{
			this.actions.Add(action);
			return this;
		}

		public ContinuesPath Add(DContinueAction action)
		{
			this.actions.Add(action);
			return this;
		}

		public ContinuesPath Insert(DContinueAction action)
		{
			this.actions.Insert(0,action);
			return this;
		}

		public ContinuesPath Insert(DContinueActionWithPath action)
		{
			this.actions.Insert(0,action);
			return this;
		}

		public void Next()
		{
			if (this.actions.Count > 0) {
				this.actions.RemoveAt(0);
				if (this.OnNext != null && actions.Count > 0)
					this.OnNext(this);
			}
		}

		public void Stop()
		{
			if (this.actions.Count > 0) {
				this.actions.Clear();

				if (this.actions.Count == 0 && this.OnFinish != null)
					this.OnFinish(this);
			}
		}

		public void Update()
		{
			if (actions.Count > 0) {

				Delegate d = actions[0];

				ContinuesStatuses st = ContinuesStatuses.OK;
				if (d.GetType() == typeof(DContinueActionWithPath))
					st = ((DContinueActionWithPath)actions[0]).Invoke(this);
				else
					st = ((DContinueAction)actions[0]).Invoke();
				bool error = false;
				switch (st) {
					case ContinuesStatuses.OK:
						actions.RemoveAt(0);
						if (this.OnNext != null && actions.Count>0)
							this.OnNext(this);
						break;
					case ContinuesStatuses.Error:
						actions.Clear();
						error = true;
						if (this.OnError != null)
							this.OnError(this);
						break;
					default:
						break;					
				}

				if (this.actions.Count == 0 && this.OnFinish != null)
					this.OnFinish(this);
				if (this.actions.Count == 0 && this.OnSuccess != null && !error)
					this.OnSuccess(this);

			}
		}

	}



}

