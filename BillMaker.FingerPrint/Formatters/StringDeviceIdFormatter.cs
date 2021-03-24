using System;
using System.Collections.Generic;
using System.Linq;

namespace BillMaker.FingerPrint.Formatters
{
    /// <summary>
    /// An implementation of <see cref="IFingerPrintFormatter"/> that combines the components into a concatenated string.
    /// </summary>
    public class StringFingerPrintFormatter : IFingerPrintFormatter
    {
        /// <summary>
        /// The <see cref="IFingerPrintComponentEncoder"/> instance to use to encode individual components.
        /// </summary>
        private readonly IFingerPrintComponentEncoder _encoder;

        /// <summary>
        /// The delimiter to use when concatenating the encoded component values.
        /// </summary>
        private readonly string _delimiter;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlFingerPrintFormatter"/> class.
        /// </summary>
        /// <param name="encoder">The <see cref="IFingerPrintComponentEncoder"/> instance to use to encode individual components.</param>
        public StringFingerPrintFormatter(IFingerPrintComponentEncoder encoder)
            : this(encoder, ".") { }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlFingerPrintFormatter"/> class.
        /// </summary>
        /// <param name="encoder">The <see cref="IFingerPrintComponentEncoder"/> instance to use to encode individual components.</param>
        /// <param name="delimiter">The delimiter to use when concatenating the encoded component values.</param>
        public StringFingerPrintFormatter(IFingerPrintComponentEncoder encoder, string delimiter)
        {
            _encoder = encoder ?? throw new ArgumentNullException(nameof(encoder));
            _delimiter = delimiter;
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

            return string.Join(_delimiter, components.OrderBy(x => x.Name).Select(x => _encoder.Encode(x)).ToArray());
        }
    }
}
