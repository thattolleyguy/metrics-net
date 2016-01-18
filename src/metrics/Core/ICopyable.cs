using System.Runtime.Serialization;
using Metrics.Support;

namespace Metrics.Core
{
    /// <summary>
    /// A marker for types that can copy themselves to another type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICopyable<out T>
    {
        /// <summary>
        /// Obtains a copy of the current type that is used in <see cref="ReadOnlyDictionary{T,TK}" /> to provide immutability
        /// </summary>
        [IgnoreDataMember]
        T Copy { get; }
    }
}