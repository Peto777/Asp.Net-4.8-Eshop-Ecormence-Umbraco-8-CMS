namespace eshoppgsoftweb.lib.Pdf
{
    public class PdfFilePrintResult
    {
        public string FileName { get; private set; }
        public byte[] FileContent { get; private set; }

        public PdfFilePrintResult(string fileName, byte[] fileContent)
        {
            this.FileName = fileName;
            this.FileContent = fileContent;
        }
    }
}
