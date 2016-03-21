using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Services;
using System.IO;
using System.Configuration;
using System.Xml.Linq;
using System.Linq;

namespace FileTransfer
{
    /// <summary>
    /// Summary description for Service1
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    public class FileTransfer : System.Web.Services.WebService
    {
        [WebMethod]
        public bool SaveDocument(Byte[] docbinaryarray, string docname, string path)
        {
            string strdocPath;
            strdocPath = ConfigurationManager.AppSettings["StoragePath"] + path + "\\" + docname;
            FileStream objfilestream = new FileStream(strdocPath, FileMode.Create, FileAccess.ReadWrite);
            objfilestream.Write(docbinaryarray, 0, docbinaryarray.Length);
            objfilestream.Close();
            
            return true;
        }

        [WebMethod]
        public bool NewUser(string username)
        {
            string strdocPath;
            strdocPath = ConfigurationManager.AppSettings["StoragePath"] + username;
            if (Directory.Exists(strdocPath))
            {
                return false;
            }
            Directory.CreateDirectory(strdocPath);
            return true;
        }

        [WebMethod]
        public bool LoginUser(string username)
        {
            string strdocPath;
            strdocPath = ConfigurationManager.AppSettings["StoragePath"] + username;
            if (Directory.Exists(strdocPath))
            {
                return false;
            }
            return true;
        }

        [WebMethod]
        public string GetFileStruture(string source)
        {
            string tempPath = ConfigurationManager.AppSettings["StoragePath"] + source;
            DirectoryInfo di = new DirectoryInfo(tempPath);
            return HttpUtility.HtmlDecode(new XElement("Dir",
                new XAttribute("Name", di.Name),
                from d in Directory.GetDirectories(tempPath)
                select GetFileStruture(source + d.Remove(0, Path.GetDirectoryName(d).Length)),
                from fi in di.GetFiles()
                select new XElement("File",
                    new XElement("Name", fi.Name),
                    new XElement("Length", fi.Length)
                )
            ).ToString());                
        }

        [WebMethod]
        public int GetDocumentLen(string DocumentName)
        {
            string strdocPath;
            strdocPath = ConfigurationManager.AppSettings["StoragePath"] + DocumentName;

            FileStream objfilestream = new FileStream(strdocPath, FileMode.Open, FileAccess.Read);
            int len = (int)objfilestream.Length;
            objfilestream.Close();

            return len;
        }

        [WebMethod]
        public Byte[] GetDocument(string DocumentName)
        {
            string strdocPath;
            strdocPath = ConfigurationManager.AppSettings["StoragePath"] + DocumentName;

            FileStream objfilestream = new FileStream(strdocPath, FileMode.Open, FileAccess.Read);
            int len = (int)objfilestream.Length;
            Byte[] documentcontents = new Byte[len];
            objfilestream.Read(documentcontents, 0, len);
            objfilestream.Close();

            return documentcontents;
        }

        [WebMethod]
        public void DeleteDocument(string DocumentName)
        {
            string strdocPath;
            strdocPath = ConfigurationManager.AppSettings["StoragePath"] + DocumentName;

            // get the file attributes for file or directory
            FileAttributes attr = File.GetAttributes(strdocPath);
            //detect whether its a directory or file
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                Directory.Delete(strdocPath, true);
            }
            else
            {
                File.Delete(strdocPath);
            }
        }

        [WebMethod]
        public bool NewFolder(string DocumentName)
        {
            string strdocPath;
            strdocPath = ConfigurationManager.AppSettings["StoragePath"] + DocumentName;
            if (!Directory.Exists(strdocPath))
            {
                Directory.CreateDirectory(strdocPath);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}