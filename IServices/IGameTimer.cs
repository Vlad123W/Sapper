using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace saper1.IServices
{
    public interface IGameTimer
    {
        event Action<int, int> TimeChanged;
        void Start();
        void Stop();
        void Reset();
    }
}
