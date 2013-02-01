using System;
using System.Collections.Generic;
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

namespace WhiteBoard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Class Instance Variables
        private Connection _connection;
        #endregion


        #region Constructors
        public MainWindow()
        {
            InitializeComponent();
            InitializeConnection();
        }
        #endregion


        #region Initialization
        private void InitializeConnection()
        {
            // Look for a service/connection with the given name. Once found go to event handler.
            Connection.Discover("WhiteBoardService", new SingleConnectionDiscoveryEventHandler(OnConnectionDiscovered));
        }

        // Handles the event that a connection is discovered
        private void OnConnectionDiscovered(Connection connection)
        {
            // This connection is the connection that is discorvered
            this._connection = connection;

            if (this._connection != null)
            {
                this._connection.MessageReceived += new ConnectionMessageEventHandler(OnMessageReceived);
                // Start connection -- networking thread
                this._connection.Start();
            }
            else
            {
                // Through the GUI thread, close the window
                this.Dispatcher.Invoke(
                    new Action(
                        delegate()
                        {
                            this.Close();
                        }
                ));
            }
        }
        #endregion

        // client ID
        private int cID;

        #region Main Body
        // Handles the event that a message is received
        private void OnMessageReceived(object sender, Message msg)
        {
            if (msg != null)
            {
                // Check message name...
                switch (msg.Name)
                {
                    default:
                        Console.WriteLine(msg.Name);
                        break;
                    case "clientID":
                        this.cID = msg.GetIntField("id");
                        Console.WriteLine("Client ID: " + cID);
                        break;
                    case "draw":
                        Point oldP = new Point(msg.GetDoubleField("oldMousePointX"), msg.GetDoubleField("oldMousePointY"));
                        Point currentP = new Point(msg.GetDoubleField("currentMousePointX"), msg.GetDoubleField("currentMousePointY"));
                        Console.WriteLine("MSG RECEIVED:"+msg.ToString() + ": " + msg.GetDoubleField("oldMousePointX") + ": " + msg.GetDoubleField("currentMousePointX"));
                        this.Dispatcher.Invoke(
                        new Action(
                            delegate()
                            {
                                drawFM(oldP, currentP);
                        
                            }
                        ));
                        
                        break;

                    /*
                     * The following is a basic example:
                     * 
                     * case "ReceivedClientID":
                     *      msg.AddField("clientID", this.clientID);
                     *      break;
                     */
                }
            }
        }

        private void sendMessage(Point oldMousePoint, Point currentMousePoint)
        {
            Message msg = new Message("draw");
            msg.AddField("oldMousePointX", oldMousePoint.X);
            msg.AddField("oldMousePointY", oldMousePoint.Y);
            msg.AddField("currentMousePointX", currentMousePoint.X);
            msg.AddField("currentMousePointY", currentMousePoint.Y);
            if (this._connection != null)
                this._connection.SendMessage(msg);
            Console.WriteLine("SENDING MESSAGE: "+msg.ToString() + ": " + msg.GetDoubleField("oldMousePointX") + ": " + msg.GetDoubleField("currentMousePointX"));
        }

        #endregion


        #region Draw stuff

        Point oldMousePoint;

        private void myCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentMousePoint = e.GetPosition((IInputElement)sender);
                draw(this.oldMousePoint, currentMousePoint);
                sendMessage(this.oldMousePoint, currentMousePoint);
            }
        }

        private void drawFM(Point oldP, Point newP)
        {
            Line line = new Line();
            line.Stroke = Brushes.Blue;
            line.StrokeThickness = 1;
            line.X1 = oldP.X;
            line.Y1 = oldP.Y;
            line.X2 = newP.X;
            line.Y2 = newP.Y;

            _mainContainer.Children.Add(line);
            //this.oldMousePoint = newP;
        }


        private void draw(Point oldP, Point newP)
        {

            Line line = new Line();
            line.Stroke = Brushes.Black;
            line.StrokeThickness = 1;
            line.X1 = oldP.X;
            line.Y1 = oldP.Y;
            line.X2 = newP.X;
            line.Y2 = newP.Y;

            _mainContainer.Children.Add(line);
            this.oldMousePoint = newP;
            
        }

        private void myCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.oldMousePoint = e.GetPosition((IInputElement)sender);
            sendMessage(this.oldMousePoint, this.oldMousePoint);
        }
        #endregion

    }
}
