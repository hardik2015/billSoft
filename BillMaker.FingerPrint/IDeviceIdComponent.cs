namespace BillMaker.FingerPrint
{
    /// <summary>
    /// Represents a component that forms part of a device identifier.
    /// </summary>
    public interface IFingerPrintComponent
    {
        /// <summary>
        /// Gets the name of the component.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the component value.
        /// </summary>
        /// <returns>The component value.</returns>
        string GetValue();
    }
}
