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

	public delegate void DContinuesPathEvent(ContinuesPath path);
	public delegate ContinuesPath DContinuePathCreation(ContinuesPath path);
	public delegate Statuses DContinueActionWithPath(ContinuesPath path);
	public delegate Statuses DContinueAction();

	/// <summary>
	/// Путь задач
	/// </summary>
	public class ContinuesPath
	{
		private List<Delegate> actions;



		public event DContinuesPathEvent OnNext;
		public event DContinuesPathEvent OnSuccess;
		public event DContinuesPathEvent OnFinish;
		public event DContinuesPathEvent OnError;


		private static ContinuesPath instance = null;
		public static ContinuesPath Instance { 
			get { 
				if (instance == null)
					instance = ContinuesPath.Create();
				return instance;
			}
		}

		public ContinuesPath()
		{
			actions = new List<Delegate>();
		}

		/// <summary>
		/// Количество задач в очереди
		/// </summary>
		/// <value>The count.</value>
		public int Count { get { return actions.Count; } }


		public ContinuesPath AddAction(Action action){
			if (action != null)
				this.Add(() => {
					action.Invoke();
					return Statuses.Immediately;
				});
			return this;
		}

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
				return path.Count > 0 ? Statuses.Continue : Statuses.Immediately;
			});
			return this;
		}

		/// <summary>
		/// Создает ветку в текущем пути, путь ждет завершения ветки
		/// </summary>
		/// <param name="path">Path.</param>
		public ContinuesPath Branch(DContinuePathCreation pathCreation)
		{
			ContinuesPath path = pathCreation.Invoke(new ContinuesPath());
			return this.Branch(path);
		}

		/// <summary>
		/// Добавить Path
		/// </summary>
		/// <param name="path">Path.</param>
		public ContinuesPath Branch(ContinuesPath path)
		{
			this.Add(() => {
				path.Update();
				return path.Count > 0 ? Statuses.Continue : Statuses.Immediately;
			});
			return this;
		}

		public ContinuesPath CopyActions(ContinuesPath other)
		{
			foreach (Delegate d in other.actions)
				this.actions.Add(d);
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

		public ContinuesPath OK()
		{
			this.Add(() => {
				return Statuses.OK;
			});
			return this;
		}

		public ContinuesPath Wait()
		{
			this.Add(() => {
				return Statuses.OK;
			});
			return this;
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
		/// Ожидание, Действие
		/// </summary>
		/// <param name="seconds">Seconds.</param>
		/// <param name="afterAction">After action.</param>
		public ContinuesPath Wait(float seconds, Action afterAction){
			return this.Wait(null, seconds, afterAction);
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
				_path.AddAction(beforeAction);

				_path.Add(() => {
					NamedAsyncQueue.Instance.AddWaitLock(guid, seconds);
					return Statuses.Immediately;
				});

				_path.Add(() => {
					return NamedAsyncQueue.Instance.Exist(guid) ? Statuses.Continue : Statuses.Immediately;
				});

				_path.AddAction(afterAction);

				return _path;
			});
			return this;
		}

		public static ContinuesPath Create()
		{
			return new ContinuesPath();
		}

		public static ContinuesPath Create(DContinueAction action){
			ContinuesPath path = new ContinuesPath();
			path.Add(action);
			return path;
		}

		public static ContinuesPath Create(DContinueAction[] actions){
			ContinuesPath path = new ContinuesPath();
			path.Add(actions);
			return path;
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
		/// Clear this path.
		/// </summary>
		public ContinuesPath Clear(){
			return this.Stop();

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

