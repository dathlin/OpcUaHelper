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
                opcUaClient.ConnectServer( "opc.tcp://118.24.36.220:62547/DataAccessServer" );
            }
            catch(Exception ex)
            {
                Console.WriteLine( "Connect failed : " + ex.Message );
                return;
            }

            float value = opcUaClient.ReadNode<float>( "ns=2;s=Machines/Machine B/TestValueFloat" );
            Console.WriteLine( "ns=2;s=Machines/Machine B/TestValueFloat" + "   value: " + value );

            string name = opcUaClient.ReadNode<string>( "ns=2;s=Machines/Machine A/Name" );
            Console.WriteLine( "ns=2;s=Machines/Machine A/Name" + "   value: " + name );

            Console.WriteLine("Please enter any key to quit!");
            opcUaClient.Disconnect( );
            Console.ReadLine( );
        }
    }
}
