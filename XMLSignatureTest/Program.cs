using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
//using System.Xml;
//using System.Xml.Linq;
using System.Security.Cryptography.Xml;
using System.IO;
using System.Xml.XPath;
using HtmlAgilityPack;
using HtmlToXml;
using System.Reflection;
//using AngleSharp;
//using AngleSharp.Dom;


namespace XMLSignatureTest
{
    internal class Program
    {
        
        [STAThread]
        static void Main(string[] args)
        {
            string cert_file = "..\\..\\..\\..\\MyCertificate.pfx";
            string xml_file = "..\\..\\..\\..\\PENDIENTE PROCESO - Pro_77303395-1_33_4285.xml";
            string xml_load_save = "..\\..\\..\\..\\ReadAndSaved.xml";
            //CertificateCreation cert = new CertificateCreation();
            //cert.CreateSelfSigned_X509Certificate2(cert_file, "", "MyCertificate");

            My_XmlSignature xmlSignature = new My_XmlSignature();
            //var xmldocument = xmlSignature.LoadXml(xml_file);
            //var xmldocument2 = xmlSignature.LoadXml2(xml_file);
            var htmlDocument = xmlSignature.LoadHtml(xml_file);

            Console.WriteLine($"Caracteres leidos del Archivo: {htmlDocument.DocumentNode.OuterHtml.Length}");

            var errores = htmlDocument.ParseErrors.ToList();
            if (errores.Count > 0)
            {
                Console.WriteLine("Hay Errores de Parseo...");
                errores.ForEach(parseError => Console.WriteLine(parseError.Reason));
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("No hay Errores de Parseo...");
            }

            if (File.Exists(xml_load_save))
            {
                File.Delete(xml_load_save);
            }

            HtmlNode current = htmlDocument.DocumentNode;
            Console.WriteLine($"Name : {current.Name} Type: {current.NodeType}");

            //HtmlNode nav = htmlDocument.DocumentNode.SelectSingleNode(".//cesion");
            //if (nav != null)
            //{
            //    Console.WriteLine($"Nodo Cesion: {nav.InnerText}");

            //    var newNodeStr = "<Cesion version=\"1.0\" xmlns=\"http://www.sii.cl/SiiDte\">Prueba de Modificación</Cesion>";
            //    var newNode = HtmlNode.CreateNode(newNodeStr);

            //    nav.ParentNode.ReplaceChild(newNode, nav);
            //    //nav.InnerHtml = "ESTE ES EL NUEVO CONTENIDO";
            //    Console.WriteLine("Modificado:");
            //    Console.WriteLine($"Nodo Cesion: {nav.InnerText}");
            //}
            //else
            //{
            //    Console.WriteLine("Nodo Cesion no encontrado");
            //}

            Console.WriteLine();

            string newHtml = My_XmlSignature.CorrectClosingTags(htmlDocument.DocumentNode.OuterHtml);
            File.WriteAllText(xml_load_save, newHtml, Encoding.GetEncoding("iso-8859-1"));
            
            // Console.ReadLine();
            /// Esto Falla porque realiza conversión de los Tags 
            //htmlDocument.Save(xml_load_save, Encoding.GetEncoding("iso-8859-1"));

            Console.WriteLine("Finalizado...");
            Console.ReadLine();
        }
    }


    public class My_XmlSignature
    {

        public static string CorrectClosingTags(string text)
        {
            const string toReplace = ">";
            const string Replacement = "/>";

            List<string> tags = new List<string>() { "CanonicalizationMethod",
                                                     "DigestMethod",
                                                     "SignatureMethod",
                                                     "Transform"
                                                    };
            foreach (string stag in tags)
            {
                text = text.Replace($"</{stag}>", "");

                string tag = $"<{stag} ";
                int start = 0;
                int pos = 0;
                while (pos >= 0)
                {
                    pos = text.Substring(start).IndexOf(tag, StringComparison.Ordinal);
                    if (pos > 0)
                    {
                        int pos2 = text.Substring(start+pos).IndexOf(toReplace, StringComparison.Ordinal);
                        if (pos2 > 0)
                        {
                            text = text.Remove(start + pos + pos2, toReplace.Length);
                            text = text.Insert(start + pos + pos2, Replacement);
                        }
                    }
                    else
                        break;
                    start += pos+1;
                    if (start >= text.Length)
                        break;
                }
            }
            return text;
        }

        

