using System.Collections.Generic;

namespace BillMaker.FingerPrint
{
    /// <summary>
    /// Provides a method to combine a number of <see cref="IFingerPrintComponent"/> instances
    /// into a single device identifier string.
    /// </summary>
    public interface IFingerPrintFormatter
    {
        /// <summary>
        /// Returns the device identifier string created by combining the specified <see cref="IFingerPrintComponent"/> instances.
        /// </summary>
        /// <param name="components">A sequence containing the <see cref="IFingerPrintComponent"/> instances to combine into the device identifier string.</param>
        /// <returns>The device identifier string.</returns>
        string GetFingerPrint(IEnumerable<IFingerPrintComponent> components);
    }
}
