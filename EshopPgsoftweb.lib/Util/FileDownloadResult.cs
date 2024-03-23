using System.IO;
using System.Web;
using System.Web.Mvc;

namespace eshoppgsoftweb.lib.Util
{
    public class FileDownloadResult : ActionResult
    {
        public string FilePath { get; set; }

        public override void ExecuteResult(ControllerContext context)
        {
            // get file size and short name
            FileInfo fi = new FileInfo(FilePath);
            // don't forget to add error checks!
            string fname = fi.Name;
            long fsize = fi.Length;

            context.HttpContext.Response.Buffer = true;
            context.HttpContext.Response.Clear();
            context.HttpContext.Response.ContentType = "application/x-force-download; name=" + fname;
            context.HttpContext.Response.AddHeader("content-disposition", "attachment; filename=" + fname);
            context.HttpContext.Response.AddHeader("content-length", fsize.ToString());
            context.HttpContext.Response.WriteFile(this.FilePath);
            context.HttpContext.Response.End();
        }

        public static ActionResult GetActionResult(string filePath)
        {
            if (File.Exists(filePath))
            {
                FileDownloadResult downloadAction = new FileDownloadResult();
                downloadAction.FilePath = filePath;

                return downloadAction;
            }
            else
            {
                return null;
            }
        }

        public static string GetFilePathForUrl(string fileUrl)
        {
            string rootPath = HttpContext.Current.Server.MapPath(HttpContext.Current.Request.ApplicationPath).TrimEnd('\\');
            string filePath = string.Format("{0}\\{1}",
                rootPath,
                fileUrl.Replace('/', '\\').TrimStart('\\'));

            return filePath;
        }
    }

    public class PdfDownloadResult : ActionResult
    {
        public byte[] _data { get; set; }
        public string _fileName { get; set; }

        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.Buffer = true;
            context.HttpContext.Response.Clear();
            context.HttpContext.Response.ContentType = "application/pdf";
            //context.HttpContext.Response.AddHeader("content-disposition", string.Format("attachment;filename={0}", _fileName));
            context.HttpContext.Response.OutputStream.Write(_data, 0, _data.Length);
            context.HttpContext.Response.End();
        }

        public static ActionResult GetActionResult(byte[] data, string fileName)
        {
            PdfDownloadResult pdf = new PdfDownloadResult();
            pdf._data = data;
            pdf._fileName = fileName;
            return pdf;
        }
    }

    public class DataDownloadResult : ActionResult
    {
        public byte[] _data { get; set; }
        public string _fileName { get; set; }

        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.Buffer = true;
            context.HttpContext.Response.Clear();
            context.HttpContext.Response.ContentType = "application/x-force-download; name=" + _fileName;
            context.HttpContext.Response.AddHeader("content-disposition", string.Format("attachment;filename={0}", _fileName));
            context.HttpContext.Response.OutputStream.Write(_data, 0, _data.Length);
            context.HttpContext.Response.End();
        }

        public static ActionResult GetActionResult(byte[] data, string fileName)
        {
            DataDownloadResult ret = new DataDownloadResult();
            ret._data = data;
            ret._fileName = fileName;

            return ret;
        }
    }
}
