using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Metrics.Reporting.Graphite
{
    public class PickledGraphite
    {

        public string Hostname { get; private set; }
        public int Port { get; private set; }

        private TcpClient _tcpClient;
        private List<Tuple<string, long, string>> metrics;
        private const int DEFAULT_BATCH_SIZE = 100;

        public PickledGraphite(string hostname, int port = 2003, int batchSize = DEFAULT_BATCH_SIZE)
        {
            Hostname = hostname;
            Port = port;
            FailureCount = 0;
            metrics = new List<Tuple<string, long, string>>();
            this.batchSize = batchSize;
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
            metrics.Add(new Tuple<string, long, string>(sanitize(name), timestamp, sanitize(value)));
            if (metrics.Count >= batchSize)
            {
                writeMetrics();
            }
        }
        private void writeMetrics()
        {
            if (metrics.Count > 0)
            {
                try
                {
                    byte[] payload = pickleMetrics(metrics);
                    byte[] header = Encoding.UTF8.GetBytes(payload.Length.ToString());


                    _tcpClient.GetStream().Write(header,0,header.Length);
                    _tcpClient.GetStream().Write(payload, 0, payload.Length);
                    _tcpClient.GetStream().Flush();
                }
                catch (Exception ex)
                {
                    FailureCount++;
                    throw ex;
                }
                finally
                {
                    metrics.Clear();
                }
            }
        }

        /// <summary>
        /// Minimally necessary pickle opcodes.
        /// </summary>
        private static readonly char
                MARK = '(',
                STOP = '.',
                LONG = 'L',
                STRING = 'S',
                APPEND = 'a',
                LIST = 'l',
                TUPLE = 't',
                QUOTE = '\'',
                LF = '\n';

        /// <summary>
        /// See: http://readthedocs.org/docs/graphite/en/1.0/feeding-carbon.html
        /// </summary>
        /// <param name="metrics"></param>
        /// <returns></returns>
        byte[] pickleMetrics(List<Tuple<string, long, string>> metrics)
        {
            StringBuilder pickled = new StringBuilder();

            pickled.Append(MARK);
            pickled.Append(LIST);

            foreach (Tuple<string, long, string> tuple in metrics)
            {
                // start the outer tuple
                pickled.Append(MARK);

                // the metric name is a string.
                pickled.Append(STRING);
                // the single quotes are to match python's repr("abcd")
                pickled.Append(QUOTE);
                pickled.Append(tuple.Item1);
                pickled.Append(QUOTE);
                pickled.Append(LF);

                // start the inner tuple
                pickled.Append(MARK);

                // timestamp is a long
                pickled.Append(LONG);
                pickled.Append(tuple.Item2);
                // the trailing L is to match python's repr(long(1234))
                pickled.Append(LONG);
                pickled.Append(LF);

                // and the value is a string.
                pickled.Append(STRING);
                pickled.Append(QUOTE);
                pickled.Append(tuple.Item3);
                pickled.Append(QUOTE);
                pickled.Append(LF);

                pickled.Append(TUPLE); // inner close
                pickled.Append(TUPLE); // outer close

                pickled.Append(APPEND);
            }

            // every pickle ends with STOP
            pickled.Append(STOP);



            return Encoding.UTF8.GetBytes(pickled.ToString());
        }

        protected string sanitize(string s)
        {
            return Regex.Replace(s, @"\s+", "-");
        }
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls
        private int batchSize;

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
