using System;
using OpcUaHelper;


/***************************************************************************************************************
 * 
 * 
 * 
 * 
 * 
 * 
 *****************************************************************************************************************/
 

namespace OpcUaHelper.NetCoreDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            OpcUaClient opcUaClient = new OpcUaClient( );

            try
            {
                opcUaClient.ConnectServer( "opc.tcp://127.0.0.1:62541/SharpNodeSettings/OpcUaServer" );
            }
            catch(Exception ex)
            {
                Console.WriteLine( "Connect failed : " + ex.Message );
                return;
            }

            int value = opcUaClient.ReadNode<int>( "ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/温度" );
            Console.WriteLine( "ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/温度" + "   value: " + value );

            Console.WriteLine("Please enter any key to quit!");
            Console.ReadLine( );
            opcUaClient.Disconnect( );
        }
    }
}
