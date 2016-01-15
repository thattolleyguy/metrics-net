using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace metrics.Core
{
    /// <summary>
    /// A hash key for storing metrics associated by the parent class and name pair
    /// </summary>
    public class MetricName : IComparable<MetricName>
    {
        public static readonly string SEPARATOR = ".";
        public static readonly IDictionary<string, string> EMPTY_TAGS = new Dictionary<string, string>().ToImmutableDictionary();
        public static readonly MetricName EMPTY = new MetricName();


        private readonly string _key;
        private readonly IDictionary<string, string> _tags;


        public MetricName()
        {
            this._key = null;
            this._tags = EMPTY_TAGS;
        }

        public MetricName(string name)
        {
            _key = name;
            _tags = EMPTY_TAGS;
        }

        public MetricName(string name, IDictionary<string, string> tags)
        {
            _key = name;
            _tags = checkTags(tags);
        }
        private IDictionary<string, string> checkTags(IDictionary<string, string> tags)
        {
            if (tags == null || tags.Count == 0)
                return EMPTY_TAGS;
            return tags.ToImmutableDictionary();
        }

        public string Key
        {
            get { return _key; }
        }

        public IDictionary<string, string> Tags
        {
            get { return _tags; }
        }
        public MetricName resolve(string p)
        {
            string next;

            if (p != null && p.Length != 0)
            {
                if (_key != null && _key.Length != 0)
                {
                    next = _key + SEPARATOR + p;
                }
                else {
                    next = p;
                }
            }
            else {
                next = this._key;
            }

            return new MetricName(next, _tags);
        }

        public MetricName tagged(IDictionary<string, string> add)
        {
            Dictionary<string, string> tags = new Dictionary<string, string>(add);
            foreach (var tag in _tags)
                tags.Add(tag.Key, tag.Value);
            return new MetricName(_key, tags);
        }

        public MetricName tagged(params string[] pairs)
        {
            if (pairs == null)
            {
                return this;
            }

            if (pairs.Length % 2 != 0)
            {
                throw new ArgumentException("Argument count must be even");
            }

            Dictionary<string, string> add = new Dictionary<string, string>();

            for (int i = 0; i < pairs.Length; i += 2)
            {
                add.Add(pairs[i], pairs[i + 1]);
            }

            return tagged(add);
        }

        /**
         * Join the specified set of metric names.
         *
         * @param parts Multiple metric names to join using the separator.
         * @return A newly created metric name which has the name of the specified
         *         parts and includes all tags of all child metric names.
         **/
        public static MetricName join(params MetricName[] parts)
        {
            stringBuilder nameBuilder = new stringBuilder();
            Dictionary<string, string> tags = new Dictionary<string, string>();

            bool first = true;
            foreach (MetricName part in parts)
            {
                string name = part.Key;

                if (name != null && name.Length != 0)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else {
                        nameBuilder.Append(SEPARATOR);
                    }

                    nameBuilder.Append(name);
                }

                if (part.Tags.Count != 0)
                    foreach (var tag in part.Tags)
                        tags.Add(tag.Key, tag.Value);

            }

            return new MetricName(nameBuilder.Tostring(), tags);
        }

        public static MetricName build(params string[] parts)
        {
            if (parts == null || parts.Length == 0)
                return MetricName.EMPTY;

            if (parts.Length == 1)
                return new MetricName(parts[0], EMPTY_TAGS);

            return new MetricName(buildName(parts), EMPTY_TAGS);
        }

        private static string buildName(params string[] names)
        {
            stringBuilder builder = new stringBuilder();
            bool first = true;

            foreach (string name in names)
            {
                if (name == null || name.Length==0)
                    continue;

                if (first)
                {
                    first = false;
                }
                else {
                    builder.Append(SEPARATOR);
                }

                builder.Append(name);
            }

            return builder.Tostring();
        }

        public string tostring()
        {
            if (_tags.Count==0)
            {
                return _key;
            }

            return _key + _tags;
        }

        public override int GetHashCode()
        {
            unchecked
            {

                const int prime = 31;
                int result = 1;
                result = prime * result + ((_key == null) ? 0 : _key.GetHashCode());
                result = prime * result + ((_tags == null) ? 0 : _tags.GetHashCode());
                return result;
            }
        }
        //TODO: Fix the following methods
        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;

            if (obj == null)
                return false;

            if (GetType() != obj.GetType())
                return false;

            MetricName other = (MetricName)obj;

            if (_key == null)
            {
                if (other._key != null)
                    return false;
            }
            else if (!_key.Equals(other._key))
                return false;

            if (!_tags.Equals(other._tags))
                return false;

            return true;
        }

        public int CompareTo(MetricName o)
        {
            if (o == null)
                return -1;

            int c = CompareName(_key, o._key);

            //if (c != 0)
                return c;

            //return CompareTags(_tags, o._tags);
        }

        private int CompareName(string left, string right)
        {
            if (left == null && right == null)
                return 0;

            if (left == null)
                return 1;

            if (right == null)
                return -1;

            return left.CompareTo(right);
        }


       /* private int compareTags(IDictionary<string, string> left, IDictionary<string, string> right)
        {
            if (left == null && right == null)
                return 0;

            if (left == null)
                return 1;

            if (right == null)
                return -1;

            const Iterable< string > keys = uniqueSortedKeys(left, right);

            for (final string key : keys)
            {
                final string a = left.get(key);
                final string b = right.get(key);

                if (a == null && b == null)
                    continue;

                if (a == null)
                    return -1;

                if (b == null)
                    return 1;

                int c = a.compareTo(b);

                if (c != 0)
                    return c;
            }

            return 0;
        }*/

        public static bool operator ==(MetricName x, MetricName y)
        {
            return x.CompareTo(y) == 0;
        }

        public static bool operator !=(MetricName x, MetricName y)
        {
            return !(x == y);
        }
        
        public static MetricName name(Type @class, params string[] names)
        {
            return name(@class.Name, names);
        }

        public static MetricName name(string name, params string[] names)
        {
            int length;

            if (names == null)
            {
                length = 0;
            }
            else {
                length = names.Length;
            }

            string[] parts = new string[length + 1];
            parts[0] = name;

            for (int i = 0; i < length; i++)
            {
                parts[i + 1] = names[i];
            }

            return MetricName.build(parts);
        }



      

    }
}



