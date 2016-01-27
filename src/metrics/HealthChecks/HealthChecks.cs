using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metrics.Health
{
    /// <summary>
    /// A manager class for health checks
    /// </summary>
    public class HealthCheckRegistry
    {
        private readonly ConcurrentDictionary<string, HealthCheck> _checks;

        public HealthCheckRegistry()
        {
            this._checks = new ConcurrentDictionary<string, HealthCheck>();
        }

        /// <summary>
        /// Registers an application <see cref="HealthCheck" /> with a given name
        /// </summary>
        /// <param name="name">The named health check instance</param>
        /// <param name="check">The <see cref="HealthCheck" /> function</param>
        public void Register(string name, HealthCheck check)
        {
            _checks.TryAdd(name, check);
        }

        /// <summary>
        /// Unregisters the application healthcheck with the given name.
        /// </summary>
        /// <param name="name">the name of the healthcheck instance</param>
        public void Unregister(string name)
        {
            HealthCheck check;
            _checks.TryRemove(name, out check);
        }

        /// <summary>
        /// Returns a set of the names of all registered health checks.
        /// </summary>
        public ImmutableSortedSet<string> Names { get { return _checks.Keys.ToImmutableSortedSet(); } }
        /// <summary>
        /// Runs the health check with the given name. 
        /// </summary>
        /// <param name="name"> the health check's name</param>
        /// <returns>the result of the health check</returns>
        public HealthCheck.Result RunHealthCheck(string name)
        {
            HealthCheck healthCheck = null;
            if (!_checks.TryGetValue(name, out healthCheck))
                throw new InvalidOperationException("Given health check not found");
            return healthCheck.Execute();
        }

        /// <summary>
        /// Runs the registered health checks and returns a map of the results.
        /// </summary>
        public IDictionary<string, HealthCheck.Result> RunHealthChecks()
        {
            var results = new SortedDictionary<string, HealthCheck.Result>();
            foreach (var entry in _checks)
            {
                var result = entry.Value.Execute();
                results.Add(entry.Key, result);
            }
            return results;
        }

        /// <summary>
        /// Returns <code>true</code>  <see cref="HealthCheck"/>s have been registered, <code>false</code> otherwise
        /// </summary>
        public bool HasHealthChecks { get { return !_checks.IsEmpty; } }
    }
}


