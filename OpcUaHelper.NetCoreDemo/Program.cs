﻿using System;
using System.Threading;
using System.Threading.Tasks;
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
        static async Task Main(string[] args)
        {
            OpcUaClient opcUaClient = new OpcUaClient( );
            opcUaClient.OpcStatusChange += OpcUaClient_OpcStatusChange;

            try
            {
                await opcUaClient.ConnectServer( "opc.tcp://118.24.36.220:62548/DataAccessServer" );
            }
            catch (Exception ex)
            {
                Console.WriteLine( "Connect failed : " + ex.Message );
                return;
            }

            while (true)
            {
                Thread.Sleep( 1000 );
                try
                {
                    float value = opcUaClient.ReadNode<float>( "ns=2;s=Machines/Machine B/TestValueFloat" );
                    Console.WriteLine( "ns=2;s=Machines/Machine B/TestValueFloat" + "   value: " + value );

                    string name = opcUaClient.ReadNode<string>( "ns=2;s=Machines/Machine A/Name" );
                    Console.WriteLine( "ns=2;s=Machines/Machine A/Name" + "   value: " + name );
                }
                catch (Exception ex)
                {
                    Console.WriteLine( "read failed: " + ex.Message );
                }
            }
        }

        private static void OpcUaClient_OpcStatusChange( object sender, OpcUaStatusEventArgs e )
        {
            Console.WriteLine( "OpcUaClient_OpcStatusChange: " + e.Error + " Text:" + e.Text );
        }
    }
}
