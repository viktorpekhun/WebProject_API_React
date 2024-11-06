using System.Collections.Concurrent;

namespace WebProject_API_React.Server.Services
{
    public class TaskCancellationService
    {
        private readonly Dictionary<int, CancellationTokenSource> _taskCancellationTokens = new();

        // Створення токена скасування для конкретного завдання
        public CancellationToken CreateCancellationToken(int taskId)
        {
            var cts = new CancellationTokenSource();
            _taskCancellationTokens[taskId] = cts;
            return cts.Token;
        }

        //// Отримання токена скасування для конкретного завдання
        public CancellationToken GetCancellationToken(int taskId)
        {
            return _taskCancellationTokens.ContainsKey(taskId) ? _taskCancellationTokens[taskId].Token : CancellationToken.None;
        }

        public void TokenDispose(int taskId)
        {
            if (_taskCancellationTokens.ContainsKey(taskId))
            {
                _taskCancellationTokens[taskId].Dispose();
                _taskCancellationTokens.Remove(taskId);
            }
        }

        // Скасування конкретного завдання
        public bool CancelTask(int taskId)
        {
            if (_taskCancellationTokens.ContainsKey(taskId))
            {
                _taskCancellationTokens[taskId].Cancel();
                return true;
            }
            return false;
        }
    }
}
