
using System;
using System.IO;
using System.Text;
using System.Xml;
using GroupLab.Phidgets;
using GroupLab.Phidgets.Components;


namespace PhidgetsXmlConduit
{
	/// <summary>
	/// Phidgets link to Flash via XML
	/// Bjoern Hartmann, bjoern@stanford.edu
	///
	/// EzLCD support has been removed from this version.
	/// 
	/// Async I/O in XmlSocketIO adapted from:
	/// http://msdn.microsoft.com/msdnmag/issues/01/07/ctocsharp/default.aspx
	/// 
	/// You will need the Phidgets .NET dll to compile this project.
	/// If you have trouble compiling, make sure you have the latest version of Phidgets .NET from
	/// http://grouplab.cpsc.ucalgary.ca/software/Phidgets.NET/
	/// Their event handlers changed names some time during Spring 2005.
	/// </summary>
	
	
	class PhidgetsXmlConduit
	{
		 
		
		static GroupLab.Phidgets.Components.InterfaceKit phidgetsKit;
		static XmlSocketIO xmlIO;
		
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		//[STAThread]
		static void Main(string[] args)
		{
			// create our socket server on localhost, port 13001
			xmlIO = new XmlSocketIO("127.0.0.1",13001);
			
			// attach a local event handler for incoming XML messages
			xmlIO.XmlIn +=new XmlInEventHandler(xmlIO_XmlIn);
			
			// create a Phidgets InterfaceKit object and attach our event handlers
			phidgetsKit = new InterfaceKit();
			phidgetsKit.AutoAttach = true;
			phidgetsKit.PhidgetDevice = null;
			phidgetsKit.Error +=new GroupLab.Phidgets.Components.EventArguments.PhidgetErrorEventHandler(phidgetsKit_Error);
			phidgetsKit.SensorChange +=new GroupLab.Phidgets.Components.EventArguments.IndexSensorChangeEventHandler(phidgetsKit_SensorChange);
			phidgetsKit.InputChange +=new GroupLab.Phidgets.Components.EventArguments.BitStateChangeEventHandler(phidgetsKit_InputChange);

			// loop forever - all action is done by the event handlers
			while(true);
		}
		
		/// <summary>
		/// Event handler for incoming XML messages; raised by XmlSocketIO class.
		/// Currently only scans for digital output events and passes them to phidgets
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private static void xmlIO_XmlIn(object sender, XmlInEventArgs e)
		{
			Console.WriteLine("Msg in: {0}",e.Text);
			
			// XmlTextReader is a simple forward-only XML parser
			XmlTextReader reader = new XmlTextReader(e.Text,XmlNodeType.Element, null);
			
			// scan through received XML from beginning to end:
			while(reader.Read()) 
			{
				if(reader.Name == "event") //this is the element name
				{
					if(reader.GetAttribute("type").Equals("digital_out")) 
					{
						// load values passed in attributes
						byte index = Convert.ToByte(reader.GetAttribute("index"));
						bool val   = Convert.ToBoolean(reader.GetAttribute("value"));
						phidgetsKit.Outputs[index].State=val;
					}
					// add code for other outputs (e.g., servo motor) here
				}
			}
		}

		/// <summary>
		/// Event handler for analog voltage-varying sensor input changes (=sliders, pots, etc).
		/// Sends an XML Element to Flash.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private static void phidgetsKit_SensorChange(object sender, GroupLab.Phidgets.Components.EventArguments.IndexSensorChangeEventArgs e)
		{
			xmlIO.SendString("<event type=\"analog_in\" index=\""+e.Index+"\" value=\""+e.Value+"\" />");
		}
		
		
		/// <summary>
		/// Event handler for digital input changes (= buttons, switches).
		/// Sends an XML Element to Flash.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private static void phidgetsKit_InputChange(object sender, GroupLab.Phidgets.Components.EventArguments.BitStateChangeEventArgs e)
		{
			xmlIO.SendString("<event type=\"digital_in\" index=\""+e.Index+"\" value=\""+Convert.ToByte(e.State)+"\" />");
		}
		
		/// <summary>
		/// Event handler for Phidgets errors - prints all errors to the console
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private static void phidgetsKit_Error(object sender, GroupLab.Phidgets.Components.EventArguments.PhidgetErrorEventArgs e)
		{
			Console.WriteLine("Phidgets Error:"+e.Description);

		}
	}
	
	public delegate void XmlInEventHandler(object sender, XmlInEventArgs e);
}
