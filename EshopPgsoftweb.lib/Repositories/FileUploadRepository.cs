using NPoco;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace eshoppgsoftweb.lib.Repositories
{
    public class FileUploadRepository
    {
        /// <summary>
        /// Default mail attachement path
        /// </summary>
        public static string DefaultPath = "Media\\EcommerceUpload";

        public object UploadFile()
        {
            try
            {
                HttpPostedFile hpf = HttpContext.Current.Request.Files["file"] as HttpPostedFile;
                string category = HttpContext.Current.Request.Params["category"];
                string fileName = HttpContext.Current.Request.Params["fileName"];

                string dirPath = GetPathForCategory(category);

                DirectoryInfo di = Directory.CreateDirectory(dirPath);// If you don't have the folder yet, you need to create.
                string sentFileName = Path.GetFileName(hpf.FileName); //it can be just a file name or a user local path! it depends on the used browser. So we need to ensure that this var will contain just the file name.
                if (!string.IsNullOrEmpty(fileName))
                {
                    // Set desired file name
                    sentFileName = string.Format("{0}.{1}", fileName, Path.GetExtension(sentFileName).Trim('.'));
                }
                string savedFileName = Path.Combine(di.FullName, sentFileName);
                hpf.SaveAs(savedFileName);

                return new { msg = "File Uploaded", filename = hpf.FileName, url = savedFileName, srvfilename = sentFileName };
            }
            catch (Exception e)
            {
                //If you want this working with a custom error you need to change in file jquery.uploadfile.js, the name of 
                //variable customErrorKeyStr in line 85, from jquery-upload-file-error to jquery_upload_file_error 
                return new { msg = e.Message, jquery_upload_file_error = e.Message };
            }
        }

        public List<FileUploadInfo> GetFiles(string category)
        {
            // Get list of files
            string dirPath = GetPathForCategory(category);
            if (!Directory.Exists(dirPath))
            {
                return new List<FileUploadInfo>();
            }

            FileDescriptionCollection fileDescription = new FileDescriptionCollection(category);
            string[] files = Directory.GetFiles(dirPath, "*.*", SearchOption.TopDirectoryOnly);

            // sort files by name
            SortedDictionary<string, FileUploadInfo> sd = new SortedDictionary<string, FileUploadInfo>();
            string fileName;
            foreach (string filePath in files)
            {
                fileName = GetFileName(filePath);
                sd.Add(fileName.ToLower(), new FileUploadInfo()
                {
                    FileType = "file",
                    FileName = fileName,
                    FileUrl = GetFileUrl(category, filePath),
                    FileDescription = fileDescription.GetFileDescription(fileName)
                });
            }

            // Create result list
            List<FileUploadInfo> result = new List<FileUploadInfo>();
            foreach (FileUploadInfo fi in sd.Values)
            {
                result.Add(fi);
            }

            return result;
        }

        public object DeleteFile(string id)
        {
            try
            {
                string filePath = string.Format("{0}{1}", HttpContext.Current.Server.MapPath(GetRootPath()), id);
                File.Delete(filePath);

                return new { msg = "File Deleted" };
            }
            catch (Exception e)
            {
                return new { msg = e.Message };
            }
        }

        public void DeleteCategory(string category)
        {
            string path = GetPathForCategory(category);
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        private string GetPathForCategory(string category)
        {
            return string.Format("{0}{1}\\{2}",
                HttpContext.Current.Server.MapPath(GetRootPath()),
                FileUploadRepository.DefaultPath, category);
        }

        private string GetRootPath()
        {
            return HttpContext.Current.Request.ApplicationPath;
        }

        private string GetFileUrl(string category, string filePath)
        {
            string categoryPath = string.Format("\\{0}\\{1}\\", FileUploadRepository.DefaultPath, category);
            int pos = filePath.IndexOf(categoryPath);

            return filePath.Substring(pos).Replace('\\', '/');
        }
        private string GetFileName(string filePath)
        {
            return Path.GetFileName(filePath);
        }

        public string SetFileDescription(string category, string fileName, string fileDescription)
        {
            try
            {
                FileDescriptionRepository rep = new FileDescriptionRepository();
                FileDescription dataRec = rep.Get(category, fileName);
                if (dataRec == null || rep.IsNew(dataRec))
                {
                    dataRec = new FileDescription()
                    {
                        Category = category,
                        FileName = fileName
                    };
                }
                dataRec.Description = fileDescription;
                rep.Save(dataRec);

                return dataRec.Description;
            }
            catch
            {
                return string.Empty;
            }
        }

        public void RenameCategory(string oldCategory, string newCategory)
        {
            string oldPath = GetPathForCategory(oldCategory).Replace("/", "\\");
            string newPath = GetPathForCategory(newCategory).Replace("/", "\\");

            Directory.Move(oldPath, newPath);
        }

        public void RenameFileInCategory(string category, string oldFileName, string newFileName, bool newFileNameIsRenameMask = false)
        {
            string categoryPath = GetPathForCategory(category).TrimEnd('\\');
            if (newFileNameIsRenameMask)
            {
                newFileName = string.Format("{0}{1}", newFileName, Path.GetExtension(oldFileName));
            }

            string oldFilePath = string.Format("{0}\\{1}", categoryPath, oldFileName);
            if (File.Exists(oldFilePath))
            {
                File.Move(oldFilePath, string.Format("{0}\\{1}", categoryPath, newFileName));
            }
        }
    }

    public class FileUploadInfo
    {
        public string FileType { get; set; }
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public string FileDescription { get; set; }
    }

    public class FileDescriptionRepository : _BaseRepository
    {
        public List<FileDescription> GetForCategory(string category)
        {
            var sql = GetBaseQuery().Where(GetCategoryWhereClause(), new { Category = category });

            return Fetch<FileDescription>(sql);
        }

        public FileDescription Get(string category, string file)
        {
            var sql = GetBaseQuery().Where(GetCategoryWhereClause(), new { Category = category });
            sql.Where(GetFileWhereClause(), new { File = file });

            return Fetch<FileDescription>(sql).FirstOrDefault();
        }

        public FileDescription Get(Guid key)
        {
            var sql = GetBaseQuery().Where(GetBaseWhereClause(), new { Key = key });

            return Fetch<FileDescription>(sql).FirstOrDefault();
        }

        public bool Save(FileDescription dataRec)
        {
            if (IsNew(dataRec))
            {
                return Insert(dataRec);
            }
            else
            {
                return Update(dataRec);
            }
        }

        bool Insert(FileDescription dataRec)
        {
            dataRec.pk = Guid.NewGuid();

            object result = InsertInstance(dataRec);
            if (result is Guid)
            {
                return (Guid)result == dataRec.pk;
            }

            return false;
        }

        bool Update(FileDescription dataRec)
        {
            return UpdateInstance(dataRec);
        }

        public bool Delete(FileDescription dataRec)
        {
            return DeleteInstance(dataRec);
        }

        Sql GetBaseQuery()
        {
            return new Sql(string.Format("SELECT * FROM {0}", FileDescription.DbTableName));
        }

        string GetBaseWhereClause()
        {
            return string.Format("{0}.pk = @Key", FileDescription.DbTableName);
        }

        string GetCategoryWhereClause()
        {
            return string.Format("{0}.category = @Category", FileDescription.DbTableName);
        }

        string GetFileWhereClause()
        {
            return string.Format("{0}.fileName = @File", FileDescription.DbTableName);
        }
    }

    [TableName(FileDescription.DbTableName)]
    [PrimaryKey("pk", AutoIncrement = false)]
    public class FileDescription : _BaseRepositoryRec
    {
        public const string DbTableName = "epFileDescription";

        public string Category { get; set; }
        public string FileName { get; set; }
        public string Description { get; set; }
    }

    public class FileDescriptionCollection
    {
        Hashtable ht;

        public FileDescriptionCollection(string category)
        {
            FileDescriptionRepository rep = new FileDescriptionRepository();
            List<FileDescription> dataList = rep.GetForCategory(category);

            ht = new Hashtable(dataList.Count + 1);
            foreach (FileDescription dataRec in dataList)
            {
                if (!ht.ContainsKey(dataRec.FileName))
                {
                    ht.Add(dataRec.FileName, dataRec);
                }
            }
        }

        public string GetFileDescription(string file)
        {
            return ht.ContainsKey(file) ? ((FileDescription)ht[file]).Description : string.Empty;
        }
    }
}
