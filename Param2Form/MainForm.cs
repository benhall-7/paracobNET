using paracobNET;
using System;
using System.Data;
using System.IO;
using System.Windows.Forms;

namespace Param2Form
{
    public partial class MainForm : Form
    {
        ParamFile paramFile;
        string paramFileName;
        DataTable paramTbl;
        string labelFileName;
        const string defLabelFile = "ParamLabels.csv";
        DataTable labelTbl;

        public MainForm()
        {
            InitializeComponent();

            paramFileName = null;
            paramTbl = new DataTable();
            paramTbl.Columns.Add("Hash");
            paramTbl.Columns.Add("Length");
            paramTbl.Columns.Add("Name");
            paramTbl.Columns.Add("Type");
            paramTbl.Columns.Add("Value");
            param_DataGridView.DataSource = paramTbl;
            param_DataGridView.AllowUserToAddRows = false;
            param_DataGridView.AllowUserToDeleteRows = false;
            param_DataGridView.AllowUserToOrderColumns = false;
            //param_DataGridView.AutoResizeColumns();

            labelFileName = null;
            labelTbl = new DataTable();
            labelTbl.Columns.Add("Index");
            labelTbl.Columns.Add("Hash");
            labelTbl.Columns.Add("Length");
            labelTbl.Columns.Add("Name");
            label_DataGridView.DataSource = labelTbl;
            label_DataGridView.AllowUserToAddRows = false;
            label_DataGridView.AllowUserToDeleteRows = false;
            label_DataGridView.AllowUserToOrderColumns = false;
            //label_DataGridView.AutoResizeColumns();

            //groupBoxRight.Visible = false;
            openParamDatabaseToolStripMenuItem.Enabled = false;

            SetStatus("Idle");
        }

        private void SetStatus(string text)
        {
            toolStripStatusLabel.Text = "Status: " + text;
            statusStrip.Refresh();
        }

        private void SetupTreeView()
        {
            param_TreeView.Nodes.Clear();
            param_TreeView.Nodes.Add(Param2TreeNode(paramFile.Root));
            param_TreeView.Nodes[0].Expand();
        }

        private TreeNode Param2TreeNode(IParam param)
        {
            TreeNode node = new TreeNode(param.TypeKey.ToString());
            switch (param.TypeKey)
            {
                case ParamType.array:
                    {
                        ParamArray paramArray = (ParamArray)param;
                        foreach (var sub_node in paramArray.Nodes)
                            node.Nodes.Add(Param2TreeNode(sub_node));
                        break;
                    }
                case ParamType.structure:
                    {
                        ParamStruct paramStruct = (ParamStruct)param;
                        foreach (var sub_node in paramStruct.Nodes)
                            node.Nodes.Add(Param2TreeNode(sub_node.Node));
                        break;
                    }
            }
            node.Tag = param;
            return node;
        }

        private void OpenParamLabels(string filename)
        {
            labelFileName = filename;
            SetStatus("Opening Labels: " + filename);
            paramFile.ReadLabels(filename);
            for (int i = 0; i < paramFile.HashData.Length; i++)
                labelTbl.Rows[i]["Name"] = paramFile.HashData[i].Name;
        }

        private void openParamFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "param files|*";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                SetStatus("Opening Params: " + dialog.FileName);
                paramTbl.Clear();
                labelTbl.Clear();
                paramFile = new ParamFile(dialog.FileName);
                for (int i = 0; i < paramFile.HashData.Length; i++)
                {
                    var hash = paramFile.HashData[i];
                    DataRow row = labelTbl.NewRow();
                    row["Index"] = i;
                    row["Hash"] = "0x" + hash.Hash.ToString("x8");
                    row["Length"] = hash.Length;
                    row["Name"] = "";
                    labelTbl.Rows.Add(row);
                }
                openParamDatabaseToolStripMenuItem.Enabled = true;
                if (File.Exists(defLabelFile))
                    OpenParamLabels(defLabelFile);
                param_DataGridView.AutoResizeColumns();
                label_DataGridView.AutoResizeColumns();
                SetupTreeView();
                SetStatus("Idle");
            }
        }

        private void openParamDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "csv files|*.csv";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                labelFileName = dialog.FileName;
                OpenParamLabels(dialog.FileName);
                SetStatus("Idle");
            }
        }

        private void param_TreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            IParam param = (IParam)e.Node.Tag;
            switch (param.TypeKey)
            {
                case ParamType.structure:
                    {
                        paramTbl.Clear();
                        var structure = (ParamStruct)param;
                        foreach (var node in structure.Nodes)
                        {
                            DataRow row = paramTbl.NewRow();
                            row["Hash"] = "0x" + paramFile.HashData[node.HashIndex].Hash.ToString("x8");
                            row["Length"] = paramFile.HashData[node.HashIndex].Length;
                            row["Name"] = paramFile.HashData[node.HashIndex].Name;
                            row["Type"] = node.Node.TypeKey.ToString();
                            if (node.Node is ParamValue)
                                row["Value"] = (node.Node as ParamValue).Value;
                            else if (node.Node is ParamArray)
                                row["Value"] = (node.Node as ParamArray).Nodes.Length;
                            else
                                row["Value"] = (node.Node as ParamStruct).Nodes.Length;
                            paramTbl.Rows.Add(row);
                        }
                        break;
                    }
                default:
                    {
                        paramTbl.Clear();
                        ParamStruct.StructNode nodeInfo = null;
                        HashEntry hashData = null;
                        DataRow row = paramTbl.NewRow();
                        if (e.Node.Parent != null && e.Node.Parent.Tag is ParamStruct)
                        {
                            nodeInfo = (e.Node.Parent.Tag as ParamStruct).Nodes[e.Node.Index];
                            hashData = paramFile.HashData[nodeInfo.HashIndex];
                            row["Hash"] = "0x" + hashData.Hash.ToString("x8");
                            row["Length"] = hashData.Length.ToString();
                            row["Name"] = hashData.Name;
                        }
                        else
                        {
                            row["Hash"] = "NA";
                            row["Length"] = "NA";
                            row["Name"] = "NA";
                        }
                        row["Type"] = param.TypeKey.ToString();
                        if (param is ParamValue)
                            row["Value"] = (param as ParamValue).Value;
                        else
                            row["Value"] = (param as ParamArray).Nodes.Length;
                        paramTbl.Rows.Add(row);
                        break;
                    }
            }
            param_DataGridView.AutoResizeColumns();
        }
    }
}
