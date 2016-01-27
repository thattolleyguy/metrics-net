using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Metrics.Reporting.Graphite
{
    class Graphite : GraphiteSender
    {

        public string Hostname { get; private set; }
        public int Port { get; private set; }

        private TcpClient _tcpClient;

        public Graphite(string hostname, int port = 2003)
        {
            Hostname = hostname;
            Port = port;
            FailureCount = 0;
        }

        public int FailureCount
        {
            get; private set;
        }

        public bool IsConnected
        {
            get
            {
                return _tcpClient != null && _tcpClient.Connected;
            }
        }

        public void Connect()
        {
            _tcpClient = new TcpClient(Hostname, Port);
        }

        public void Flush()
        {
            _tcpClient.GetStream().Flush();
        }

        public void Send(string name, string value, long timestamp)
        {
            try
            {
                string line = string.Format("{0} {1} {2}\n", sanitize(name), sanitize(value), timestamp);
                byte[] message = Encoding.UTF8.GetBytes(line);
                _tcpClient.GetStream().Write(message, 0, message.Length);
                this.FailureCount = 0;
            }
            catch (Exception ex)
            {
                FailureCount++;
                throw ex;
            }
        }

        protected string sanitize(string s)
        {
            return Regex.Replace(s, @"\s+", "-");
        }
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {

                    if (_tcpClient != null)
                    {
                        _tcpClient.Close();
                    }
                }


                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
