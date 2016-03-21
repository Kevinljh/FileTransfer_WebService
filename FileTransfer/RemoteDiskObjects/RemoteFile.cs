using System;
using System.IO;

namespace RemoteDiskObjects
{
    /// <summary>
    /// Summary description for RemoteFile.
    /// </summary>

    public class RemoteFile
    {
        private string _name;
        private RemoteFolder _folder;

        #region properties

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        public RemoteFolder Folder
        {
            get
            {
                return _folder;
            }
            set
            {
                _folder = value;
            }
        }

        #endregion properties

        #region constructors

        public RemoteFile()
        {
        }

        public RemoteFile(string Name, RemoteFolder Folder)
        {
            _name = Name;
            _folder = Folder;
        }

        #endregion constructors

        #region methods

        public byte[] GetData()
        {
            string ummaskedPath = RemoteFolder.UnmaskPath(_folder.Path);
            string fullname = ummaskedPath + "\\" + _name;

            byte[] data = null;

            using (FileStream fs = File.OpenRead(fullname))
            {
                data = new byte[fs.Length];

                fs.Read(data, 0, (int)fs.Length);
            }

            return data;
        }

        #endregion methods
    }
}
