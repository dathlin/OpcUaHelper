using Opc.Ua;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpcUaHelper.Forms
{
    public partial class FormConnectSelect : Form
    {
        public FormConnectSelect( OpcUaClient opcUaClient )
        {
            InitializeComponent( );
            this.m_OpcUaClient = opcUaClient;
        }

        private void FormConnectSelect_Load( object sender, EventArgs e )
        {

        }


        private OpcUaClient m_OpcUaClient;

        private void button1_Click( object sender, EventArgs e )
        {
            // 匿名登录
            m_OpcUaClient.UserIdentity = new UserIdentity( new AnonymousIdentityToken( ) );
            DialogResult = DialogResult.OK;
            return;
        }

        private void button2_Click( object sender, EventArgs e )
        {
            // 用户名密码登录
            m_OpcUaClient.UserIdentity = new UserIdentity( textBox1.Text, textBox2.Text );
            DialogResult = DialogResult.OK;
            return;
        }

        private void button3_Click( object sender, EventArgs e )
        {
            // 证书登录
            try
            {
                X509Certificate2 certificate = new X509Certificate2( textBox4.Text, textBox3.Text, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable );
                m_OpcUaClient.UserIdentity = new UserIdentity( certificate );
                DialogResult = DialogResult.OK;
                return;
            }
            catch(Exception ex)
            {
                ClientUtils.HandleException( this.Text, ex );
            }
        }

        private void button4_Click( object sender, EventArgs e )
        {

            using (OpenFileDialog openFileDialog = new OpenFileDialog( ))
            {
                openFileDialog.Multiselect = false;
                if (openFileDialog.ShowDialog( ) == DialogResult.OK)
                {
                    textBox4.Text = openFileDialog.FileName;
                }
            }
        }
    }
}
