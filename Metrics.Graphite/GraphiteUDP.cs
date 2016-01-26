using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Metrics.Reporting.Graphite
{
    public class GraphiteUDP : GraphiteSender
    {

        private readonly string hostname;
        private readonly int port;
        private int failures;
        private UdpClient _udpClient;

        /**
         * Creates a new client which connects to the given address and socket factory using the given
         * character set.
         *
         * @param hostname The hostname of the Carbon server
         * @param port The port of the Carbon server
         * @param socketFactory the socket factory
         * @param charset       the character set used by the server
         */
        public GraphiteUDP(string hostname, int port)
        {
            this.hostname = hostname;
            this.port = port;
        }



        public virtual void Connect()
        {
            if (IsConnected)
            {
                throw new InvalidOperationException("Already connected");
            }
            this._udpClient = new UdpClient(hostname, port);
        }

        public virtual bool IsConnected
        {
            get
            { return _udpClient != null; }
        }

        public virtual void Send(string name, string value, long timestamp)
        {

            if (!IsConnected)
                Connect();
            try
            {
                string line = string.Format("{0} {1} {2}\n", sanitize(name), sanitize(value), timestamp);
                byte[] message = Encoding.UTF8.GetBytes(line);
                _udpClient.Send(message, message.Length);
                this.failures = 0;
            }
            catch (Exception e)
            {
                failures++;
                throw e;
            }
        }

        public virtual int FailureCount
        {
            get { return failures; }
        }

        public virtual void Flush()
        {
            // Nothing to do
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
                    if (_udpClient != null)
                        _udpClient.Close();
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
