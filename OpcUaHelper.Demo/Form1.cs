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

        private async void Form1_Load( object sender, EventArgs e )
        {
            m_OpcUaClient = new OpcUaClient( );
            try
            {
                await m_OpcUaClient.ConnectServer( "opc.tcp://127.0.0.1:62541/SharpNodeSettings/OpcUaServer" );
            }
            catch(Exception ex)
            {
                MessageBox.Show( ex.Message );
            }
        }



        private OpcUaClient m_OpcUaClient;

        private void button1_Click( object sender, EventArgs e )
        {
            test6( );
            DataValue dataValue = m_OpcUaClient.ReadNode( new NodeId( textBox1.Text ) );
            textBox2.Text = dataValue.WrappedValue.Value.ToString( );
        }




        private void Form1_FormClosing( object sender, FormClosingEventArgs e )
        {
            m_OpcUaClient.Disconnect( );
        }

        private void button2_Click( object sender, EventArgs e )
        {
            // sub
            m_OpcUaClient.AddSubscription( "A", textBox4.Text, SubCallback );
        }

        private void button3_Click( object sender, EventArgs e )
        {
            // remove sub
            m_OpcUaClient.RemoveSubscription( "A" );
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

        // 缓存的批量订阅的节点
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
            m_OpcUaClient.AddSubscription( "B", MonitorNodeTags, SubCallback );
        }

        private void button4_Click( object sender, EventArgs e )
        {
            // 取消多个节点的订阅
            m_OpcUaClient.RemoveSubscription( "B" );
        }


        private void test( )
        {
            OpcUaHelper.Forms.FormBrowseServer formBrowseServer = new Forms.FormBrowseServer( "opc.tcp://127.0.0.1:62541/SharpNodeSettings/OpcUaServer" );
            formBrowseServer.ShowDialog( );
        }


        private void test1( )
        {
            try
            {
                short value = m_OpcUaClient.ReadNode<short>( "ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/温度" );
            }
            catch(Exception ex)
            {
                ClientUtils.HandleException( this.Text, ex );
            }
        }

        private async void test2( )
        {
            try
            {
                short value = await m_OpcUaClient.ReadNodeAsync<short>( "ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/温度" );
            }
            catch (Exception ex)
            {
                ClientUtils.HandleException( this.Text, ex );
            }
        }

        private void test3( )
        {
            try
            {
                m_OpcUaClient.WriteNode( "ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/温度", (short)123 );
            }
            catch (Exception ex)
            {
                ClientUtils.HandleException( this.Text, ex );
            }
        }

        private void test4( )
        {
            try
            {
                // 添加所有的读取的节点，此处的示例是类型不一致的情况
                List<NodeId> nodeIds = new List<NodeId>( );
                nodeIds.Add( new NodeId( "ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/温度" ) );
                nodeIds.Add( new NodeId( "ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/风俗" ) );
                nodeIds.Add( new NodeId( "ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/转速" ) );
                nodeIds.Add( new NodeId( "ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/机器人关节" ) );
                nodeIds.Add( new NodeId( "ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/cvsdf" ) );
                nodeIds.Add( new NodeId( "ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/条码" ) );
                nodeIds.Add( new NodeId( "ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/开关量" ) );

                // dataValues按顺序定义的值，每个值里面需要重新判断类型
                List<DataValue> dataValues = m_OpcUaClient.ReadNodes( nodeIds.ToArray() );
                // 然后遍历你的数据信息
                foreach (var dataValue in dataValues)
                {
                    // 获取你的实际的数据
                    object value = dataValue.WrappedValue.Value;
                }




                // 如果你批量读取的值的类型都是一样的，比如float，那么有简便的方式
                List<string> tags = new List<string>( );
                tags.Add( "ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/风俗" );
                tags.Add( "ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/转速" );

                // 按照顺序定义的值
                List<float> values = m_OpcUaClient.ReadNodes<float>( tags.ToArray() );

            }
            catch (Exception ex)
            {
                ClientUtils.HandleException( this.Text, ex );
            }


            try
            {
                // 此处演示写入一个short，2个float类型的数据批量写入操作
                bool success = m_OpcUaClient.WriteNodes( new string[] {
                    "ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/温度",
                    "ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/风俗",
                    "ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/转速"},
                    new object[] {
                        (short)1234,
                        123.456f,
                        123f
                    } );
                if (success)
                {
                    // 写入成功
                }
                else
                {
                    // 写入失败，一个失败即为失败
                }
            }
            catch (Exception ex)
            {
                ClientUtils.HandleException( this.Text, ex );
            }


            try
            {
                // 此处演示读取历史数据的操作，读取8月18日12点到13点的数据，如果想要读取成功，该节点是支持历史记录的
                List<float> values = m_OpcUaClient.ReadHistoryRawDataValues<float>( "ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/转速",
                    new DateTime( 2018, 8, 18, 12, 0, 0 ), new DateTime( 2018, 8, 18, 13, 0, 0 ) ).ToList( );
                
            }
            catch (Exception ex)
            {
                ClientUtils.HandleException( this.Text, ex );
            }


        }

        public void test5( )
        {

            try
            {
                OpcNodeAttribute[] nodeAttributes = m_OpcUaClient.ReadNoteAttributes( "ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/温度" );
                foreach (var item in nodeAttributes)
                {
                    Console.Write( string.Format( "{0,-30}", item.Name ) );
                    Console.Write( string.Format( "{0,-20}", item.Type ) );
                    Console.Write( string.Format( "{0,-20}", item.StatusCode ) );
                    Console.WriteLine( string.Format( "{0,20}", item.Value ) );
                }
                
                // 输出如下
                //  Name                          Type                StatusCode                         Vlaue

                //  NodeClass                     Int32               Good                                   2
                //  BrowseName                    QualifiedName       Good                              2:温度
                //  DisplayName                   LocalizedText       Good                                温度
                //  Description                   LocalizedText       Good                                    
                //  WriteMask                     UInt32              Good                                  96
                //  UserWriteMask                 UInt32              Good                                  96
                //  Value                         Int16               Good                              -11980
                //  DataType                      NodeId              Good                                 i=4
                //  ValueRank                     Int32               Good                                  -1
                //  ArrayDimensions               Null                Good                                    
                //  AccessLevel                   Byte                Good                                   3
                //  UserAccessLevel               Byte                Good                                   3
                //  MinimumSamplingInterval       Double              Good                                   0
                //  Historizing                   Boolean             Good                               False
            }
            catch (Exception ex)
            {
                ClientUtils.HandleException( this.Text, ex );
            }
        }

        public void test6( )
        {

            try
            {
                ReferenceDescription[] references = m_OpcUaClient.BrowseNodeReference( "ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端" );
                foreach (var item in references)
                {
                    Console.Write( string.Format( "{0,-30}", item.NodeClass ) );
                    Console.Write( string.Format( "{0,-30}", item.BrowseName ) );
                    Console.Write( string.Format( "{0,-20}", item.DisplayName ) );
                    Console.WriteLine( string.Format( "{0,-20}", item.NodeId.ToString( ) ) );
                }

                ;
                // 输出如下
                //  NodeClass                     BrowseName                      DisplayName           NodeId

                //  Variable                      2:温度                          温度                  ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/温度
                //  Variable                      2:风俗                          风俗                  ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/风俗
                //  Variable                      2:转速                          转速                  ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/转速
                //  Variable                      2:机器人关节                    机器人关节            ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/机器人关节
                //  Variable                      2:cvsdf                         cvsdf                 ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/cvsdf
                //  Variable                      2:条码                          条码                  ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/条码
                //  Variable                      2:开关量                        开关量                ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/开关量
            }
            catch (Exception ex)
            {
                ClientUtils.HandleException( this.Text, ex );
            }
        }
    }
}
