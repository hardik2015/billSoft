namespace BillMaker.FingerPrint
{
    /// <summary>
    /// Provides functionality to encode a <see cref="IFingerPrintComponent"/> as a string.
    /// </summary>
    public interface IFingerPrintComponentEncoder
    {
        /// <summary>
        /// Encodes the specified <see cref="IFingerPrintComponent"/> as a string.
        /// </summary>
        /// <param name="component">The component to encode.</param>
        /// <returns>The component encoded as a string.</returns>
        string Encode(IFingerPrintComponent component);
    }
}
