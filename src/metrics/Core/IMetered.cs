namespace Metrics.Core
{
    /// <summary>
    /// An object which maintains mean and exponentially-weighted rate.
    /// </summary>
    public interface IMetered : ICounted
    {
        /// <summary>
        /// Returns the fifteen-minute exponentially-weighted moving average rate at
        /// which events have occured since the meter was created
        /// <remarks>
        /// This rate has the same exponential decay factor as the fifteen-minute load
        /// average in the top Unix command.
        /// </remarks> 
        /// </summary>
        double FifteenMinuteRate { get; }

        /// <summary>
        /// Returns the five-minute exponentially-weighted moving average rate at
        /// which events have occured since the meter was created
        /// <remarks>
        /// This rate has the same exponential decay factor as the five-minute load
        /// average in the top Unix command.
        /// </remarks>
        /// </summary>
        double FiveMinuteRate { get; }

        /// <summary>
        /// Returns the mean rate at which events have occured since the meter was created
        /// </summary>
        double MeanRate { get; }

        /// <summary>
        /// Returns the one-minute exponentially-weighted moving average rate at
        /// which events have occured since the meter was created
        /// <remarks>
        /// This rate has the same exponential decay factor as the one-minute load
        /// average in the top Unix command.
        /// </remarks>
        /// </summary>
        /// <returns></returns>
        double OneMinuteRate { get; }
    }
}