        public static FieldInfo IsImplicitEndProperty = typeof(HtmlNode).GetField("_isImplicitEnd", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        public HtmlDocument LoadHtml(string fileName)
        {
            
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.OptionOutputOriginalCase = true;
            htmlDocument.OptionAutoCloseOnEnd = false;
            htmlDocument.OptionEmptyCollection = true;
            htmlDocument.OptionFixNestedTags = true;
            htmlDocument.OptionXmlForceOriginalComment = true;
            htmlDocument.OptionWriteEmptyNodes = true;

            List<string> tags = new List<string>() { "canonicalizationmethod",
                                                     "digestmethod",
                                                     "signaturemethod",
                                                     "transform"
                                                    };

            htmlDocument.Load(fileName);

            foreach (string tag in tags)
            {
                var nodes = htmlDocument.DocumentNode.SelectNodes($".//{tag}");
                if (nodes != null)
                {
                    nodes.ToList().ForEach(x =>
                    {
                        IsImplicitEndProperty.SetValue(x, true);
                    }
                    );
                }
            }

            return htmlDocument;
        }



        //public static void firmarDocumentoXml(ref XmlDocument xmldocument, X509Certificate2 certificado, string referenciaUri)
        //{
        //    // Create a SignedXml object.
        //    SignedXml signedXml = new SignedXml(xmldocument);

        //    // Add the key to the SignedXml document.  'key'
        //    signedXml.SigningKey = certificado.PrivateKey;

        //    // Get the signature object from the SignedXml object.
        //    Signature XMLSignature = signedXml.Signature;

        //    // Create a reference to be signed.  Pass "" 
        //    // to specify that all of the current XML
        //    // document should be signed.
        //    Reference reference = new Reference();
        //    reference.Uri = referenciaUri;

        //    // Add the Reference object to the Signature object.
        //    XMLSignature.SignedInfo.AddReference(reference);

        //    // Add an RSAKeyValue KeyInfo (optional; helps recipient find key to validate).
        //    KeyInfo keyInfo = new KeyInfo();
        //    keyInfo.AddClause(new RSAKeyValue((RSA)certificado.PrivateKey));

        //    ////
        //    //// Agregar información del certificado x509
        //    //// X509Certificate MSCert = X509Certificate.CreateFromCertFile(Certificate);
        //    keyInfo.AddClause(new KeyInfoX509Data(certificado));

        //    // Add the KeyInfo object to the Reference object.
        //    XMLSignature.KeyInfo = keyInfo;


        //    // Compute the signature.
        //    signedXml.ComputeSignature();

        //    // Get the XML representation of the signature and save
        //    // it to an XmlElement object.
        //    XmlElement xmlDigitalSignature = signedXml.GetXml();

        //    ///
        //    /// inserte la firma en el DTE
        //    xmldocument.DocumentElement.AppendChild(xmldocument.ImportNode(xmlDigitalSignature, true));

        //}

    }


    public class CertificateCreation
    {
        public void CreateSelfSigned_X509Certificate2(string certificateFileName, string certificatePassword, string certificateSubjectName)
            {
            // Create a new X509Certificate2 object
            X509Certificate2 cert = new X509Certificate2();

            // Create a new RSA key
            RSA rsa = RSA.Create();

            // Create a certificate request
            CertificateRequest certRequest = new CertificateRequest(
                "cn=" + certificateSubjectName,
                rsa,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);

            // Add a basic constraint extension
            certRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));

            // Add a subject key identifier extension
            certRequest.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(certRequest.PublicKey, false));

            // Create a self-signed certificate
            cert = certRequest.CreateSelfSigned(new DateTimeOffset(DateTime.Now), new DateTimeOffset(DateTime.Now.AddYears(1)));

            // Export the certificate to a file
            byte[] certBytes = cert.Export(X509ContentType.Pfx, certificatePassword);
            File.WriteAllBytes(certificateFileName, certBytes);
        }
        
    }



}
