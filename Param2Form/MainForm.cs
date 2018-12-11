using paracobNET;
using System;
using System.Data;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Param2Form
{
    public partial class MainForm : Form
    {
        ParamFile paramFile;

        DataTable paramTbl;

        Dictionary<uint, string> labels = new Dictionary<uint, string>();
        const string defaultLabels = "ParamLabels.csv";

        public MainForm()
        {
            InitializeComponent();
            
            paramTbl = new DataTable();
            paramTbl.Columns.Add("Hash");
            paramTbl.Columns.Add("Type");
            paramTbl.Columns.Add("Value");
            param_DataGridView.DataSource = paramTbl;
            param_DataGridView.AllowUserToAddRows = false;
            param_DataGridView.AllowUserToDeleteRows = false;
            param_DataGridView.AllowUserToOrderColumns = false;

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
            SetStatus("Opening Labels: " + filename);
            labels = LabelIO.Read(filename);
        }

        private void openParamFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "param files|*";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                SetStatus("Opening Params: " + dialog.FileName);
                paramTbl.Clear();

                paramFile = new ParamFile(dialog.FileName);

                if (File.Exists(defaultLabels))
                    OpenParamLabels(defaultLabels);
                param_DataGridView.AutoResizeColumns();
                SetupTreeView();
                SetStatus("Idle");
            }
        }

        private void openParamLabelsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "csv files|*.csv";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
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
                            row["Hash"] = node.HashEntry.ToString(labels);
                            row["Type"] = node.Node.TypeKey.ToString();
                            if (node.Node is ParamValue)
                                row["Value"] = (node.Node as ParamValue).ToString(labels);
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
                        DataRow row = paramTbl.NewRow();
                        if (e.Node.Parent != null && e.Node.Parent.Tag is ParamStruct)
                        {
                            var paramNode = (e.Node.Parent.Tag as ParamStruct).Nodes[e.Node.Index];
                            row["Hash"] = paramNode.HashEntry.ToString(labels);
                        }
                        else
                        {
                            row["Hash"] = "NA";
                        }
                        row["Type"] = param.TypeKey.ToString();
                        if (param is ParamValue)
                            row["Value"] = (param as ParamValue).ToString(labels);
                        else
                            row["Value"] = (param as ParamArray).Nodes.Length;
                        paramTbl.Rows.Add(row);
                        break;
                    }
            }
            param_DataGridView.AutoResizeColumns();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                paramFile.Save(sfd.FileName);
            }
        }
    }
}
