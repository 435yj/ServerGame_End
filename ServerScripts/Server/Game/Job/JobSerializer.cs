using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
	public class JobSerializer
	{
        // jobtimer를 instance없이 사용하기 위함이다
		JobTimer _timer = new JobTimer();

        // 미래의 일 관리
		Queue<IJob> _jobQueue = new Queue<IJob>();
		object _lock = new object();
		bool _flush = false;

        // 예약 사용, 똑같이 헬퍼들을 만들어 준다.
		public void PushAfter(int tickAfter, Action action) { PushAfter(tickAfter, new Job(action)); }
		public void PushAfter<T1>(int tickAfter, Action<T1> action, T1 t1) { PushAfter(tickAfter, new Job<T1>(action, t1)); }
		public void PushAfter<T1, T2>(int tickAfter, Action<T1, T2> action, T1 t1, T2 t2) { PushAfter(tickAfter, new Job<T1, T2>(action, t1, t2)); }
		public void PushAfter<T1, T2, T3>(int tickAfter, Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3) { PushAfter(tickAfter, new Job<T1, T2, T3>(action, t1, t2, t3)); }

		public void PushAfter(int tickAfter, IJob job)
		{
			_timer.Push(job, tickAfter);
		}

		public void Push(Action action) { Push(new Job(action)); }
		public void Push<T1>(Action<T1> action, T1 t1) { Push(new Job<T1>(action, t1)); }
		public void Push<T1, T2>(Action<T1, T2> action, T1 t1, T2 t2) { Push(new Job<T1, T2>(action, t1, t2)); }
		public void Push<T1, T2, T3>(Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3) { Push(new Job<T1, T2, T3>(action, t1, t2, t3)); }

        public void Push(IJob job)
        {
            bool flush = false;

            // 락을 풀고 실행
            lock (_lock)
            {
                _jobQueue.Enqueue(job);
                if (_flush == false)
                    flush = _flush = true;
            }

            // 첫 번째 들어온 flush를 실행함
            if (flush)
                Flush(); 
        }

        public void Flush()
        {
            _timer.Flush();

            while (true) 
            {
                IJob job = Pop();
                if (job == null)
                    return;

                // Action.Invoke임
                job.Execute();
            }
        }

         public IJob Pop()
        {
            lock (_lock)
            {
                if (_jobQueue.Count == 0)
                {
                    _flush = false;
                    return null;
                }
                return _jobQueue.Dequeue();
            }
        }
    }
}
