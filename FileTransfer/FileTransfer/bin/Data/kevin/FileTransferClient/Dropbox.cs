using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using RemoteDiskObjects;
using System.Xml.Linq;
using System.Web;
using System.Xml;

namespace FileTransferClient
{
    public partial class Dropbox : Form
    {
        string sFile = "D:\\用户目录\\Desktop\\FileTransfer\\FileTransferClient\\bin\\Debug\\Data\\a2.txt";
        string currentuser = null;
        bool login = false;

        public Dropbox()
        {
            InitializeComponent();
        }

        private void RefreshFileList(string username)
        {
            //To do
            FileTransfer.FileTransferSoapClient myservice = new FileTransfer.FileTransferSoapClient();
            string temp = myservice.GetFileStruture(username);
            XElement xml = new XElement(XElement.Parse(temp));
            XmlDataDocument xmldoc = new XmlDataDocument();
            xmldoc.LoadXml(xml.ToString());
            XmlNode xmlnode;
            xmlnode = xmldoc.FirstChild;
            treeView1.Nodes.Clear();
            treeView1.Nodes.Add(new TreeNode(xmldoc.DocumentElement.GetAttribute("Name")));
            TreeNode tNode;
            tNode = treeView1.Nodes[0];
            AddNode(xmlnode, tNode);
        }

        //create dir treeview
        private void AddNode(XmlNode inXmlNode, TreeNode inTreeNode)
        {
            XmlNode xNode;
            TreeNode tNode;
            XmlNodeList nodeList;
            int i = 0;
            if (inXmlNode.HasChildNodes)
            {
                nodeList = inXmlNode.ChildNodes;
                for (i = 0; i <= nodeList.Count - 1; i++)
                {
                    xNode = inXmlNode.ChildNodes[i];
                    if (xNode.Name == "Dir")
                    {
                        inTreeNode.Nodes.Add(new TreeNode(xNode.Attributes["Name"].Value));
                        tNode = inTreeNode.Nodes[i];
                        AddNode(xNode, tNode);
                    }
                    else if (xNode.Name == "File")
                    {
                        inTreeNode.Nodes.Add(xNode.FirstChild.InnerText);
                    }
                }
            }
        }

        private void UploadButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog()
            {
                Title = "Select a file to upload",
                RestoreDirectory = true,
                CheckFileExists = true
            };

            dlg.ShowDialog();

            if (!string.IsNullOrEmpty(dlg.FileName))
            {
                //file name
                string virtualPath = Path.GetFileName(dlg.FileName);
                FileStream objfilestream = new FileStream(dlg.FileName, FileMode.Open, FileAccess.Read);
                int len = (int)objfilestream.Length;
                Byte[] mybytearray = new Byte[len];
                objfilestream.Read(mybytearray, 0, len);
                FileTransfer.FileTransferSoapClient myservice = new FileTransfer.FileTransferSoapClient();
                myservice.SaveDocument(mybytearray, virtualPath);
                objfilestream.Close();

                RefreshFileList(currentuser);
            }

            
        }

        private void DownloadButton_Click(object sender, EventArgs e)
        {
            // Ask where it should be saved
            //SaveFileDialog dlg = new SaveFileDialog()
            //{
            //    RestoreDirectory = true,
            //    OverwritePrompt = true,
            //    Title = "Save as...",
            //    FileName = Path.GetFileName(path)
            //};

            //dlg.ShowDialog(this);


            //MemoryStream objstreaminput = new MemoryStream();
            //FileStream objfilestream = new FileStream(sFile.Insert(sFile.LastIndexOf("."), "2"), FileMode.Create, FileAccess.ReadWrite);

            //FileTransfer.FileTransferSoapClient myservice = new FileTransfer.FileTransferSoapClient();
            //int len = (int)myservice.GetDocumentLen(sFile.Remove(0, sFile.LastIndexOf("\\") + 1));
            //Byte[] mybytearray = new Byte[len];
            //mybytearray = myservice.GetDocument(sFile.Remove(0, sFile.LastIndexOf("\\") + 1));
            //objfilestream.Write(mybytearray, 0, len);
            //objfilestream.Close();
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {

        }

        private void Register_Click(object sender, EventArgs e)
        {
            FileTransfer.FileTransferSoapClient myservice = new FileTransfer.FileTransferSoapClient();
            bool bResult = myservice.NewUser(textBox1.Text);
            if (!bResult)
            {
                MessageBox.Show("User name already exisits, please login");
                return;
            }
            MessageBox.Show("Register Success");
            login = true;
            currentuser = textBox1.Text;
            RefreshFileList(textBox1.Text);
        }

        private void Login_Click(object sender, EventArgs e)
        {
            FileTransfer.FileTransferSoapClient myservice = new FileTransfer.FileTransferSoapClient();
            bool bResult = myservice.LoginUser(textBox1.Text);
            if (!bResult)
            {
                MessageBox.Show("Login Success");
                login = true;
                currentuser = textBox1.Text;
                RefreshFileList(textBox1.Text);
                return;
            }
            else if (bResult)
            {
                MessageBox.Show("User no find");
                return;
            }
        }
    }
}
