using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfSign
{
    class Program
    {
        enum ExitCode : int
        {
            Success = 0,
            InvalidParameters = 1,
            FileNotFound = 2,
            OtherError = 4
        }

        static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: PdfSign.exe fileToSign.pdf");
                return (int)ExitCode.InvalidParameters;
            }
            else
            {
                string pdfFile = args[0];
                if (!File.Exists(pdfFile))
                {
                    Console.WriteLine("File '{0}' not found.", pdfFile);
                    return (int)ExitCode.FileNotFound;
                }

                try
                {
                    var certSubject = ConfigurationManager.AppSettings["certSubjectName"];
                    var reason = ConfigurationManager.AppSettings["reason"];
                    var location = ConfigurationManager.AppSettings["location"];
                    var allowInvalidCertificate = bool.Parse(ConfigurationManager.AppSettings["allowInvalidCertificate"]);
                    var backupOriginalFile = bool.Parse(ConfigurationManager.AppSettings["backupOriginalFile"]);

                    var fileContent = File.ReadAllBytes(pdfFile);
                    var signedFileContent = PdfSignHelper.SignPdf(certSubject, fileContent, reason, location, allowInvalidCertificate);

                    if (backupOriginalFile)
                        File.Move(pdfFile, pdfFile + ".original");

                    File.WriteAllBytes(pdfFile, signedFileContent);

                    return (int)ExitCode.Success;
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return (int)ExitCode.OtherError;
                }
            }
        }
    }
}
