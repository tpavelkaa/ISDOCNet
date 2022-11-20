namespace ISDOCNet.Console
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var filePath = @"C:\test\input.isdoc";
            var fileSavePath = @"C:\test\output.isdoc";
            DWHandler handler = new DWHandler("https://sbdpraha.docuware.cloud/DocuWare/Platform/Home", "dwsystem", "J6tFAstZBXqluR0nqKvW");
            //var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Open);
            //var invoice = ISDOCNet.Invoice.Load(stream);

            var invoice = Sample.SimpleInvoiceVat.Create(handler.GetDocument(2, "6df6782b-709d-4f31-9ae0-931a35a0e39e", "e5957e43-9e80-4ebd-a646-6edb08d9eb58"));
            invoice.Save(fileSavePath);
        }
    }
}
