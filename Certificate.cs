using System;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using iTextSharp;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.security;
using System.Collections.Generic;
using Org.BouncyCastle.X509;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;
using System.Security.Cryptography.X509Certificates;
using iTextSharp.text;
using iTextSharp.text.pdf.interfaces;
using System.Drawing;

using Rectangle = iTextSharp.text.Rectangle;
using Org.BouncyCastle.Crypto.Tls;

namespace DS
{
    public sealed class SingletonCertificate : X509Certificate2
    {
        public X509Certificate2 Certificate
        {
            get
            {
                X509Certificate2 certificate = GetCertificate(); ;
                return certificate;
            }
        }

        private static SingletonCertificate instanceCertificate = null;
        private static readonly object lockObject = new object();

        private X509Certificate2 certificate = null;

        private SingletonCertificate() { }

        public static SingletonCertificate InstanceCertificate
        {
            get
            {
                lock (lockObject)
                {
                    if (instanceCertificate == null)
                    {
                        instanceCertificate = new SingletonCertificate();
                    }
                }
                return instanceCertificate;
            }
        }

        public X509Certificate2 GetCertificate()
        {
            if (certificate == null)
            {
                X509Store store = new X509Store(StoreLocation.CurrentUser);

                store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadOnly);

                X509Certificate2Collection collectionCretificates = store.Certificates;

                X509Certificate2Collection collection = X509Certificate2UI.SelectFromCollection(collectionCretificates, "Selecione um certificate", "Escolha um certificado para examinar.", X509SelectionFlag.SingleSelection);

                if (collection.Count == 0)
                {
                    return null;
                }


                certificate = collection[0];

                RSACryptoServiceProvider csp = null;

                csp = (RSACryptoServiceProvider)certificate.PrivateKey;

                SHA1Managed sha1 = new SHA1Managed();

                UnicodeEncoding encoding = new UnicodeEncoding();

                try
                {
                    csp.SignHash(sha1.ComputeHash(encoding.GetBytes("Lokesh")), CryptoConfig.MapNameToOID("SHA1"));
                }
                catch (Exception ex)
                {
                    return null;

                    throw;
                }

                return certificate;
            }
            else
            {
                return certificate;
            }


        }

        public void SignPDF(String pathFileSource, String pathFileDestination, X509Certificate2 certificate)
        {

            PdfReader pdfReader = null;

            try
            {
                pdfReader = new PdfReader(pathFileSource);
                SignPDF(pdfReader, pathFileDestination, certificate);
            }
            catch (Exception ex)
            {
                Console.WriteLine("GMA: " + ex.Message);
            }
            finally
            {
                if (pdfReader != null)
                    pdfReader.Close();
            }
        }

        public void SignPDF(Stream pathFileSource, String pathFileDestination, X509Certificate2 certificate)
        {
            PdfReader pdfReader = null;

            try
            {
                pdfReader = new PdfReader(pathFileSource);
                SignPDF(pdfReader, pathFileDestination, certificate);
            }
            catch (Exception ex)
            {
                Console.WriteLine("GMA: " + ex.Message);
            }
            finally
            {
                if (pdfReader != null)
                    pdfReader.Close();
            }

        }

        private void SignPDF(PdfReader pdfReader, String pathFileDestination, X509Certificate2 certificate)
        {

            int estimatedSize = 0;

            ICollection<ICrlClient> crlList = null;
            IOcspClient ocspClient = null;
            ITSAClient tsaClient = null;
            PdfStamper pdfStamper = null;
            FileStream fileStream = null;

            try
            {

                X509CertificateParser parser = new X509CertificateParser();

                X509Certificate[] chain = new X509Certificate[] { parser.ReadCertificate(certificate.RawData) };

                fileStream = new FileStream(pathFileDestination, FileMode.Create);

                pdfStamper = PdfStamper.CreateSignature(pdfReader, fileStream, '\0', null, true);

                PdfSignatureAppearance appearance = pdfStamper.SignatureAppearance;

                Rectangle rectangle = new Rectangle(50, 810);

                appearance.SetVisibleSignature(rectangle, 1, "Assinatura");

                appearance.Reason = "Assinatura digital";

                appearance.SignatureRenderingMode = PdfSignatureAppearance.RenderingMode.DESCRIPTION;

                appearance.Certificate = new X509CertificateParser().ReadCertificate(certificate.RawData);

                string userNameCertificate = certificate.GetNameInfo(X509NameType.SimpleName, false);

                //TODO : substituir pelo local do posto de coleta
                appearance.Location = "Salvador/Ba";

                PdfTemplate layer2 = appearance.GetLayer(2);

                layer2.Transform(new iTextSharp.awt.geom.AffineTransform(0, 1, -1, 0, rectangle.Width, 0));

                var ct2 = new ColumnText(layer2)
                {
                    RunDirection = PdfWriter.RUN_DIRECTION_NO_BIDI,
                };

                iTextSharp.text.Font font = new iTextSharp.text.Font();
                font.SetFamily("HELVETICA");
                font.SetColor(250, 0, 0);
                font.Size = 7;

                string assinatura = userNameCertificate + " - RENOVA TECNOLOGIA - Assinado digitalmente em " + DateTime.Now;


                ct2.SetSimpleColumn(new Phrase(assinatura, font), 10, 10, rectangle.Height, rectangle.Width, 15, Element.ALIGN_CENTER);

                ct2.Go();

                IExternalSignature pks = new X509Certificate2Signature(certificate, "SHA1");

                MakeSignature.SignDetached(appearance, pks, chain, crlList, ocspClient, tsaClient, estimatedSize, CryptoStandard.CADES);

            }

            catch (Exception ex)
            {
                Console.WriteLine("GMA: " + ex.Message);
            }
            finally
            {
                if (pdfStamper != null)
                    pdfStamper.Close();
                if (fileStream != null)
                    fileStream.Close();
            }
        }

    }
}
