using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using GroupLab.iNetwork;
using GroupLab.iNetwork.Tcp;

namespace WhiteBoardServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Class Instance Variables
        private Server _server;
        private List<Connection> _clients;
        #endregion


        #region Constructors
        public MainWindow()
        {
            InitializeComponent();
            InitializeServer();

            this.Closing += new CancelEventHandler(OnWindowClosing);
        }
        #endregion


        #region Initialization
        private void InitializeServer()
        {
            this._clients = new List<Connection>();

            // Creates a new server with the given name and is bound to the given port.
            this._server = new Server("WhiteBoardService", 12345);
            this._server.IsDiscoverable = true;
            this._server.Connection += new ConnectionEventHandler(OnServerConnection);
            this._server.Start();

            this._statusLabel.Content = "IP: " + this._server.Configuration.IPAddress.ToString()
                + ", Port: " + this._server.Configuration.Port.ToString();
        }

        // Handles the event that a connection is made
        private void OnServerConnection(object sender, ConnectionEventArgs e)
        {
            if (e.ConnectionEvent == ConnectionEvents.Connect)
            {
                // Lock the list so that only the networking thread affects it
                lock (this._clients)
                {
                    if (!(this._clients.Contains(e.Connection)))
                    {
                        // Add to list and create event listener
                        this._clients.Add(e.Connection);
                        e.Connection.MessageReceived += new ConnectionMessageEventHandler(OnConnectionMessage);

                        // Using the GUI thread make changes
                        this.Dispatcher.Invoke(
                            new Action(
                                delegate()
                                {
                                    this._clientsList.Items.Add(e.Connection);
                                }
                        ));
                    }
                }
            }
            else if (e.ConnectionEvent == ConnectionEvents.Disconnect)
            {
                // Lock the list so that only the networking thread affects it
                lock (this._clients)
                {
                    if (this._clients.Contains(e.Connection))
                    {
                        // Clean up --  remove from list and remove event listener
                        this._clients.Remove(e.Connection);
                        e.Connection.MessageReceived -= new ConnectionMessageEventHandler(OnConnectionMessage);

                        // Using the GUI thread make changes
                        this.Dispatcher.Invoke(
                            new Action(
                                delegate()
                                {
                                    this._clientsList.Items.Remove(e.Connection);
                                }
                        ));
                    }
                }
            }
        }
        #endregion


        #region Main Body
        // Handles the event that a message is received
        private void OnConnectionMessage(object sender, Message msg)
        {
            if (msg != null)
            {
                // Check message name...
                switch (msg.Name)
                {
                    default:
                        //Do nothing
                        break;

                    case "draw":
                        this._server.BroadcastMessage(msg);
                        Console.WriteLine(msg.ToString() + " SERVER: " + msg.GetDoubleField("oldMousePointX") + ": " + msg.GetDoubleField("currentMousePointX"));
                        break;
                }
            }
        }

        // When server window closes, stop running the server
        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this._server != null && this._server.IsRunning)
            {
                this._server.Stop();
            }
        }
        #endregion
    }
}
