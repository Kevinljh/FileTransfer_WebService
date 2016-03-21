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
       string currentuser = null;
        bool login = false;

        public Dropbox()
        {
            InitializeComponent();
        }

        private void RefreshFileList(string username)
        {
            try
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
                treeView1.ExpandAll();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //create dir treeview
        private void AddNode(XmlNode inXmlNode, TreeNode inTreeNode)
        {
            try
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        private void UploadButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (treeView1.SelectedNode != null)
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
                        TreeNode tempNode = treeView1.SelectedNode;
                        string path = "\\" + tempNode.Text;
                        while (tempNode.Parent != null)
                        {
                            tempNode = tempNode.Parent;
                            path = path.Insert(0, "\\" + tempNode.Text);
                        }
                        //file name
                        string fileName = Path.GetFileName(dlg.FileName);
                        FileStream objfilestream = new FileStream(dlg.FileName, FileMode.Open, FileAccess.Read);
                        int len = (int)objfilestream.Length;
                        Byte[] mybytearray = new Byte[len];
                        objfilestream.Read(mybytearray, 0, len);
                        FileTransfer.FileTransferSoapClient myservice = new FileTransfer.FileTransferSoapClient();
                        myservice.SaveDocument(mybytearray, fileName, path);
                        objfilestream.Close();

                        RefreshFileList(currentuser);
                    }
                }
                else
                {
                    MessageBox.Show("Please select a folder");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        private void DownloadButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (treeView1.SelectedNode != null)
                {
                    string fileName = treeView1.SelectedNode.Text;
                    //Ask where it should be saved
                    SaveFileDialog dlg = new SaveFileDialog()
                    {
                        RestoreDirectory = true,
                        OverwritePrompt = true,
                        Title = "Save as...",
                        FileName = fileName
                    };

                    dlg.ShowDialog(this);

                    if (!string.IsNullOrEmpty(dlg.FileName))
                    {
                        TreeNode tempNode = treeView1.SelectedNode;
                        string path = "\\" + tempNode.Text;
                        while (tempNode.Parent != null)
                        {
                            tempNode = tempNode.Parent;
                            path = path.Insert(0, "\\" + tempNode.Text);
                        }

                        // Get the file from the server
                        MemoryStream objstreaminput = new MemoryStream();
                        FileStream objfilestream = new FileStream(Path.GetFullPath(dlg.FileName), FileMode.Create, FileAccess.ReadWrite);
                        FileTransfer.FileTransferSoapClient myservice = new FileTransfer.FileTransferSoapClient();
                        int len = (int)myservice.GetDocumentLen(path);
                        Byte[] mybytearray = new Byte[len];
                        mybytearray = myservice.GetDocument(path);
                        objfilestream.Write(mybytearray, 0, len);
                        objfilestream.Close();
                    }
                }
                else
                {
                    MessageBox.Show("Please select a file");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (treeView1.SelectedNode != null)
                {
                    TreeNode tempNode = treeView1.SelectedNode;
                    string path = "\\" + tempNode.Text;
                    if (tempNode.Parent != null)
                    {
                        //confirm box
                        var confirmResult = MessageBox.Show("Are you sure to delete " + tempNode.Text + "??",
                                             "Confirm Delete!!",
                                             MessageBoxButtons.YesNo);
                        if (confirmResult == DialogResult.Yes)
                        {
                            while (tempNode.Parent != null)
                            {
                                tempNode = tempNode.Parent;
                                path = path.Insert(0, "\\" + tempNode.Text);
                            }
                            FileTransfer.FileTransferSoapClient myservice = new FileTransfer.FileTransferSoapClient();
                            myservice.DeleteDocument(path);
                            RefreshFileList(currentuser);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Cannot delete root folder");
                    }
                }
                else
                {
                    MessageBox.Show("Please select a file");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        private void Register_Click(object sender, EventArgs e)
        {
            try
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
                RefreshFileList(currentuser);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        private void Login_Click(object sender, EventArgs e)
        {
            try
            {
                FileTransfer.FileTransferSoapClient myservice = new FileTransfer.FileTransferSoapClient();
                bool bResult = myservice.LoginUser(textBox1.Text);
                if (!bResult)
                {
                    MessageBox.Show("Login Success");
                    login = true;
                    currentuser = textBox1.Text;
                    RefreshFileList(currentuser);
                    return;
                }
                else if (bResult)
                {
                    MessageBox.Show("User no find");
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }
        //new folder
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (treeView1.SelectedNode != null)
                {
                    TreeNode tempNode = treeView1.SelectedNode;
                    string path = "\\" + tempNode.Text;
                    while (tempNode.Parent != null)
                    {
                        tempNode = tempNode.Parent;
                        path = path.Insert(0, "\\" + tempNode.Text);
                    }
                    string defaultNmae = "Folder Name";
                    if (InputBox("New Folder Nmae", "", ref defaultNmae) == DialogResult.OK)
                    {
                        FileTransfer.FileTransferSoapClient myservice = new FileTransfer.FileTransferSoapClient();
                        bool result = myservice.NewFolder(path + "\\" + defaultNmae);
                        if(!result)
                        {
                            MessageBox.Show("Folder alrealy exists");
                        }
                        RefreshFileList(currentuser);
                    }
                }
                else
                {
                    MessageBox.Show("Please select a root folder");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        //input box
        public static DialogResult InputBox(string title, string promptText, ref string value)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }
    }
}
