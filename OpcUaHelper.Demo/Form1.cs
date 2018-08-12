using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Opc.Ua;
using Opc.Ua.Client;
using OpcUaHelper;

namespace OpcUaHelper.Demo
{
    public partial class Form1 : Form
    {
        public Form1( )
        {
            InitializeComponent( );
        }

        private void Form1_Load( object sender, EventArgs e )
        {
            opcUaClient = new OpcUaClient( );
            try
            {
                opcUaClient.ConnectServer( "opc.tcp://127.0.0.1:62541/SharpNodeSettings/OpcUaServer" );
            }
            catch(Exception ex)
            {
                MessageBox.Show( ex.Message );
            }
        }



        private OpcUaClient opcUaClient = new OpcUaClient( );

        private void button1_Click( object sender, EventArgs e )
        {
            DataValue dataValue = opcUaClient.ReadNode( new NodeId( textBox1.Text ) );
            textBox2.Text = dataValue.WrappedValue.Value.ToString( );
        }




        private void Form1_FormClosing( object sender, FormClosingEventArgs e )
        {
            opcUaClient.Disconnect( );
        }

        private void button2_Click( object sender, EventArgs e )
        {
            // sub
            opcUaClient.AddSubscription( "A", textBox4.Text, SubCallback );
        }

        private void button3_Click( object sender, EventArgs e )
        {
            // remove sub
            opcUaClient.RemoveSubscription( "A" );
        }

        private void SubCallback(string key, MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs args )
        {
            if (InvokeRequired)
            {
                Invoke( new Action<string, MonitoredItem, MonitoredItemNotificationEventArgs>( SubCallback ), key, monitoredItem, args );
                return;
            }

            if (key == "A")
            {
                // 如果有多个的订阅值都关联了当前的方法，可以通过key和monitoredItem来区分
                MonitoredItemNotification notification = args.NotificationValue as MonitoredItemNotification;
                if (notification != null)
                {
                    textBox3.Text = notification.Value.WrappedValue.Value.ToString( );
                }
            }
            else if(key == "B")
            {
                // 需要区分出来每个不同的节点信息
                MonitoredItemNotification notification = args.NotificationValue as MonitoredItemNotification;
                if (monitoredItem.StartNodeId.ToString( ) == MonitorNodeTags[0])
                {
                    textBox5.Text = notification.Value.WrappedValue.Value.ToString( );
                }
                else if (monitoredItem.StartNodeId.ToString( ) == MonitorNodeTags[1])
                {
                    textBox9.Text = notification.Value.WrappedValue.Value.ToString( );
                }
                else if (monitoredItem.StartNodeId.ToString( ) == MonitorNodeTags[2])
                {
                    textBox10.Text = notification.Value.WrappedValue.Value.ToString( );
                }
            }
        }

        private string[] MonitorNodeTags = null;

        private void button5_Click( object sender, EventArgs e )
        {
            // 多个节点的订阅
            MonitorNodeTags = new string[]
            {
                textBox6.Text,
                textBox7.Text,
                textBox8.Text,
            };
            opcUaClient.AddSubscription( "B", MonitorNodeTags, SubCallback );
        }

        private void button4_Click( object sender, EventArgs e )
        {
            // 取消多个节点的订阅
            opcUaClient.RemoveSubscription( "B" );
        }

    }
}
