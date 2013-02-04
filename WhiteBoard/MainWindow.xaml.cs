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
        // Must change to server's IP and port
        private string _ipAddress = "192.168.0.119";
        private int _port = 12345;

        private void InitializeConnection()
        {
            this._connection = new Connection(this._ipAddress, this._port);
            this._connection.Connected += new ConnectionEventHandler(OnConnected);
            this._connection.Start();
        }

        void OnConnected(object sender, ConnectionEventArgs e)
        {
            this._connection.MessageReceived += new ConnectionMessageEventHandler(OnMessageReceived);
        }
        #endregion

        // client ID used to determine the color of the brush
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
                    case "clientId":
                        // receives device ID
                        this.cID = msg.GetIntField("id");
                        Console.WriteLine("Client ID: " + cID);
                        break;
                    case "draw":
                        // verify who is the message's sender, if it's the same device, don't do anything
                        if (msg.GetIntField("from") != this.cID)
                        {
                            Point oldP = new Point(msg.GetDoubleField("oldMousePointX"), msg.GetDoubleField("oldMousePointY"));
                            Point currentP = new Point(msg.GetDoubleField("currentMousePointX"), msg.GetDoubleField("currentMousePointY"));
                            int color = msg.GetIntField("from") % 4;
                            Console.WriteLine("MSG RECEIVED FROM " + msg.GetIntField("from") + ": " + msg.GetDoubleField("oldMousePointX") + ": " + msg.GetDoubleField("currentMousePointX"));
                            this.Dispatcher.Invoke(
                            new Action(
                                delegate()
                                {
                                    drawFM(oldP, currentP, color);
                                }
                            ));
                        }
                        break;
                    default:
                        Console.WriteLine(msg.Name);
                        break;
                }
            }
        }

        private void sendMessage(Point oldMousePoint, Point currentMousePoint)
        {
            Message msg = new Message("draw");
            msg.AddField("from", this.cID);
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
                this.oldMousePoint = currentMousePoint;
            }
        }

        private void drawFM(Point oldP, Point newP, int colorId)
        {
            // When a draw message is received it must know who sent the message so the color of the draw will be setted
            Line line = new Line();
            switch (colorId)
            {
                case 0:
                    line.Stroke = Brushes.Blue;        
                    break;
                case 1:
                    line.Stroke = Brushes.DarkGray;
                    break;
                case 2:
                    line.Stroke = Brushes.Red;
                    break;
                case 3:
                    line.Stroke = Brushes.Green;
                    break;
            }
            
            
            line.StrokeThickness = 1;
            line.X1 = oldP.X;
            line.Y1 = oldP.Y;
            line.X2 = newP.X;
            line.Y2 = newP.Y;

            _mainContainer.Children.Add(line);
        }


        private void draw(Point oldP, Point newP)
        {
            // Default method to draw on screen
            Line line = new Line();
            line.Stroke = Brushes.Black;
            line.StrokeThickness = 1;
            line.X1 = oldP.X;
            line.Y1 = oldP.Y;
            line.X2 = newP.X;
            line.Y2 = newP.Y;

            _mainContainer.Children.Add(line);         
        }

        private void myCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.oldMousePoint = e.GetPosition((IInputElement)sender);
            sendMessage(this.oldMousePoint, this.oldMousePoint);
        }
        #endregion

    }
}
