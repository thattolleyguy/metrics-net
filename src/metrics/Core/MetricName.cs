using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace metrics.Core
{
    /// <summary>
    /// A hash key for storing metrics associated by the parent class and name pair
    /// </summary>
    public struct MetricName : IComparable<MetricName>
    {
        private readonly string _name;
        private readonly ImmutableDictionary<string, string> _tags;

        public string Name
        {
            get { return _name; }
        }


        [Obsolete]
        public MetricName(string context, string name)
        {
            _name = name;
            _tags = ImmutableDictionary.Create<string, string>();
        }

        public MetricName(string name)
        {
            _name = name;
            _tags = ImmutableDictionary.Create<string, string>();
           }

        public MetricName(string name, Dictionary<string, string> tags)
        {
            _name = name;
            _tags = tags.ToImmutableDictionary();
        }

        [Obsolete]
        public MetricName(Type @class, string name)
            : this()
        {
            if (name == null) throw new ArgumentNullException("name");
            _name = name;
        }
        //TODO: Fix the following methods
        public bool Equals(MetricName other)
        {
            return string.Equals(Name, other.Name);
        }

        public int CompareTo(MetricName other)
        {
            return String.Compare(_name, other._name, StringComparison.OrdinalIgnoreCase);
        }

        public static bool operator ==(MetricName x, MetricName y)
        {
            return x.CompareTo(y) == 0;
        }

        public static bool operator !=(MetricName x, MetricName y)
        {
            return !(x == y);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is MetricName && Equals((MetricName)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0) * 397);
            }
        }

    }
}



