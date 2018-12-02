using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using paracobNET;

namespace Param2Form
{
    public partial class MainForm : Form
    {
        ParamFile file;
        DataTable param_tbl;

        public MainForm()
        {
            InitializeComponent();
            groupBoxRight.Visible = false;
            param_tbl = new DataTable();
            param_tbl.Columns.Add("Hash");
            param_tbl.Columns.Add("Length");
            param_tbl.Columns.Add("Name");
            param_tbl.Columns.Add("Type");
            param_tbl.Columns.Add("Value");
            param_DataGridView.DataSource = param_tbl;
            param_DataGridView.AllowUserToAddRows = false;
            param_DataGridView.AllowUserToDeleteRows = false;
            param_DataGridView.AllowUserToOrderColumns = false;
            param_DataGridView.AutoResizeColumns();
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
            param_TreeView.Nodes.Add(Param2TreeNode(file.Root));
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

        private void openParamFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "param files|*";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                SetStatus("Opening " + dialog.FileName);
                param_tbl.Clear();
                file = new ParamFile(dialog.FileName);
                SetupTreeView();
                SetStatus("Idle");
            }
        }

        private void openParamDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void param_TreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            IParam param = (IParam)e.Node.Tag;
            switch (param.TypeKey)
            {
                case ParamType.structure:
                    {
                        param_tbl.Clear();
                        var structure = (ParamStruct)param;
                        foreach (var node in structure.Nodes)
                        {
                            DataRow row = param_tbl.NewRow();
                            row["Hash"] = "0x" + file.HashData[node.HashIndex].Hash.ToString("x8");
                            row["Length"] = file.HashData[node.HashIndex].Length;
                            row["Name"] = file.HashData[node.HashIndex].Name;
                            row["Type"] = node.Node.TypeKey.ToString();
                            if (node.Node is ParamValue)
                                row["Value"] = (node.Node as ParamValue).Value;
                            else if (node.Node is ParamArray)
                                row["Value"] = (node.Node as ParamArray).Nodes.Length;
                            else
                                row["Value"] = (node.Node as ParamStruct).Nodes.Length;
                            param_tbl.Rows.Add(row);
                        }
                        break;
                    }
                default:
                    {
                        param_tbl.Clear();
                        bool isParentStruct = e.Node.Parent != null && e.Node.Parent.Tag is ParamStruct;
                        ParamStruct.StructNode nodeInfo = null;
                        if (isParentStruct)
                            nodeInfo = (e.Node.Parent.Tag as ParamStruct).Nodes[e.Node.Index];
                        DataRow row = param_tbl.NewRow();
                        HashEntry hashData = file.HashData[nodeInfo.HashIndex];
                        row["Hash"] = isParentStruct ? "0x" + hashData.Hash.ToString("x8") : "";
                        row["Length"] = isParentStruct ? hashData.Length.ToString() : "";
                        row["Name"] = isParentStruct ? hashData.Name : "";
                        row["Type"] = param.TypeKey.ToString();
                        if (param is ParamValue)
                            row["Value"] = (param as ParamValue).Value;
                        else
                            row["Value"] = (param as ParamArray).Nodes.Length;
                        param_tbl.Rows.Add(row);
                        break;
                    }
            }
            param_DataGridView.AutoResizeColumns();
        }
    }
}
