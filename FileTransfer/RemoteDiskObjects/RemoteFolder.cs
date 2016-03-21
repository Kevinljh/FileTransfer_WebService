using System;
using System.IO;
using System.Text;
using System.Configuration;
using System.Collections.Generic;

namespace RemoteDiskObjects
{
    /// <summary>
    /// Summary description for RemoteFolder.
    /// </summary>
    public class RemoteFolder
    {
        private string _path;
        static private string _rootPath;

        const string _rootDrive = @"$:";

        #region properties

        public string Path
        {
            get
            {
                return _path;
            }
            set
            {
                _path = value;
            }
        }

        #endregion properties

        #region constructors

        public RemoteFolder()
        {
            InitRootPath();
        }

        public RemoteFolder(string Path)
        {
            InitRootPath();

            _path = Path;
        }

        #endregion constructors

        #region methods

        private void InitRootPath()
        {
            _rootPath = ConfigurationManager.AppSettings["StoragePath"];
        }

        private string MaskPath(string Path)
        {
            string maskedPath = null;

            if (Path.StartsWith(_rootPath))
            {
                maskedPath = Path.Replace(_rootPath, _rootDrive);
            }

            return maskedPath;
        }

        internal static string UnmaskPath(string Path)
        {
            string unmaskedPath = null;

            if (Path.StartsWith(_rootDrive))
            {
                unmaskedPath = Path.Replace(_rootDrive, _rootPath);
            }

            return unmaskedPath;
        }

        public static RemoteFolder GetRootFolder()
        {
            RemoteFolder root = new RemoteFolder(_rootDrive);

            return root;
        }

        public List<RemoteFolder> GetSubFolders()
        {
            string ummaskedPath = UnmaskPath(_path);
            string[] paths = Directory.GetDirectories(ummaskedPath);

            List<RemoteFolder> folders = new List<RemoteFolder>();
            folders.Capacity = paths.Length;

            for (int i = 0; i < paths.Length; i++)
            {
                string maskedPath = MaskPath(paths[i]);
                folders.Add(new RemoteFolder(maskedPath));
            }

            return folders;
        }

        public List<RemoteFile> GetFiles()
        {
            //string ummaskedPath = UnmaskPath(_path);
            string[] names = Directory.GetFiles(_path);

            List<RemoteFile> files = new List<RemoteFile>();
            files.Capacity = names.Length;

            for (int i = 0; i < names.Length; i++)
            {
                files.Add(new RemoteFile(
                    System.IO.Path.GetFileName(names[i]), this));
            }

            return files;
        }

        #endregion methods
    }
}
