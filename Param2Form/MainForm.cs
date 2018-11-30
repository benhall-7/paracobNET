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
            toolStripStatusLabel.Text = "";
            param_tbl = new DataTable();
            param_DataGridView.DataSource = param_tbl;
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

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "param files|*.*";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                SetStatus("Opening " + dialog.FileName);
                file = new ParamFile(dialog.FileName);
                SetupTreeView();
            }
            SetStatus("Idle");
        }

        private void param_TreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            param_tbl.Rows.Clear();
            param_tbl.Columns.Clear();
            IParam param = (IParam)e.Node.Tag;
            switch (param.TypeKey)
            {
                default:
                    {
                        param_tbl.Columns.Add("Type");
                        param_tbl.Columns.Add("Value");
                        var value = (ParamValue)param;
                        param_tbl.Rows.Add(value.TypeKey.ToString(), value.Value.ToString());
                        break;
                    }
                case ParamType.array:
                    {
                        param_tbl.Columns.Add("Type");
                        var array = (ParamArray)param;
                        foreach (var node in array.Nodes)
                            param_tbl.Rows.Add(node.TypeKey.ToString());
                    }
                    break;
                case ParamType.structure:
                    {
                        param_tbl.Columns.Add("Hash");
                        param_tbl.Columns.Add("Length");
                        param_tbl.Columns.Add("Name");
                        param_tbl.Columns.Add("Type");
                        param_tbl.Columns.Add("Value");
                        var structure = (ParamStruct)param;
                        foreach (var node in structure.Nodes)
                        {
                            param_tbl.Rows.Add("0x" + node.Hash.ToString("x8"),
                                node.StrLength,
                                node.Name,
                                node.Node.TypeKey.ToString(),
                                node.Node is ParamValue ? (node.Node as ParamValue).Value : "");
                        }
                        break;
                    }
            }
        }
    }
}
