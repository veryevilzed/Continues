using System;
using System.Collections;
using System.Collections.Generic;

namespace Async
{
	/// <summary>
	/// Очередь асинхронных событий, срабатывают по требованию
	/// </summary>
	public class AsyncQueue
	{
		private class AsyncQueueItem
		{
			public Action Act { get; set;}
		}

		private class AsyncQueueTimedItem : AsyncQueueItem
		{
			public float SecondsRemain {get;set;}
		}

		private class AsyncQueueIntervalItem : AsyncQueueTimedItem
		{
			public float SecondsDelay {get;set;}
		}


		static AsyncQueue _instance = null;

		/// <summary>
		/// Экземпляр глобальной очереди
		/// </summary>
		/// <value>The instance.</value>
		public static AsyncQueue Instance {
			get { 
				if (_instance == null)
					_instance = new AsyncQueue();
				return _instance; 
			}
		}

		Queue<AsyncQueueItem> actions;

		/// <summary>
		/// Добавить событие
		/// </summary>
		/// <param name="action">Action.</param>
		public void Add(Action action)
		{
			actions.Enqueue(new AsyncQueueItem { Act = action });
		}

		/// <summary>
		/// Добавить отложенное событие
		/// </summary>
		/// <param name="action">Action.</param>
		/// <param name="delay">Время в сек.</param>
		public void Add(Action action, float delay)
		{
			actions.Enqueue(new AsyncQueueTimedItem { Act = action, SecondsRemain = delay });
		}

		/// <summary>
		/// Добавить отлоденное или повторяющееся событие
		/// </summary>
		/// <param name="action">Action.</param>
		/// <param name="delay">Время в сек.</param>
		/// <param name="loop">Если повторять то <c>true</c>.</param>
		public void Add(Action action, float delay, bool loop)
		{
			if (!loop)
				Add(action, delay);
			else
				actions.Enqueue(new AsyncQueueIntervalItem { Act = action, SecondsRemain = delay, SecondsDelay = delay });
		}
			
		/// <summary>
		/// Метод для вызова исполнителя 
		/// </summary>
		/// <param name="deltaTime">Для Unity3d Time.deltaTime</param>
		/// <param name="count">Количество задач</param>
		public void Execute(float deltaTime, int count)
		{
			for (int i = 0; i < count; i++) {
				if (actions.Count == 0)
					break;
				AsyncQueueItem item = actions.Dequeue();
				if (item.GetType() == typeof(AsyncQueueIntervalItem) || item.GetType() == typeof(AsyncQueueTimedItem)) {
					((AsyncQueueTimedItem)item).SecondsRemain -= deltaTime;
					if (((AsyncQueueTimedItem)item).SecondsRemain > 0)
						actions.Enqueue(item);
					else {
						item.Act.Invoke();
						if (item.GetType() == typeof(AsyncQueueIntervalItem)) {
							AsyncQueueIntervalItem _item = (AsyncQueueIntervalItem)item;
							_item.SecondsRemain += _item.SecondsDelay;
							actions.Enqueue(_item);
						}
					}
				} else {
					item.Act.Invoke();
				}
			}
		}

		/// <summary>
		/// Метод для вызова исполнителя 
		/// </summary>
		/// <param name="deltaTime">Для Unity3d Time.deltaTime</param>
		public void Execute(float deltaTime)
		{
			Execute(deltaTime, this.Count);
		}

		public int Count { get { return actions.Count; } }

		public AsyncQueue()
		{
			actions = new Queue<AsyncQueueItem>();
		}

	}
}

