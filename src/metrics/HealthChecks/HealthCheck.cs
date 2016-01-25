using System;
using System.Text;
using Metrics.Core;

namespace Metrics
{
    /// <summary>
    /// A template class for an encapsulated service health check
    /// </summary>
    public class HealthCheck
    {
        public sealed class Result
        {
            private static readonly Result HEALTHY = new Result(true, null, null);

            public static Result Healthy()
            {
                return HEALTHY;
            }

            public static Result Healthy(string message)
            {
                return new Result(true, message, null);
            }

            public static Result Healthy(string message, params object[] args)
            {
                return Healthy(string.Format(message, args));
            }

            public static Result Unhealthy(string message)
            {
                return new Result(false, message, null);
            }

            public static Result Unhealthy(string message, params object[] args)
            {
                return Unhealthy(string.Format(message, args));
            }

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


        public HealthCheck(Func<Result> check)
        {
            _check = check;
        }

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