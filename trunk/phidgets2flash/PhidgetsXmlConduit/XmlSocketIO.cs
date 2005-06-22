using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Xml;


namespace PhidgetsXmlConduit
{
	/// <summary>
	/// XmlSocketIO - general socket-based message passing (nothing in here is XML-specific - may need to be renamed?)
	/// </summary>
	public class XmlSocketIO
	{
		private NetworkStream stream;       
		private byte[] readBuffer;          
		private AsyncCallback readCallBack;
		private TcpListener server;

		public event XmlInEventHandler XmlIn;

		
		public XmlSocketIO(String address, Int32 port)
		{
			IPAddress IPAddr = IPAddress.Parse(address);
			server = new TcpListener(IPAddr, port);
			
			// Start listening for client requests.
			server.Start();
			Console.Write("Waiting for a connection to "+address+", port "+port+"... ");
			
			// Perform a blocking call to accept requests.
			// You could also user server.AcceptSocket() here.
			TcpClient client = server.AcceptTcpClient();            
			Console.WriteLine("Connected!");
			
			stream = client.GetStream();
			readBuffer = new byte[256]; // maximum size of a message 
			
			// set the function that will be called when a complete message came in
			readCallBack = new AsyncCallback(this.OnCompletedRead);
			
			// start asynchronous read from socket
			stream.BeginRead(
				readBuffer,             // where to put the results
				0,						// offset
				readBuffer.Length,      // how many bytes (BUFFER_SIZE)
				readCallBack,			// call back delegate
				null);					// local state object
		}
		/// <summary>
		/// When a complete message (\0 terminated?) has been received,
		/// rasie the XmlIn event and pass the message in the event
		/// </summary>
		/// <param name="asyncResult"></param>
		
		void OnCompletedRead(IAsyncResult asyncResult)
		{
			int bytesRead = stream.EndRead(asyncResult);
			if (bytesRead > 0) 
			{
				// careful when passing non-ascii strings!
				String s = Encoding.ASCII.GetString(readBuffer, 0, bytesRead);

				// raise XmlIn event 
				XmlInEventArgs e = new XmlInEventArgs(s);
				XmlIn(this,e);
			
				// go back to reading the port
				stream.BeginRead(readBuffer, 0, readBuffer.Length,
					readCallBack, null);
			}
		}

		/// <summary>
		/// Send a string over the socket (and output string to local console).
		/// </summary>
		/// <param name="s"></param>
		public void SendString(string s)
		{
			s = s+'\0'; // add zero character to terminate message.
			Byte[] msg = System.Text.Encoding.ASCII.GetBytes(s);
			stream.Write(msg, 0, msg.Length);
			Console.WriteLine(String.Format("Msg out: {0}", s)); 
		}

	}
	
	/// <summary>
	/// Class to hold arguments for incoming XML event
	/// </summary>
	public class XmlInEventArgs : EventArgs 
	{   
		private string s;
		// Constructor.
		public XmlInEventArgs(string sIn) {s=sIn;}
		// Properties.
		public string Text { get { return s;}}
	}

}
