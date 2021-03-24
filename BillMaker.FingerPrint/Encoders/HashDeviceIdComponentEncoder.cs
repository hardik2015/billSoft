using System;
using System.Security.Cryptography;
using System.Text;

namespace BillMaker.FingerPrint.Encoders
{
    /// <summary>
    /// An implementation of <see cref="IFingerPrintComponentEncoder"/> that encodes components as hashes.
    /// </summary>
    public class HashFingerPrintComponentEncoder : IFingerPrintComponentEncoder
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
        /// Initializes a new instance of the <see cref="HashFingerPrintComponentEncoder"/> class.
        /// </summary>
        /// <param name="hashAlgorithm">A function that returns the hash algorithm to use.</param>
        /// <param name="byteArrayEncoder">The <see cref="IByteArrayEncoder"/> to use to encode the resulting hash.</param>
        public HashFingerPrintComponentEncoder(Func<HashAlgorithm> hashAlgorithm, IByteArrayEncoder byteArrayEncoder)
        {
            _hashAlgorithm = hashAlgorithm ?? throw new ArgumentNullException(nameof(hashAlgorithm));
            _byteArrayEncoder = byteArrayEncoder ?? throw new ArgumentNullException(nameof(byteArrayEncoder));
        }

        /// <summary>
        /// Encodes the specified <see cref="IFingerPrintComponent"/> as a string.
        /// </summary>
        /// <param name="component">The component to encode.</param>
        /// <returns>The component encoded as a string.</returns>
        public string Encode(IFingerPrintComponent component)
        {
            var value = component.GetValue() ?? string.Empty;
            var bytes = Encoding.UTF8.GetBytes(value);
            using var algorithm = _hashAlgorithm.Invoke();
            var hash = algorithm.ComputeHash(bytes);
            var output = _byteArrayEncoder.Encode(hash);
            return output;
        }
    }
}
