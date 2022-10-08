using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
namespace Messenger_Client_Side
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

        }
        private void Form_Connection(object sender, RoutedEventArgs e)
        {

            IPAddress address;
            int port;
            string Iip_port;
            try
            {
                Iip_port = ip_port.Text;
                string[] socket = Iip_port.Split(':');
                address = IPAddress.Parse(socket[0]);
                port = Int32.Parse(socket[1]);
                if (port > 65000)
                    throw new Exception();
            }
            catch
            {
                System.Windows.MessageBox.Show("Incorrect Format or Value.Enter Again");
                return;

            }
            string msg = "Forming Connection with " + address.ToString() + ":" + port;
            System.Windows.MessageBox.Show(msg);
            try
            {

                Socket client;
                IPEndPoint server = new IPEndPoint(address, port);
                client = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                client.Connect(server);
                App.Current.Properties["Socket"] = client;
                byte[] msgs = Encoding.ASCII.GetBytes("Test");
                client.Send(msgs);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                return;
            }
            connect.IsEnabled = false;
            Status.Text = "Connected";
            ip_port.IsEnabled = false;
            send_msg.IsEnabled = true;
            ThreadStart ts1 = new ThreadStart(recv_Messages);
            Thread t1 = new Thread(ts1);
            t1.IsBackground = true;
            t1.Start();
            App.Current.Properties["Thread"] = t1;
            send_msg.Clear();
            recv_msg.Clear();
        }

        private void FormClosing(object sender, FormClosedEventArgs e)
        {
            end_everything();
        }
        private void Msg_send(object sender, RoutedEventArgs e)
        {
            try
            {
                Socket client = App.Current.Properties["Socket"] as Socket;
                byte[] msg = Encoding.ASCII.GetBytes(send_msg.Text);
                client.Send(msg);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }
        private void recv_Messages()
        {
            Socket listenner = App.Current.Properties["Socket"] as Socket;
            string data = null;
            byte[] bytes = null;

            try
            {
                while (true)
                {
                    bytes = new byte[1024];
                    int bytesRec = listenner.Receive(bytes);
                    data = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    this.Dispatcher.Invoke(() =>
                    {
                        recv_msg.Text = data;
                    });

                    if (data.IndexOf("<EOF>") > -1)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                listenner.Close();
                System.Windows.MessageBox.Show("Client has terminated Connection Pls End");
                this.Dispatcher.Invoke(() =>
                {
                    this.Close();
                });
                return;
            }


        }
        private void Finish(object sender, RoutedEventArgs e)
        {
            end_everything();
        }
        private void end_everything()
        {
            Socket soc = App.Current.Properties["Socket"] as Socket;
            if (soc != null)
                if (soc.Connected)
                {
                    Thread t1 = App.Current.Properties["Thread"] as Thread;
                    if (t1 != null)
                        t1.Abort();
                    soc.Shutdown(SocketShutdown.Both);
                    soc.Close();
                }
            this.Dispatcher.Invoke(() =>
            {
                this.Close();
            });

        }

    }
}