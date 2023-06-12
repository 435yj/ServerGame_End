using System;
using System.Collections.Generic;
using System.Text;
using ServerCore;

namespace Server.Game
{
	struct JobTimerElem : IComparable<JobTimerElem>
	{
		public int execTick; // 실행 시간
		public IJob job;

		public int CompareTo(JobTimerElem other)
		{
			return other.execTick - execTick;
		}
	}

	public class JobTimer
	{
		PriorityQueue<JobTimerElem> _pq = new PriorityQueue<JobTimerElem>();
		object _lock = new object();

		// action으로 실행 안하도록 바꿈
		public void Push(IJob job, int tickAfter = 0)
		{
			JobTimerElem jobElement;
			jobElement.execTick = System.Environment.TickCount + tickAfter;
			jobElement.job = job;

			lock (_lock)
			{
				_pq.Push(jobElement);
			}
		}

		public void Flush()
		{
			while (true)
			{
				int now = System.Environment.TickCount;

				JobTimerElem jobElement;

				lock (_lock)
				{
					if (_pq.Count == 0)
						break;

					jobElement = _pq.Peek();
					if (jobElement.execTick > now)
						break;

					_pq.Pop();
				}

				// invoke가 아니게 만듬
				jobElement.job.Execute();
			}
		}
	}
}
