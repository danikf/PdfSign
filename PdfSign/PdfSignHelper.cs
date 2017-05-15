using iTextSharp.text.pdf;
using iTextSharp.text.pdf.security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace PdfSign
{
    internal static class PdfSignHelper
    {
        //Sample from: http://www.rahulsingla.com/blog/2012/09/digitally-sign-and-verify-pdf-documents-in-c-using-itextsharp-5-3-x-library
        // http://stackoverflow.com/questions/14997118/how-do-i-sign-a-pdf-document-using-a-certificate-from-the-windows-cert-store

        /// <summary>
        /// Signs a PDF document using iTextSharp library
        /// </summary>
        /// <param name="certSubjectName">Cerificate subject (prefix) in local certStore.</param>
        /// <param name="sourceDocument">The path of the source pdf document which is to be signed</param>
        /// <param name="destinationPath">The path at which the signed pdf document should be generated</param>
        /// <param name="reason">String describing the reason for signing, would be embedded as part of the signature</param>
        /// <param name="location">Location where the document was signed, would be embedded as part of the signature</param>
        /// <param name="allowInvalidCertificate">Allows also usage of invalid certificate from store.</param>
        public static byte[] SignPdf(string certSubjectName, byte[] sourceDocument, string reason, string location, bool allowInvalidCertificate)
        {
            try
            {
                // reader and stamper
                using (PdfReader reader = new PdfReader(sourceDocument))
                {
                    using (MemoryStream fout = new MemoryStream())
                    {
                        PdfStamper stamper = PdfStamper.CreateSignature(reader, fout, '\0');
                        // appearance
                        PdfSignatureAppearance appearance = stamper.SignatureAppearance;
                        appearance.Reason = reason;
                        appearance.Location = location;
                        // digital signature

                        ICollection<Org.BouncyCastle.X509.X509Certificate> certChain;
                        IExternalSignature es = ResolveExternalSignatureFromCertStore(certSubjectName, allowInvalidCertificate, out certChain);

                        MakeSignature.SignDetached(appearance, es, certChain, null, null, null, 0, CryptoStandard.CMS);

                        stamper.Close();
                        return fout.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Exception during pdf sign: {0}", ex.Message);
                throw;
            }
        }

        private static IExternalSignature ResolveExternalSignatureFromCertStore(string certSubjectName, bool allowInvalidCertificate, out ICollection<Org.BouncyCastle.X509.X509Certificate> chain)
        {
            // Acquire certificate chain
            var certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            certStore.Open(OpenFlags.ReadOnly);

            try
            {
                X509Certificate2Collection certCollection = certStore.Certificates.Find(X509FindType.FindBySubjectName, certSubjectName, !allowInvalidCertificate);
                if (certCollection.Count <= 0)
                    throw new Exception(string.Format("Certificate with SUBJ: '{0}' not found in cert store (or certifiacte is invalid).", certSubjectName));
                X509Certificate cert = certCollection.Cast<X509Certificate>().Single();
                X509Certificate2 signatureCert = new X509Certificate2(cert);

                // iTextSharp needs this cert as a BouncyCastle X509 object; this converts it.
                Org.BouncyCastle.X509.X509Certificate bcCert = Org.BouncyCastle.Security.DotNetUtilities.FromX509Certificate(cert);
                chain = new List<Org.BouncyCastle.X509.X509Certificate> { bcCert };

                var pk = Org.BouncyCastle.Security.DotNetUtilities.GetKeyPair(signatureCert.PrivateKey).Private;
                return new PrivateKeySignature(pk, "SHA-256");
            }
            finally
            {
                certStore.Close();
            }
        }
    }
}
