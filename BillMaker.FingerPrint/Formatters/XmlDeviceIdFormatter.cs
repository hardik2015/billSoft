using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace BillMaker.FingerPrint.Formatters
{
    /// <summary>
    /// An implementation of <see cref="IFingerPrintFormatter"/> that combines the components into an XML string.
    /// </summary>
    public class XmlFingerPrintFormatter : IFingerPrintFormatter
    {
        /// <summary>
        /// The <see cref="IFingerPrintComponentEncoder"/> instance to use to encode individual components.
        /// </summary>
        private readonly IFingerPrintComponentEncoder _encoder;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlFingerPrintFormatter"/> class.
        /// </summary>
        /// <param name="encoder">The <see cref="IFingerPrintComponentEncoder"/> instance to use to encode individual components.</param>
        public XmlFingerPrintFormatter(IFingerPrintComponentEncoder encoder)
        {
            _encoder = encoder ?? throw new ArgumentNullException(nameof(encoder));
        }

        /// <summary>
        /// Returns the device identifier string created by combining the specified <see cref="IFingerPrintComponent"/> instances.
        /// </summary>
        /// <param name="components">A sequence containing the <see cref="IFingerPrintComponent"/> instances to combine into the device identifier string.</param>
        /// <returns>The device identifier string.</returns>
        public string GetFingerPrint(IEnumerable<IFingerPrintComponent> components)
        {
            if (components == null)
            {
                throw new ArgumentNullException(nameof(components));
            }

            var document = new XDocument(GetElement(components));
            return document.ToString(SaveOptions.DisableFormatting);
        }

        /// <summary>
        /// Returns an <see cref="XElement"/> representing the specified collection of <see cref="IFingerPrintComponent"/> instances.
        /// </summary>
        /// <param name="components">The sequence of <see cref="IFingerPrintComponent"/> instances to represent.</param>
        /// <returns>An <see cref="XElement"/> representing the specified collection of <see cref="IFingerPrintComponent"/> instances</returns>
        private XElement GetElement(IEnumerable<IFingerPrintComponent> components)
        {
            var elements = components
                .OrderBy(x => x.Name)
                .Select(x => GetElement(x));

            return new XElement("FingerPrint", elements);
        }

        /// <summary>
        /// Returns an <see cref="XElement"/> representing the specified <see cref="IFingerPrintComponent"/> instance.
        /// </summary>
        /// <param name="component">The <see cref="IFingerPrintComponent"/> to represent.</param>
        /// <returns>An <see cref="XElement"/> representing the specified <see cref="IFingerPrintComponent"/> instance.</returns>
        private XElement GetElement(IFingerPrintComponent component)
        {
            return new XElement("Component",
                new XAttribute("Name", component.Name),
                new XAttribute("Value", _encoder.Encode(component)));
        }
    }
}
