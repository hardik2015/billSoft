using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace BillMaker.FingerPrint.Formatters
{
    /// <summary>
    /// An implementation of <see cref="IFingerPrintFormatter"/> that combines the components into a hash.
    /// </summary>
    public class HashFingerPrintFormatter : IFingerPrintFormatter
    {
        /// <summary>
        /// A function that returns the hash algorithm to use.
        /// </summary>
        private readonly Func<HashAlgorithm> _hashAlgorithm;

        /// <summary>
        /// The <see cref="IByteArrayEncoder"/> to use to encode the resulting hash.
        /// </summary>
        private readonly IByteArrayEncoder _byteArrayEncoder;

        /// <summary>
        /// Initializes a new instance of the <see cref="HashFingerPrintFormatter"/> class.
        /// </summary>
        /// <param name="hashAlgorithm">A function that returns the hash algorithm to use.</param>
        /// <param name="byteArrayEncoder">The <see cref="IByteArrayEncoder"/> to use to encode the resulting hash.</param>
        public HashFingerPrintFormatter(Func<HashAlgorithm> hashAlgorithm, IByteArrayEncoder byteArrayEncoder)
        {
            _hashAlgorithm = hashAlgorithm ?? throw new ArgumentNullException(nameof(hashAlgorithm));
            _byteArrayEncoder = byteArrayEncoder ?? throw new ArgumentNullException(nameof(byteArrayEncoder));
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

            var value = string.Join(",", components.OrderBy(x => x.Name).Select(x => x.GetValue()).ToArray());
            var bytes = Encoding.UTF8.GetBytes(value);
            using var algorithm = _hashAlgorithm.Invoke();
            var hash = algorithm.ComputeHash(bytes);
            return _byteArrayEncoder.Encode(hash);
        }
    }
}
