namespace BillMaker.FingerPrint.Components
{
    /// <summary>
    /// An implementation of <see cref="IFingerPrintComponent"/> that returns a constant string to indicate this type of component is not supported.
    /// </summary>
    internal sealed class UnsupportedFingerPrintComponent : IFingerPrintComponent
    {
        /// <summary>
        /// Gets the name of the component.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnsupportedFingerPrintComponent"/> class.
        /// </summary>
        /// <param name="name">The name of the component.</param>
        public UnsupportedFingerPrintComponent(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets the component value.
        /// </summary>
        /// <returns>The component value.</returns>
        public string GetValue()
        {
            return null;
        }
    }
}
