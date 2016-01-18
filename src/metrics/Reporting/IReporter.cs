﻿using System;

namespace Metrics.Reporting
{
    interface IReporter : IDisposable
    {
        void Run();
        void Start(long period, TimeUnit unit);
        void Stop();

        event EventHandler<EventArgs> Started;
        event EventHandler<EventArgs> Stopped;
    }
}
