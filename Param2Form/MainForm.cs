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

        Dictionary<ulong, string> labels = new Dictionary<ulong, string>();
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

        private ParamTreeNode Param2TreeNode(IParam param)
        {
            ParamTreeNode node = new ParamTreeNode(param, param.TypeKey.ToString());
            switch (param.TypeKey)
            {
                case ParamType.array:
                    {
                        foreach (var child in (param as ParamArray).Nodes)
                            node.Nodes.Add(Param2TreeNode(child));
                        break;
                    }
                case ParamType.@struct:
                    {
                        foreach (var child in (param as ParamStruct).Nodes)
                        {
                            ParamTreeNode childPTN = Param2TreeNode(child.Value);
                            childPTN.ContainsHash = true;
                            childPTN.Hash = child.Key;
                            node.Nodes.Add(childPTN);
                        }
                        break;
                    }
            }
            return node;
        }

        private void OpenParamLabels(string filename)
        {
            SetStatus("Opening Labels: " + filename);
            labels = LabelIO.GetHashStringDict(filename);
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
            ParamTreeNode node = e.Node as ParamTreeNode;
            IParam param = node.Param;
            switch (param.TypeKey)
            {
                case ParamType.@struct:
                    {
                        paramTbl.Clear();
                        foreach (var child in (param as ParamStruct).Nodes)
                        {
                            DataRow row = paramTbl.NewRow();
                            row["Hash"] = Hash40Operator.FormatToString(child.Key, labels);
                            row["Type"] = child.Value.TypeKey.ToString();
                            if (child.Value is ParamValue)
                                row["Value"] = (child.Value as ParamValue).ToString(labels);
                            else if (child.Value is ParamArray)
                                row["Value"] = (child.Value as ParamArray).Nodes.Length;
                            else
                                row["Value"] = (child.Value as ParamStruct).Nodes.Count;
                            paramTbl.Rows.Add(row);
                        }
                        break;
                    }
                default:
                    {
                        paramTbl.Clear();
                        DataRow row = paramTbl.NewRow();
                        if (node.ContainsHash)
                            row["Hash"] = Hash40Operator.FormatToString(node.Hash, labels);
                        else
                            row["Hash"] = "NA";
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
