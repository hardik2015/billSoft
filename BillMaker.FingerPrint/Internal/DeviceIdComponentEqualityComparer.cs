using System;
using System.Collections.Generic;

namespace BillMaker.FingerPrint.Internal
{
    /// <summary>
    /// Defines methods to support the comparison of <see cref="IFingerPrintComponent"/> instances for equality.
    /// </summary>
    internal sealed class FingerPrintComponentEqualityComparer : IEqualityComparer<IFingerPrintComponent>
    {
        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>true if the specified objects are equal; otherwise, false.</returns>
        public bool Equals(IFingerPrintComponent x, IFingerPrintComponent y)
        {
            return StringComparer.OrdinalIgnoreCase.Equals(x.Name, y.Name);
        }

        /// <summary>
        /// Returns a hash code for the specified object.
        /// </summary>
        /// <param name="obj">The object for which a hash code is to be returned.</param>
        /// <returns>A hash code for the specified object.</returns>
        public int GetHashCode(IFingerPrintComponent obj)
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Name);
        }
    }
}
