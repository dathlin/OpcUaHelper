using Opc.Ua;
using Opc.Ua.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpcUaHelper.Forms
{

    /// <summary>
    /// 一个浏览OPC服务器节点的窗口类
    /// </summary>
    public partial class FormBrowseServer : Form
    {
        #region Constructor
        /// <summary>
        /// 允许自己输入服务器地址的实例化
        /// </summary>
        public FormBrowseServer()
        {
            InitializeComponent();
            
            Icon = ClientUtils.GetAppIcon();
        }

        /// <summary>
        /// 固定地址且不允许更改的实例化
        /// </summary>
        public FormBrowseServer(string server)
        {
            InitializeComponent();
            
            Icon = ClientUtils.GetAppIcon();
            textBox1.Text = server;
        }

        #endregion

        #region Load Show Close


        private void FormBrowseServer_Load(object sender, EventArgs e)
        {
            BrowseNodesTV.Enabled = false;

            BrowseNodesTV.ImageList = new ImageList();
            BrowseNodesTV.ImageList.Images.Add("Class_489", Properties.Resources.Class_489);
            BrowseNodesTV.ImageList.Images.Add("ClassIcon", Properties.Resources.ClassIcon);
            BrowseNodesTV.ImageList.Images.Add("brackets", Properties.Resources.brackets_Square_16xMD);
            BrowseNodesTV.ImageList.Images.Add("VirtualMachine", Properties.Resources.VirtualMachine);
            BrowseNodesTV.ImageList.Images.Add("Enum_582", Properties.Resources.Enum_582);
            BrowseNodesTV.ImageList.Images.Add("Method_636", Properties.Resources.Method_636);
            BrowseNodesTV.ImageList.Images.Add("Module_648", Properties.Resources.Module_648);
            BrowseNodesTV.ImageList.Images.Add("Loading", Properties.Resources.loading);

            // 判断是否允许更改
            if (!string.IsNullOrEmpty(textBox1.Text)) textBox1.ReadOnly = true;
            // Opc Ua 服务的初始化
            OpcUaClientInitialization();
        }



        private string GetImageKeyFromDescription(ReferenceDescription target, NodeId sourceId)
        {
            if (target.NodeClass == NodeClass.Variable)
            {
                DataValue dataValue = m_OpcUaClient.ReadNode((NodeId)target.NodeId);

                if (dataValue.WrappedValue.TypeInfo != null)
                {
                    if (dataValue.WrappedValue.TypeInfo.ValueRank == ValueRanks.Scalar)
                    {
                        return "Enum_582";
                    }
                    else if (dataValue.WrappedValue.TypeInfo.ValueRank == ValueRanks.OneDimension)
                    {
                        return "brackets";
                    }
                    else if (dataValue.WrappedValue.TypeInfo.ValueRank == ValueRanks.TwoDimensions)
                    {
                        return "Module_648";
                    }
                    else
                    {
                        return "ClassIcon";
                    }
                }
                else
                {
                    return "ClassIcon";
                }
            }
            else if (target.NodeClass == NodeClass.Object)
            {
                if (sourceId == ObjectIds.ObjectsFolder)
                {
                    return "VirtualMachine";
                }
                else
                {
                    return "ClassIcon";
                }
            }
            else if (target.NodeClass == NodeClass.Method)
            {
                return "Method_636";
            }
            else
            {
                return "ClassIcon";
            }
        }

        private void FormBrowseServer_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_OpcUaClient.Disconnect();
        }

        #endregion

        #region OPC UA client

        /// <summary>
        /// Opc客户端的核心类
        /// </summary>
        private OpcUaClient m_OpcUaClient = null;

        /// <summary>
        /// 初始化
        /// </summary>
        private void OpcUaClientInitialization()
        {
            m_OpcUaClient = new OpcUaClient();
            m_OpcUaClient.OpcStatusChange += M_OpcUaClient_OpcStatusChange1; ;
            m_OpcUaClient.ConnectComplete += M_OpcUaClient_ConnectComplete;
        }
        /// <summary>
        /// 连接服务器结束后马上浏览根节点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void M_OpcUaClient_ConnectComplete(object sender, EventArgs e)
        {
            try
            {
                OpcUaClient client = (OpcUaClient)sender;
                if (client.Connected)
                {
                    // populate the browse view.
                    PopulateBranch(ObjectIds.ObjectsFolder, BrowseNodesTV.Nodes);
                    BrowseNodesTV.Enabled = true;
                }
            }
            catch (Exception exception)
            {
                ClientUtils.HandleException(Text, exception);
            }
        }

        /// <summary>
        /// OPC 客户端的状态变化后的消息提醒
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void M_OpcUaClient_OpcStatusChange1(object sender, OpcUaStatusEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() =>
                {
                    M_OpcUaClient_OpcStatusChange1(sender, e);
                }));
                return;
            }

            if (e.Error)
            {
                toolStripStatusLabel1.BackColor = Color.Red;
            }
            else
            {
                toolStripStatusLabel1.BackColor = SystemColors.Control;
            }

            toolStripStatusLabel_opc.Text = e.ToString();
        }


        private ReferenceDescriptionCollection GetReferenceDescriptionCollection(NodeId sourceId)
        {
            TaskCompletionSource<ReferenceDescriptionCollection> task = new TaskCompletionSource<ReferenceDescriptionCollection>();

            // find all of the components of the node.
            BrowseDescription nodeToBrowse1 = new BrowseDescription();

            nodeToBrowse1.NodeId = sourceId;
            nodeToBrowse1.BrowseDirection = BrowseDirection.Forward;
            nodeToBrowse1.ReferenceTypeId = ReferenceTypeIds.Aggregates;
            nodeToBrowse1.IncludeSubtypes = true;
            nodeToBrowse1.NodeClassMask = (uint)(NodeClass.Object | NodeClass.Variable | NodeClass.Method | NodeClass.ReferenceType | NodeClass.ObjectType | NodeClass.View | NodeClass.VariableType | NodeClass.DataType);
            nodeToBrowse1.ResultMask = (uint)BrowseResultMask.All;

            // find all nodes organized by the node.
            BrowseDescription nodeToBrowse2 = new BrowseDescription();

            nodeToBrowse2.NodeId = sourceId;
            nodeToBrowse2.BrowseDirection = BrowseDirection.Forward;
            nodeToBrowse2.ReferenceTypeId = ReferenceTypeIds.Organizes;
            nodeToBrowse2.IncludeSubtypes = true;
            nodeToBrowse2.NodeClassMask = (uint)(NodeClass.Object | NodeClass.Variable | NodeClass.Method | NodeClass.View | NodeClass.ReferenceType | NodeClass.ObjectType | NodeClass.VariableType | NodeClass.DataType);
            nodeToBrowse2.ResultMask = (uint)BrowseResultMask.All;

            BrowseDescriptionCollection nodesToBrowse = new BrowseDescriptionCollection();
            nodesToBrowse.Add(nodeToBrowse1);
            nodesToBrowse.Add(nodeToBrowse2);

            // fetch references from the server.
            ReferenceDescriptionCollection references = FormUtils.Browse(m_OpcUaClient.Session, nodesToBrowse, false);
            return references;
        }

        /// <summary>
        /// 0:NodeClass  1:Value  2:AccessLevel  3:DisplayName  4:Description
        /// </summary>
        /// <param name="nodeIds"></param>
        /// <returns></returns>
        private DataValue[] ReadOneNodeFiveAttributes(List<NodeId> nodeIds)
        {
            ReadValueIdCollection nodesToRead = new ReadValueIdCollection();
            foreach (var nodeId in nodeIds)
            {
                NodeId sourceId = nodeId;
                nodesToRead.Add(new ReadValueId()
                {
                    NodeId = sourceId,
                    AttributeId = Attributes.NodeClass
                });
                nodesToRead.Add(new ReadValueId()
                {
                    NodeId = sourceId,
                    AttributeId = Attributes.Value
                });
                nodesToRead.Add(new ReadValueId()
                {
                    NodeId = sourceId,
                    AttributeId = Attributes.AccessLevel
                });
                nodesToRead.Add(new ReadValueId()
                {
                    NodeId = sourceId,
                    AttributeId = Attributes.DisplayName
                });
                nodesToRead.Add(new ReadValueId()
                {
                    NodeId = sourceId,
                    AttributeId = Attributes.Description
                });
            }

            // read all values.
            m_OpcUaClient.Session.Read(
                null,
                0,
                TimestampsToReturn.Neither,
                nodesToRead,
                out DataValueCollection results,
                out DiagnosticInfoCollection diagnosticInfos);

            ClientBase.ValidateResponse(results, nodesToRead);
            ClientBase.ValidateDiagnosticInfos(diagnosticInfos, nodesToRead);

            return results.ToArray();
        }


        /// <summary>
        /// 读取一个节点的指定属性
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        private DataValue ReadNoteDataValueAttributes(NodeId nodeId, uint attribute)
        {
            NodeId sourceId = nodeId;
            ReadValueIdCollection nodesToRead = new ReadValueIdCollection();


            ReadValueId nodeToRead = new ReadValueId();
            nodeToRead.NodeId = sourceId;
            nodeToRead.AttributeId = attribute;
            nodesToRead.Add(nodeToRead);

            int startOfProperties = nodesToRead.Count;

            // find all of the pror of the node.
            BrowseDescription nodeToBrowse1 = new BrowseDescription();

            nodeToBrowse1.NodeId = sourceId;
            nodeToBrowse1.BrowseDirection = BrowseDirection.Forward;
            nodeToBrowse1.ReferenceTypeId = ReferenceTypeIds.HasProperty;
            nodeToBrowse1.IncludeSubtypes = true;
            nodeToBrowse1.NodeClassMask = 0;
            nodeToBrowse1.ResultMask = (uint)BrowseResultMask.All;

            BrowseDescriptionCollection nodesToBrowse = new BrowseDescriptionCollection();
            nodesToBrowse.Add(nodeToBrowse1);

            // fetch property references from the server.
            ReferenceDescriptionCollection references = FormUtils.Browse(m_OpcUaClient.Session, nodesToBrowse, false);

            if (references == null)
            {
                return null;
            }

            for (int ii = 0; ii < references.Count; ii++)
            {
                // ignore external references.
                if (references[ii].NodeId.IsAbsolute)
                {
                    continue;
                }

                ReadValueId nodeToRead2 = new ReadValueId();
                nodeToRead2.NodeId = (NodeId)references[ii].NodeId;
                nodeToRead2.AttributeId = Attributes.Value;
                nodesToRead.Add(nodeToRead2);
            }

            // read all values.
            DataValueCollection results = null;
            DiagnosticInfoCollection diagnosticInfos = null;

            m_OpcUaClient.Session.Read(
                null,
                0,
                TimestampsToReturn.Neither,
                nodesToRead,
                out results,
                out diagnosticInfos);

            ClientBase.ValidateResponse(results, nodesToRead);
            ClientBase.ValidateDiagnosticInfos(diagnosticInfos, nodesToRead);

            return results[0];
        }

        #endregion

        #region Menu Click Event

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // press exit menu button
            Close();
        }

        private void discoverToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string endpointUrl = new DiscoverServerDlg().ShowDialog(m_OpcUaClient.AppConfig, null);

            if (endpointUrl != null)
            {
                textBox1.Text = endpointUrl;
            }
        }



        #endregion

        #region Press Connect Click Button

        private async void button1_Click( object sender, EventArgs e )
        {
            // connect to server
            using (FormConnectSelect formConnectSelect = new FormConnectSelect( m_OpcUaClient ))
            {
                if (formConnectSelect.ShowDialog( ) == DialogResult.OK)
                {
                    try
                    {
                        await m_OpcUaClient.ConnectServer( textBox1.Text );
                        button1.BackColor = Color.LimeGreen;
                    }
                    catch (Exception ex)
                    {
                        ClientUtils.HandleException( Text, ex );
                    }
                }
            }
        }

        #endregion

        #region 填入分支
        
        /// <summary>
        /// Populates the branch in the tree view.
        /// </summary>
        /// <param name="sourceId">The NodeId of the Node to browse.</param>
        /// <param name="nodes">The node collect to populate.</param>
        private async void PopulateBranch(NodeId sourceId, TreeNodeCollection nodes)
        {
            nodes.Clear();
            nodes.Add(new TreeNode("Browsering...", 7, 7));
            // fetch references from the server.
            TreeNode[] listNode = await Task.Run(() =>
             {
                 ReferenceDescriptionCollection references = GetReferenceDescriptionCollection(sourceId);
                 List<TreeNode> list = new List<TreeNode>();
                 if (references != null)
                 {
                     // process results.
                     for (int ii = 0; ii < references.Count; ii++)
                     {
                         ReferenceDescription target = references[ii];
                         TreeNode child = new TreeNode(Utils.Format("{0}", target));

                         child.Tag = target;
                         string key = GetImageKeyFromDescription(target, sourceId);
                         child.ImageKey = key;
                         child.SelectedImageKey = key;

                         // if (target.NodeClass == NodeClass.Object || target.NodeClass == NodeClass.Unspecified || expanded)
                         // {
                         //     child.Nodes.Add(new TreeNode());
                         // }

                         if (!checkBox1.Checked)
                         {
                             if (GetReferenceDescriptionCollection( (NodeId)target.NodeId ).Count > 0)
                             {
                                child.Nodes.Add( new TreeNode( ) );
                             }
                         }
                         else
                         {
                             child.Nodes.Add( new TreeNode( ) );
                         }
                         

                         list.Add(child);
                     }
                 }

                 return list.ToArray();
             });

            
            // update the attributes display.
            // DisplayAttributes(sourceId);
            nodes.Clear();
            nodes.AddRange(listNode.ToArray());
        }
    


        #endregion

        #region 节点打开的时候操作

        private void BrowseNodesTV_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                TreeNode tn = BrowseNodesTV.GetNodeAt(e.X, e.Y);
                if (tn != null)
                {
                    BrowseNodesTV.SelectedNode = tn;
                }
            }
        }

        private void BrowseNodesTV_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            try
            {

                // check if node has already been expanded once.
                if (e.Node.Nodes.Count != 1)
                {
                    return;
                }

                if (e.Node.Nodes.Count > 0)
                {
                    if (e.Node.Nodes[0].Text != String.Empty)
                    {
                        return;
                    }
                }

                // get the source for the node.
                ReferenceDescription reference = e.Node.Tag as ReferenceDescription;

                if (reference == null || reference.NodeId.IsAbsolute)
                {
                    e.Cancel = true;
                    return;
                }

                // populate children.
                PopulateBranch((NodeId)reference.NodeId, e.Node.Nodes);
            }
            catch (Exception exception)
            {
                ClientUtils.HandleException(this.Text, exception);
            }
        }



        private void BrowseNodesTV_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                RemoveAllSubscript( );
                // get the source for the node.
                ReferenceDescription reference = e.Node.Tag as ReferenceDescription;

                if (reference == null || reference.NodeId.IsAbsolute)
                {
                    return;
                }

                // populate children.
                ShowMember((NodeId)reference.NodeId);
            }
            catch (Exception exception)
            {
                ClientUtils.HandleException(Text, exception);
            }
        }


        private void ClearDataGridViewRows(int index)
        {
            for (int i = dataGridView1.Rows.Count - 1; i >= index; i--)
            {
                if (i >= 0)
                {
                    dataGridView1.Rows.RemoveAt(i);
                }
            }
        }

        #endregion

        #region 点击树节点后在数据表显示

        /// <summary>
        /// 点击了节点名称前的内容进行复制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void label2_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox_nodeId.Text))
            {
                Clipboard.SetText(textBox_nodeId.Text);
            }
        }
        private async void ShowMember(NodeId sourceId)
        {

            textBox_nodeId.Text = sourceId.ToString();

            // dataGridView1.Rows.Clear();
            int index = 0;
            ReferenceDescriptionCollection references;
            try
            {
                references = await Task.Run(() =>
                 {
                     return GetReferenceDescriptionCollection(sourceId);
                 });
            }
            catch(Exception exception)
            {
                ClientUtils.HandleException(Text, exception);
                return;
            }


            if (references?.Count > 0)
            {
                // 获取所有要读取的子节点
                List<NodeId> nodeIds = new List<NodeId>();
                for (int ii = 0; ii < references.Count; ii++)
                {
                    ReferenceDescription target = references[ii];
                    nodeIds.Add((NodeId)target.NodeId);
                }

                DateTime dateTimeStart = DateTime.Now;

                // 获取所有的值
                DataValue[] dataValues = await Task.Run(() =>
                {
                  return ReadOneNodeFiveAttributes(nodeIds);
                });
                
                label_time_spend.Text = (int)(DateTime.Now - dateTimeStart).TotalMilliseconds + " ms";
                
                // 显示
                for (int jj = 0; jj < dataValues.Length; jj += 5)
                {
                    AddDataGridViewNewRow(dataValues, jj, index++, nodeIds[jj / 5]);
                }
                
            }
            else
            {
                // 子节点没有数据的情况
                try
                {
                    DateTime dateTimeStart = DateTime.Now;
                    DataValue dataValue = m_OpcUaClient.ReadNode(sourceId);

                    if (dataValue.WrappedValue.TypeInfo?.ValueRank == ValueRanks.OneDimension)
                    {
                        // 数组显示
                        AddDataGridViewArrayRow(sourceId, out index);
                    }
                    else
                    {
                        // 显示单个数本身
                        label_time_spend.Text = (int)(DateTime.Now - dateTimeStart).TotalMilliseconds + " ms";
                        AddDataGridViewNewRow(ReadOneNodeFiveAttributes(new List<NodeId>() { sourceId }), 0, index++, sourceId);
                    }
                }
                catch (Exception exception)
                {
                    ClientUtils.HandleException(Text, exception);
                    return;
                }
            }

            ClearDataGridViewRows(index);

        }


        private void AddDataGridViewNewRow(DataValue[] dataValues, int startIndex, int index, NodeId nodeId)
        {
            // int index = dataGridView1.Rows.Add();
            while (index >= dataGridView1.Rows.Count)
            {
                dataGridView1.Rows.Add();
            }
            DataGridViewRow dgvr = dataGridView1.Rows[index];
            dgvr.Tag = nodeId;

            if (dataValues[startIndex].WrappedValue.Value == null) return;
            NodeClass nodeclass = (NodeClass)dataValues[startIndex].WrappedValue.Value;

            dgvr.Cells[1].Value = dataValues[3 + startIndex].WrappedValue.Value;
            dgvr.Cells[5].Value = dataValues[4 + startIndex].WrappedValue.Value;
            dgvr.Cells[4].Value = GetDiscriptionFromAccessLevel(dataValues[2 + startIndex]);

            if (nodeclass == NodeClass.Object)
            {
                dgvr.Cells[0].Value = Properties.Resources.ClassIcon;
                dgvr.Cells[2].Value = "";
                dgvr.Cells[3].Value = nodeclass.ToString();
            }
            else if (nodeclass == NodeClass.Method)
            {
                dgvr.Cells[0].Value = Properties.Resources.Method_636;
                dgvr.Cells[2].Value = "";
                dgvr.Cells[3].Value = nodeclass.ToString();
            }
            else if (nodeclass == NodeClass.Variable)
            {
                DataValue dataValue = dataValues[1 + startIndex];

                if (dataValue.WrappedValue.TypeInfo != null)
                {
                    dgvr.Cells[3].Value = dataValue.WrappedValue.TypeInfo.BuiltInType;
                    dgvr.Cells[6].Value = dataValue.StatusCode.ToString();
                    // dgvr.Cells[3].Value = dataValue.Value.GetType().ToString();
                    if (dataValue.WrappedValue.TypeInfo.ValueRank == ValueRanks.Scalar)
                    {
                        dgvr.Cells[2].Value = dataValue.WrappedValue.Value;
                        dgvr.Cells[0].Value = Properties.Resources.Enum_582;
                    }
                    else if (dataValue.WrappedValue.TypeInfo.ValueRank == ValueRanks.OneDimension)
                    {
                        dgvr.Cells[2].Value = dataValue.Value.GetType().ToString();
                        dgvr.Cells[0].Value = Properties.Resources.brackets_Square_16xMD;
                    }
                    else if (dataValue.WrappedValue.TypeInfo.ValueRank == ValueRanks.TwoDimensions)
                    {
                        dgvr.Cells[2].Value = dataValue.Value.GetType().ToString();
                        dgvr.Cells[0].Value = Properties.Resources.Module_648;
                    }
                    else
                    {
                        dgvr.Cells[2].Value = dataValue.Value.GetType().ToString();
                        dgvr.Cells[0].Value = Properties.Resources.ClassIcon;
                    }
                }
                else
                {
                    dgvr.Cells[0].Value = Properties.Resources.ClassIcon;
                    dgvr.Cells[2].Value = dataValue.Value;
                    dgvr.Cells[3].Value = "null";
                }
            }
            else
            {
                dgvr.Cells[2].Value = "";
                dgvr.Cells[0].Value = Properties.Resources.ClassIcon;
                dgvr.Cells[3].Value = nodeclass.ToString();
                dgvr.Cells[6].Value = "";
            }
        }

        private void AddDataGridViewArrayRow(NodeId nodeId,out int index)
        {
            
            DateTime dateTimeStart = DateTime.Now;
            DataValue[] dataValues = ReadOneNodeFiveAttributes(new List<NodeId>() { nodeId });
            label_time_spend.Text = (int)(DateTime.Now - dateTimeStart).TotalMilliseconds + " ms";

            DataValue dataValue = dataValues[1];

            if (dataValue.WrappedValue.TypeInfo?.ValueRank == ValueRanks.OneDimension)
            {
                string access= GetDiscriptionFromAccessLevel(dataValues[2]);
                BuiltInType type = dataValue.WrappedValue.TypeInfo.BuiltInType;
                object des = dataValues[4].Value ?? "";
                object dis = dataValues[3].Value ?? type;

                Array array = dataValue.Value as Array;
                int i = 0;
                foreach(object obj in array)
                {
                    while (i >= dataGridView1.Rows.Count)
                    {
                        dataGridView1.Rows.Add();
                    }
                    
                    DataGridViewRow dgvr = dataGridView1.Rows[i];

                    dgvr.Tag = null;

                    dgvr.Cells[0].Value = Properties.Resources.Enum_582;
                    dgvr.Cells[1].Value = $"{dis} [{i++}]";
                    dgvr.Cells[2].Value = obj;
                    dgvr.Cells[3].Value = type;
                    dgvr.Cells[4].Value = access;
                    dgvr.Cells[5].Value = des;
                }
                index = i;
            }
            else
            {
                index = 0;
            }
        }

        private string GetDiscriptionFromAccessLevel(DataValue value)
        {
            if (value.WrappedValue.Value != null)
            {
                switch ((byte)value.WrappedValue.Value)
                {
                    case 0: return "None";
                    case 1: return "CurrentRead";
                    case 2: return "CurrentWrite";
                    case 3: return "CurrentReadOrWrite";
                    case 4: return "HistoryRead";
                    case 8: return "HistoryWrite";
                    case 12: return "HistoryReadOrWrite";
                    case 16: return "SemanticChange";
                    case 32: return "StatusWrite";
                    case 64: return "TimestampWrite";
                    default: return "None";
                }
            }
            else
            {
                return "null";
            }
        }



        #endregion

        #region 订阅刷新块


        private List<string> subNodeIds = new List<string>( );
        private bool isSingleValueSub = false;

        private void RemoveAllSubscript( )
        {
            m_OpcUaClient?.RemoveAllSubscription( );
        }


        private void SubCallBack( string key, MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs eventArgs )
        {
            if (InvokeRequired)
            {
                Invoke( new Action<string, MonitoredItem, MonitoredItemNotificationEventArgs>( SubCallBack ), key, monitoredItem, eventArgs );
                return;
            }


            MonitoredItemNotification notification = eventArgs.NotificationValue as MonitoredItemNotification;
            string nodeId = monitoredItem.StartNodeId.ToString( );

            int index = subNodeIds.IndexOf( nodeId );
            if (index >= 0)
            {
                if (isSingleValueSub)
                {
                    if (notification.Value.WrappedValue.TypeInfo?.ValueRank == ValueRanks.OneDimension)
                    {
                        Array array = notification.Value.WrappedValue.Value as Array;
                        int i = 0;
                        foreach (object obj in array)
                        {
                            DataGridViewRow dgvr = dataGridView1.Rows[i];
                            dgvr.Cells[2].Value = obj;
                            i++;
                        }
                    }
                    else
                    {
                        dataGridView1.Rows[index].Cells[2].Value = notification.Value.WrappedValue.Value;
                    }
                }
                else
                {
                    dataGridView1.Rows[index].Cells[2].Value = notification.Value.WrappedValue.Value;
                }
            }
        }


        private async void button2_Click(object sender, EventArgs e)
        {
            if (m_OpcUaClient != null)
            {
                RemoveAllSubscript( );
                if (button2.BackColor != Color.LimeGreen)
                {
                    button2.BackColor = Color.LimeGreen;
                    // 判断当前的选择
                    if (string.IsNullOrEmpty( textBox_nodeId.Text )) return;

                    
                    ReferenceDescriptionCollection references;
                    try
                    {
                        references = await Task.Run( ( ) =>
                        {
                            return GetReferenceDescriptionCollection( new NodeId( textBox_nodeId.Text ) );
                        } );
                    }
                    catch (Exception exception)
                    {
                        ClientUtils.HandleException( Text, exception );
                        return;
                    }

                    subNodeIds = new List<string>( );
                    if (references?.Count > 0)
                    {
                        isSingleValueSub = false;
                        // 获取所有要订阅的子节点
                        for (int ii = 0; ii < references.Count; ii++)
                        {
                            ReferenceDescription target = references[ii];
                            subNodeIds.Add( ((NodeId)target.NodeId).ToString( ) );
                        }
                    }
                    else
                    {
                        isSingleValueSub = true;
                        // 子节点没有数据的情况
                        subNodeIds.Add( textBox_nodeId.Text );
                    }

                    m_OpcUaClient.AddSubscription( "subTest", subNodeIds.ToArray( ), SubCallBack );
                }
                else
                {
                    button2.BackColor = SystemColors.Control;
                }
            }
        }

        #endregion

        #region 点击了表格修改数据

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex + 1].Value is BuiltInType builtInType)
            {
                dynamic value = null;
                if (dataGridView1.Rows[e.RowIndex].Tag is NodeId nodeId)
                {
                    // 节点
                    try
                    {
                        value = GetValueFromString(dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString(), builtInType);
                    }
                    catch
                    {
                        MessageBox.Show("Invalid Input Value");
                        return;
                    }

                    if (!m_OpcUaClient.WriteNode(nodeId.ToString(), value))
                    {
                        MessageBox.Show("Failed to write value");
                    }
                }
                else
                {
                    // 点击了数组修改
                    IList<string> list = new List<string>();

                    for (int jj = 0; jj < dataGridView1.RowCount; jj++)
                    {
                        list.Add(dataGridView1.Rows[jj].Cells[e.ColumnIndex].Value.ToString());
                    }
                    
                    try
                    {
                        value = GetArrayValueFromString(list, builtInType);
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show("Invalid Input Value: " + ex.Message);
                        return;
                    }

                    if (!m_OpcUaClient.WriteNode(textBox_nodeId.Text, value))
                    {
                        MessageBox.Show("Failed to write value");
                    }
                }
            }
            else
            {
                MessageBox.Show("Invalid data type");
            }

            //MessageBox.Show(
            //    "Type:" + dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.GetType().ToString() + Environment.NewLine +
            //    "Value:" + dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString());
        }
        
        private dynamic GetValueFromString(string value, BuiltInType builtInType)
        {
            switch(builtInType)
            {
                case BuiltInType.Boolean:
                    {
                        return bool.Parse(value);
                    }
                case BuiltInType.Byte:
                    {
                        return byte.Parse(value);
                    }
                case BuiltInType.DateTime:
                    {
                        return DateTime.Parse(value);
                    }
                case BuiltInType.Double:
                    {
                        return double.Parse(value);
                    }
                case BuiltInType.Float:
                    {
                        return float.Parse(value);
                    }
                case BuiltInType.Guid:
                    {
                        return Guid.Parse(value);
                    }
                case BuiltInType.Int16:
                    {
                        return short.Parse(value);
                    }
                case BuiltInType.Int32:
                    {
                        return int.Parse(value);
                    }
                case BuiltInType.Int64:
                    {
                        return long.Parse(value);
                    }
                case BuiltInType.Integer:
                    {
                        return int.Parse(value);
                    }
                case BuiltInType.LocalizedText:
                    {
                        return value;
                    }
                case BuiltInType.SByte:
                    {
                        return sbyte.Parse(value);
                    }
                case BuiltInType.String:
                    {
                        return value;
                    }
                case BuiltInType.UInt16:
                    {
                        return ushort.Parse(value);
                    }
                case BuiltInType.UInt32:
                    {
                        return uint.Parse(value);
                    }
                case BuiltInType.UInt64:
                    {
                        return ulong.Parse(value);
                    }
                case BuiltInType.UInteger:
                    {
                        return uint.Parse(value);
                    }
                default:throw new Exception("Not supported data type");
            }
        }
        

        private dynamic GetArrayValueFromString(IList<string> values, BuiltInType builtInType)
        {
            switch (builtInType)
            {
                case BuiltInType.Boolean:
                    {
                        bool[] result = new bool[values.Count];
                        for (int i = 0; i < values.Count; i++)
                        {
                            result[i] = bool.Parse(values[i]);
                        }
                        return result;
                    }
                case BuiltInType.Byte:
                    {
                        byte[] result = new byte[values.Count];
                        for (int i = 0; i < values.Count; i++)
                        {
                            result[i] = byte.Parse(values[i]);
                        }
                        return result;
                    }
                case BuiltInType.DateTime:
                    {
                        DateTime[] result = new DateTime[values.Count];
                        for (int i = 0; i < values.Count; i++)
                        {
                            result[i] = DateTime.Parse(values[i]);
                        }
                        return result;
                    }
                case BuiltInType.Double:
                    {
                        double[] result = new double[values.Count];
                        for (int i = 0; i < values.Count; i++)
                        {
                            result[i] = double.Parse(values[i]);
                        }
                        return result;
                    }
                case BuiltInType.Float:
                    {
                        float[] result = new float[values.Count];
                        for (int i = 0; i < values.Count; i++)
                        {
                            result[i] = float.Parse(values[i]);
                        }
                        return result;
                    }
                case BuiltInType.Guid:
                    {
                        Guid[] result = new Guid[values.Count];
                        for (int i = 0; i < values.Count; i++)
                        {
                            result[i] = Guid.Parse(values[i]);
                        }
                        return result;
                    }
                case BuiltInType.Int16:
                    {
                        short[] result = new short[values.Count];
                        for (int i = 0; i < values.Count; i++)
                        {
                            result[i] = short.Parse(values[i]);
                        }
                        return result;
                    }
                case BuiltInType.Int32:
                    {
                        int[] result = new int[values.Count];
                        for (int i = 0; i < values.Count; i++)
                        {
                            result[i] = int.Parse(values[i]);
                        }
                        return result;
                    }
                case BuiltInType.Int64:
                    {
                        long[] result = new long[values.Count];
                        for (int i = 0; i < values.Count; i++)
                        {
                            result[i] = long.Parse(values[i]);
                        }
                        return result;
                    }
                case BuiltInType.Integer:
                    {
                        int[] result = new int[values.Count];
                        for (int i = 0; i < values.Count; i++)
                        {
                            result[i] = int.Parse(values[i]);
                        }
                        return result;
                    }
                case BuiltInType.LocalizedText:
                    {
                        return values.ToArray();
                    }
                case BuiltInType.SByte:
                    {
                        sbyte[] result = new sbyte[values.Count];
                        for (int i = 0; i < values.Count; i++)
                        {
                            result[i] = sbyte.Parse(values[i]);
                        }
                        return result;
                    }
                case BuiltInType.String:
                    {
                        return values.ToArray();
                    }
                case BuiltInType.UInt16:
                    {
                        ushort[] result = new ushort[values.Count];
                        for (int i = 0; i < values.Count; i++)
                        {
                            result[i] = ushort.Parse(values[i]);
                        }
                        return result;
                    }
                case BuiltInType.UInt32:
                    {
                        uint[] result = new uint[values.Count];
                        for (int i = 0; i < values.Count; i++)
                        {
                            result[i] = uint.Parse(values[i]);
                        }
                        return result;
                    }
                case BuiltInType.UInt64:
                    {
                        ulong[] result = new ulong[values.Count];
                        for (int i = 0; i < values.Count; i++)
                        {
                            result[i] = ulong.Parse(values[i]);
                        }
                        return result;
                    }
                case BuiltInType.UInteger:
                    {
                        uint[] result = new uint[values.Count];
                        for (int i = 0; i < values.Count; i++)
                        {
                            result[i] = uint.Parse(values[i]);
                        }
                        return result;
                    }
                default: throw new Exception("Not supported data type");
            }
        }

        private void dataGridView1_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex + 1].Value is BuiltInType builtInType)
            {
                if (
                    builtInType == BuiltInType.Boolean ||
                    builtInType == BuiltInType.Byte ||
                    builtInType == BuiltInType.DateTime ||
                    builtInType == BuiltInType.Double ||
                    builtInType == BuiltInType.Float ||
                    builtInType == BuiltInType.Guid ||
                    builtInType == BuiltInType.Int16 ||
                    builtInType == BuiltInType.Int32 ||
                    builtInType == BuiltInType.Int64 ||
                    builtInType == BuiltInType.Integer ||
                    builtInType == BuiltInType.LocalizedText ||
                    builtInType == BuiltInType.SByte ||
                    builtInType == BuiltInType.String ||
                    builtInType == BuiltInType.UInt16 ||
                    builtInType == BuiltInType.UInt32 ||
                    builtInType == BuiltInType.UInt64 ||
                    builtInType == BuiltInType.UInteger
                    )
                {

                }
                else
                {
                    e.Cancel = true;
                    MessageBox.Show("Not support the Type of modify value!");
                    return;
                }
            }
            else
            {
                e.Cancel = true;
                MessageBox.Show("Not support the Type of modify value!");
                return;
            }


            if (!dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex + 2].Value.ToString().Contains("Write"))
            {
                e.Cancel = true;
                MessageBox.Show("Not support the access of modify value!");
            }
        }



        #endregion
    }
}
