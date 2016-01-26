using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrics.Reporting.Graphite
{
    public interface GraphiteSender : IDisposable
    {
        void Connect();

        void Send(string name, string value, long timestamp);

        void Flush();
        bool IsConnected { get; }
        int FailureCount { get; }

    }
}
