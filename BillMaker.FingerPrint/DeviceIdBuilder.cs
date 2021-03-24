using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using BillMaker.FingerPrint.Encoders;
using BillMaker.FingerPrint.Formatters;
using BillMaker.FingerPrint.Internal;

namespace BillMaker.FingerPrint
{
    /// <summary>
    /// Provides a fluent interface for constructing unique device identifiers.
    /// </summary>
    public class FingerPrintBuilder
    {
        /// <summary>
        /// Gets or sets the formatter to use.
        /// </summary>
        public IFingerPrintFormatter Formatter { get; set; }

        /// <summary>
        /// A set containing the components that will make up the device identifier.
        /// </summary>
#if NET35
        public HashSet<IFingerPrintComponent> Components { get; }
#else
        public ISet<IFingerPrintComponent> Components { get; }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="FingerPrintBuilder"/> class.
        /// </summary>
        public FingerPrintBuilder()
        {
            Formatter = new HashFingerPrintFormatter(() => SHA256.Create(), new Base64UrlByteArrayEncoder());
            Components = new HashSet<IFingerPrintComponent>(new FingerPrintComponentEqualityComparer());
        }

        /// <summary>
        /// Returns a string representation of the device identifier.
        /// </summary>
        /// <returns>A string representation of the device identifier.</returns>
        public override string ToString()
        {
            if (Formatter == null)
            {
                throw new InvalidOperationException($"The {nameof(Formatter)} property must not be null in order for {nameof(ToString)} to be called.");
            }

            return Formatter.GetFingerPrint(Components);
        }
    }
}
