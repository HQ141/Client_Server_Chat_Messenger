using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Messenger_Server_Side
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void FormClosing(object sender, FormClosingEventHandler e)
        {
            Socket server = App.Current.Properties["Socket"] as Socket;
            if (server.Connected)
            {
                System.Windows.MessageBox.Show("Disconnecting Socket");
                server.Close();
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            IPAddress address;
            int Iport;
            Socket server;
            try
            {
                string Sport = Port.Text;
                Iport = int.Parse(Sport);
            }
            catch
            {
                System.Windows.MessageBox.Show("Incorrect Format or Value Enter Again");
                return;
            }
            address = IPAddress.Parse("127.0.0.1");
            string msg = "Binding to " + address.ToString() + ":" + Iport;
            IPEndPoint localhost = new IPEndPoint(address, Iport);
            Port.IsEnabled = false;
            System.Windows.MessageBox.Show(msg);
            
            try
            {
                server = new Socket(SocketType.Stream, ProtocolType.Tcp);
                server.Bind(localhost);
                App.Current.Properties["OrgSocket"] = server;
                server.Listen(10);
                recv_msg.Clear();
                send_msg.IsEnabled = true;
                Bind.IsEnabled = false;
                ThreadStart ts1 = new ThreadStart(recv_Messages);
                Thread t1 = new Thread(ts1);
                t1.IsBackground = true;
                t1.Start();
            }
            catch(Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }

        }
        private void recv_Messages()
        {
            Socket server = App.Current.Properties["OrgSocket"] as Socket;
            string data = null;
            byte[] bytes = null;
            Socket handler = server.Accept();
            App.Current.Properties["Socket"] = handler;
            try
            {
                while (true)
                {
                    bytes = new byte[1024];
                    int bytesRec = handler.Receive(bytes);
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
                handler.Close();
                server.Close();
                System.Windows.MessageBox.Show("Client has terminated Connection Pls End");
                this.Dispatcher.Invoke(() =>
                {
                    this.Close();
                });
                return;
            }
        }
        private void Port_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Port.Clear();
        }

        private void MSG_send(object sender, RoutedEventArgs e)
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
                    if(t1!=null)
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

