using System;
using System.Collections;
using System.Collections.Generic;

namespace Async
{
	public delegate Statuses DNamedAction(NamedActionState state);

	public class NamedActionState
	{

		public enum NamedActionTypes
		{
			Periodic,
			Permanently
		}

		public NamedActionTypes Type { get; set; }

		public DNamedAction     Action { get; set; }
		public float            FloatCounter {get; set; }  // Float Counter
		public int              IntegerCounter { get; set; } // int counter
		public float            Delay { get; set; } // int counter
		public Statuses         Status { get; set; }

		public Dictionary<string, object> Args { get; protected set; }

		public virtual Statuses Update(float delta)
		{
			if (Delay > 0 && Status == Statuses.Wait) {
				Delay -= delta;
			}
				

			if (Delay <= 0 && Status == Statuses.Wait)
				Status = Statuses.Continue;

			if (this.Status == Statuses.Continue) {
				this.FloatCounter += delta;
				this.IntegerCounter++;
				this.Status = this.Action.Invoke(this);
			}
			return this.Status;
		}

		public void Set(string key, object value)
		{
			if (this.Args.ContainsKey(key))
				this.Args[key] = value;
			else
				this.Args.Add(key, value);
		}

		public object Get(string key)
		{
			return this.Args[key];
		}

		public object Get(string key, object def)
		{
			if (!this.Args.ContainsKey(key))
				return def;
			return this.Args[key];
		}


		public NamedActionState OK()
		{
			this.Status = Statuses.OK;
			return this;
		}

		public NamedActionState Continue()
		{
			this.Status = Statuses.Continue;
			return this;
		}

		public NamedActionState Error()
		{
			this.Status = Statuses.Error;
			return this;
		}

		public NamedActionState Error(string message)
		{
			this.Status = Statuses.Error;
			this.Args.Add("ERR", message);
			return this;
		}

		public NamedActionState(DNamedAction action, Dictionary<string, object> args) : this(action)
		{
			if (args != null)
				this.Args = args;
		}

		public NamedActionState(DNamedAction action)
		{
			Status = Statuses.Wait;
			FloatCounter = 0;
			Action = action;
			IntegerCounter = 0;
			Args = new Dictionary<string, object>();
		}

	}

	public class NamedActionPeriodicState : NamedActionState
	{
		public int   PeriodicCounter {get;set;}
		public float PeriodicDelay   {get;set;}

		public override Statuses Update(float delta)
		{

			Statuses st = base.Update(delta);
			if (st != Statuses.Continue)
				return st;


			if (this.PeriodicCounter != -1) {
				this.PeriodicCounter -= 1;
				if (this.PeriodicCounter > 0)
					st = Statuses.Wait;
				else
					st = Statuses.OK;
				
			} else
				st = Statuses.Wait;
			this.Delay = PeriodicDelay;
			this.Status = st;

			return st;
		}

		public NamedActionPeriodicState(DNamedAction action, float delay, int count) : base(action)
		{
			this.PeriodicDelay = delay;
			this.PeriodicCounter = count;
			this.Delay = delay;
			this.Status = Statuses.Wait;
		}

		public NamedActionPeriodicState(DNamedAction action, float delay, int count, Dictionary<string, object> args) : base(action, args)
		{
			this.PeriodicDelay = delay;
			this.PeriodicCounter = count;
			this.Delay = delay;
			this.Status = Statuses.Wait;
		}
	}


	public class NamedAsyncQueue 
	{

		Dictionary<string, NamedActionState> queue;

		public void Add(string name, NamedActionState state)
		{
			if (name == "")
				name = this.UUID();
			if (!this.queue.ContainsKey(name))
				this.queue.Add(name, state);
		}

		public bool Exist(string name)
		{
			return this.queue.ContainsKey(name);
		}

		public void Stop(string name)
		{
			this.queue.Remove(name);
		}

		public NamedAsyncQueue Add(string name, DNamedAction action)
		{
			this.queue.Add(name, new NamedActionState(action));
			return this;
		}

		public NamedAsyncQueue Add(string name, DNamedAction action, Dictionary<string, object> args)
		{
			this.Add(name, new NamedActionState(action, args));
			return this;
		}

		public NamedAsyncQueue AddPeriodic(string name, DNamedAction action, float delay, int count)
		{
			this.Add(name, new NamedActionPeriodicState(action, delay, count));
			return this;
		}

		public NamedAsyncQueue AddPeriodic(string name, DNamedAction action, float delay, int count, Dictionary<string, object> args)
		{
			this.Add(name, new NamedActionPeriodicState(action, delay, count, args));
			return this;
		}

		public void Clear()
		{
			this.queue.Clear();
		}

		public int Count()
		{
			return this.queue.Keys.Count;
		}

		private static NamedAsyncQueue _instance = null;
		public static NamedAsyncQueue Instance 
		{
			get { 
				if (_instance == null)
					_instance = new NamedAsyncQueue();
				return _instance; 
			}
		}

		public void Update(float delta)
		{
			List<string> removeItems = new List<string>();
			foreach (KeyValuePair<string, NamedActionState> kv in this.queue) {
				Statuses st = kv.Value.Update(delta);
				switch (st) {
					case Statuses.OK:
						removeItems.Add(kv.Key);
						break;
					case Statuses.Error:
						removeItems.Add(kv.Key);
						break;
					default:
						break;
				}
			}

			foreach (string key in removeItems) {
				this.queue.Remove(key);
			}
		}

		public string UUID()
		{
			return System.Guid.NewGuid().ToString();
		}

		public NamedAsyncQueue()
		{
			queue = new Dictionary<string, NamedActionState>();
		}
	}
}

