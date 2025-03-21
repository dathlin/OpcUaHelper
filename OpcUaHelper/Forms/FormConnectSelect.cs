using Opc.Ua;
using Opc.Ua.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
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

		private async void button3_Click( object sender, EventArgs e )
		{
			// 证书登录
			try
			{
				// 如果需要生成自签名的证书，可以参考网址：http://www.hsltechnology.cn/Doc/HslCommunication?chapter=HslCommChapter6-5
				X509Certificate2 certificate = new X509Certificate2( textBox4.Text, textBox3.Text, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable );
				m_OpcUaClient.UserIdentity = new UserIdentity( certificate );

				FileInfo fileInfo = new FileInfo( textBox4.Text );
				m_OpcUaClient.AppConfig.SecurityConfiguration.ApplicationCertificate.StoreType = CertificateStoreType.Directory;
				m_OpcUaClient.AppConfig.SecurityConfiguration.ApplicationCertificate.StorePath = fileInfo.DirectoryName;
				m_OpcUaClient.AppConfig.SecurityConfiguration.ApplicationCertificate.SubjectName = null;

				m_OpcUaClient.AppConfig.SecurityConfiguration.TrustedPeerCertificates.StoreType = CertificateStoreType.Directory;
				m_OpcUaClient.AppConfig.SecurityConfiguration.TrustedPeerCertificates.StorePath = fileInfo.DirectoryName;

				m_OpcUaClient.AppConfig.SecurityConfiguration.TrustedIssuerCertificates.StoreType = CertificateStoreType.Directory;
				m_OpcUaClient.AppConfig.SecurityConfiguration.TrustedIssuerCertificates.StorePath = fileInfo.DirectoryName;

				// 创建一个应用实例对象，用于检查证书
				var application = new ApplicationInstance( m_OpcUaClient.AppConfig );
				// 检查应用实例对象的证书
				bool check = await application.CheckApplicationInstanceCertificate( false, 2048 );

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
