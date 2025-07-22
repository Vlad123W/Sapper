using saper1.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace saper1.Services
{
    public class GameTimer : IGameTimer
    {
        private readonly DispatcherTimer _timer;
        private int _seconds;
        private int _minutes;

        public event Action<int, int>? TimeChanged;

        public GameTimer()
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += (s, e) =>
            {
                _seconds++;
                if (_seconds == 60)
                {
                    _minutes++;
                    _seconds = 0;
                }
                TimeChanged?.Invoke(_minutes, _seconds);
            };
        }

        public void Start() => _timer.Start();
        public void Stop() => _timer.Stop();
        public void Reset()
        {
            _minutes = 0;
            _seconds = 0;
            TimeChanged?.Invoke(_minutes, _seconds);
        }
    }
}
