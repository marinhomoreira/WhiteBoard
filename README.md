WhiteBoard
==========
This project is related to the first assignment for CSCW & PI course at University of Calgary. It was developed in Visual Studio 2010 using C# with WPF and for communication, iNetwork toolkit.

This project is a shared whiteboard where multiple users can draw anything on his/her device screen and the others users are able to see it in real time and also colaborate to the drawing.

To identify each user, when he/she connects to the server, his/her device receives an ID which is used by the clients to determine which color will be used for each device. So far, there are only four colors available, but it's possible to connect as many clients as wanted - the limitation is that many users will be represented by the same color.

Execution:
To run the system you must, first, run the Server, using Visual Studio or the executable present inside the Debug folder. Server's IP and port will appear on its window.
You need to update client's variables _ipAddress and _port present in MainWindow.xaml.cs file with server's IP and port.
Then, you must Build the project and run the client using Visual Studio or the executable present inside the Debug folder from the client project.

To deploy in other devices, you must copy the executable and the iNetwork dll from the Debug folder to the device that you want to use.
