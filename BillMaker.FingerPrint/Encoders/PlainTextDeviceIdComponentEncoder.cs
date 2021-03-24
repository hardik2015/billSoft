namespace BillMaker.FingerPrint.Encoders
{
    /// <summary>
    /// An implementation of <see cref="IFingerPrintComponentEncoder"/> that encodes components as plain text.
    /// </summary>
    public class PlainTextFingerPrintComponentEncoder : IFingerPrintComponentEncoder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlainTextFingerPrintComponentEncoder"/> class.
        /// </summary>
        public PlainTextFingerPrintComponentEncoder() { }

        /// <summary>
        /// Encodes the specified <see cref="IFingerPrintComponent"/> as a string.
        /// </summary>
        /// <param name="component">The component to encode.</param>
        /// <returns>The component encoded as a string.</returns>
        public string Encode(IFingerPrintComponent component)
        {
            return component.GetValue() ?? string.Empty;
        }
    }
}
