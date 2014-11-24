using System;
using System.Collections.Generic;

namespace Async
{
	/// <summary>
	/// Состояния задачи
    /// </summary>
	public enum Statuses
	{
		/// <summary>
		/// Завершить задачу успешно
		/// </summary>
		OK,

		/// <summary>
		/// Завершить задачу с ошибкой
		/// </summary>
		Error,

		/// <summary>
		/// Продолжить исполнение задачи
		/// </summary>
		Continue,
        
		/// <summary>
		/// Ожидание 
		/// </summary>
		Wait,

		/// <summary>
		/// Исполнить немедленно
		/// </summary>
		Immediately
	}

	public delegate ContinuesPath DContinuePathCreation(ContinuesPath path);
	public delegate Statuses DContinueActionWithPath(ContinuesPath path);
	public delegate Statuses DContinueAction();

	public delegate void DContinuesPathEvent(ContinuesPath path);

	/// <summary>
	/// Путь задач
	/// </summary>
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

		/// <summary>
		/// Количество задач в очереди
		/// </summary>
		/// <value>The count.</value>
		public int Count { get { return actions.Count; } }

		/// <summary>
		/// Добавить действие в конец пути
		/// </summary>
		/// <param name="action">Action.</param>
		public ContinuesPath Add(DContinueActionWithPath action)
		{
			if (action!=null)
				this.actions.Add(action);
			return this;
		}

		/// <summary>
		/// Добавить действие в конец пути
		/// </summary>
		/// <param name="action">Action.</param>
		public ContinuesPath Add(DContinueAction action)
		{
			if (action!=null)
				this.actions.Add(action);
			return this;
		}

		/// <summary>
		/// Добавить действия в конец пути
		/// </summary>
		/// <param name="actions">Actions.</param>
		public ContinuesPath Add(DContinueAction[] actions)
		{
			if (actions!=null)
				foreach(DContinueAction a in actions)
					this.actions.Add(a);
			return this;
		}

		/// <summary>
		/// Добавить действия в путь
		/// </summary>
		/// <param name="actions">Actions.</param>
		public ContinuesPath Add(DContinueActionWithPath[] actions)
		{
			if (actions!=null)
				foreach(DContinueActionWithPath a in actions)
					this.actions.Add(a);
			return this;
		}


		/// <summary>
		/// Создает ветку в текущем пути, путь ждет завершения ветки
		/// </summary>
		/// <param name="actions">Actions.</param>
		public ContinuesPath Branch(DContinueAction[] actions){
			ContinuesPath path = new ContinuesPath();
			foreach(DContinueAction action in actions)
				path.Add(action);
			this.Add(() => {
				path.Update();
				return path.Count > 0 ? Statuses.Continue : Statuses.OK;
			});
			return this;
		}

		/// <summary>
		/// Создает ветку в текущем пути, путь ждет завершения ветки
		/// </summary>
		/// <param name="path">Path.</param>
		public ContinuesPath Branch(DContinuePathCreation path)
		{
			ContinuesPath _path = path.Invoke(new ContinuesPath());
			this.Add(() => {
				_path.Update();
				return _path.Count > 0 ? Statuses.Continue : Statuses.OK;
			});
			return this;
		}

		/// <summary>
		/// Добавить задачу в начало очереди
		/// </summary>
		/// <param name="action">Action.</param>
		public ContinuesPath Insert(DContinueAction action)
		{
			this.actions.Insert(0,action);
			return this;
		}


		/// <summary>
		/// Добавить задачу в начало очереди
		/// </summary>
		/// <param name="action">Action.</param>
		public ContinuesPath Insert(DContinueActionWithPath action)
		{
			this.actions.Insert(0,action);
			return this;
		}


		/// <summary>
		/// Добавить задачу в начало очереди
		/// </summary>
		/// <param name="action">Action.</param>
		public ContinuesPath Insert(DContinueAction action, int position)
		{
			this.actions.Insert(position,action);
			return this;
		}


		/// <summary>
		/// Добавить задачу в начало очереди
		/// </summary>
		/// <param name="action">Action.</param>
		public ContinuesPath Insert(DContinueActionWithPath action, int position)
		{
			this.actions.Insert(position,action);
			return this;
		}


		/// <summary>
		/// Создает ожидание на основе NamedAsyncQueue
		/// </summary>
		/// <param name="seconds">Seconds.</param>
		public ContinuesPath Wait(float seconds){
			return this.Wait(null, seconds, null);
		}

		/// <summary>
		/// Действие, Ожидание
		/// </summary>
		/// <param name="beforeAction">Before action.</param>
		/// <param name="seconds">Seconds.</param>
		public ContinuesPath Wait(Action beforeAction, float seconds){
			return this.Wait(beforeAction, seconds, null);
		}

		/// <summary>
		/// Создает ожидание на основе NamedAsyncQueue
		/// </summary>
		/// <param name="beforeAction">Before Action</param>
		/// <param name="seconds">Seconds Wait After.</param>
		/// <param name="afterAction">After Action.</param>
		public ContinuesPath Wait(Action beforeAction, float seconds, Action afterAction){

			string guid = System.Guid.NewGuid().ToString();

			this.Branch((ContinuesPath _path) => {

				if (beforeAction != null)
					_path.Add(() => {
						beforeAction.Invoke();
						return Statuses.OK;
					});

				_path.Add(() => {
					NamedAsyncQueue.Instance.AddWaitLock(guid, seconds);
					return Statuses.Immediately;
				});

				_path.Add(() => {
					if (NamedAsyncQueue.Instance.Exist(guid))
						return Statuses.Continue;
					return Statuses.OK;
				});

				if (afterAction != null)
					_path.Add(() => {
						afterAction.Invoke();
						return Statuses.OK;
					});

				return _path;
			});
			return this;
		}

		public static ContinuesPath Create()
		{
			return new ContinuesPath();
		}

		/// <summary>
		/// Бросить эту задачу и начать следующую
		/// </summary>
		public ContinuesPath Next()
		{
			if (this.actions.Count > 0) {
				this.actions.RemoveAt(0);
				if (this.OnNext != null && actions.Count > 0)
					this.OnNext(this);
			}
			return this;
		}

		/// <summary>
		/// Остановить очередь задач
		/// </summary>
		public ContinuesPath Stop()
		{
			if (this.actions.Count > 0) {
				this.actions.Clear();

				if (this.actions.Count == 0 && this.OnFinish != null)
					this.OnFinish(this);
			}
			return this;
		}

		/// <summary>
		/// Обновления состояния задачи
		/// </summary>
		public void Update()
		{
			if (actions.Count > 0) {

				Delegate d = actions[0];

				Statuses st = Statuses.OK;
				if (d.GetType() == typeof(DContinueActionWithPath))
					st = ((DContinueActionWithPath)actions[0]).Invoke(this);
				else
					st = ((DContinueAction)actions[0]).Invoke();
				bool error = false;
				switch (st) {
					case Statuses.OK:
					if (actions.Count > 0)
						actions.RemoveAt(0);
					if (this.OnNext != null && actions.Count > 0)
							this.OnNext(this);
						break;
					case Statuses.Immediately:
						if (actions.Count > 0)
							actions.RemoveAt(0);
						if (this.OnNext != null && actions.Count > 0)
							this.OnNext(this);
						if (actions.Count > 0)
							Update();
						break;

					case Statuses.Error:
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

