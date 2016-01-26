using System;
using System.Text;
using Metrics.Core;

namespace Metrics.Health
{
    /// <summary>
    /// A template class for an encapsulated service health check. This class is enforced so that all exceptions are caught
    /// and handled correctly
    /// </summary>
    public class HealthCheck
    {
        public sealed class Result
        {
            private static readonly Result HEALTHY = new Result(true, null, null);

            /// <summary>
            /// Returns a healthy <see cref="Result"/> with no additional message.
            /// </summary>
            /// <returns>healthy <see cref="Result"/></returns>
            public static Result Healthy()
            {
                return HEALTHY;
            }
            /// <summary>
            /// Returns a healthy <see cref="Result"/> with an additional message.
            /// </summary>
            /// <returns>healthy <see cref="Result"/></returns>
            public static Result Healthy(string message)
            {
                return new Result(true, message, null);
            }

            /// <summary>
            /// Returns a healthy <see cref="Result"/> with a formatted message.
            /// <para />
            /// Message formatting follows the same rules as <see cref="string.Format(string, object[])"/> 
            /// </summary>
            /// <param name="message">a message format</param>
            /// <param name="args">the arguments to apply to the message format</param>
            /// <returns>a health <see cref="Result"/> with an addition message</returns>
            /// <seealso cref="string.Format(string, object[])"/>
            public static Result Healthy(string message, params object[] args)
            {
                return Healthy(string.Format(message, args));
            }

            /// <summary>
            /// Returns an unhealthy <see cref="Result"/> with the given message
            /// </summary>
            /// <param name="message">an informative message describing how the health check failed</param>
            /// <returns>an unhealthy <see cref="Result"/>  with the given message</returns>
            public static Result Unhealthy(string message)
            {
                return new Result(false, message, null);
            }

            /// <summary>
            /// Returns an unhealthy <see cref="Result"/> with a formatted message.
            /// <para />
            /// Message formatting follows the same rules as <see cref="string.Format(string, object[])"/> 
            /// </summary>
            /// <param name="message">a message format</param>
            /// <param name="args">the arguments to apply to the message format</param>
            /// <returns>an unhealthy <see cref="Result"/>  with the given message</returns>
            public static Result Unhealthy(string message, params object[] args)
            {
                return Unhealthy(string.Format(message, args));
            }

            /// <summary>
            /// Returns an unhealthy <see cref="Result"/> with the given error.
            /// </summary>
            /// <param name="error">an exception thrown during the health check</param>
            /// <returns>an unhealthy <see cref="Result"/> with the given error</returns>
            public static Result Unhealthy(Exception error)
            {
                return new Result(false, error.Message, error);
            }

            public string Message { get; private set; }

            public Exception Error { get; private set; }

            public bool IsHealthy { get; private set; }

            private Result(bool isHealthy, string message, Exception error)
            {
                IsHealthy = isHealthy;
                Message = message;
                Error = error;
            }
        }


        private readonly Func<Result> _check;

        /// <summary>
        /// Creates a new health check based off the underlying check function
        /// </summary>
        /// <param name="check">Function that returns the health state of the health check</param>
        public HealthCheck(Func<Result> check)
        {
            _check = check;
        }

        /// <summary>
        /// Executes the health check, catching and handling any exceptions raised by the check function
        /// </summary>
        /// <returns>if the component is healthy, a healthy <see cref="Result"/>; otherwise, an unhealthy 
        /// <see cref="Result"/> with a descriptive error message or exception
        /// </returns>
        public Result Execute()
        {
            try
            {
                return _check();
            }
            catch (Exception e)
            {
                return Result.Unhealthy(e);
            }
        }


    }
}