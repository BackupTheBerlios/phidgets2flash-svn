////////////////////////////////////////////////////////////
// PhidgetsTest.as
// Bjoern Harmann, bjoern@stanford.edu
// June 2005
// 

////////////////////////////////////////////////////////////
// Add a listener to every output check box so we can
// fire off the appropriate socket actions
//
changeListener = new Object();
changeListener.click = function(evt) {
	sendOutputXML(Number(evt.target.label),Boolean(evt.target.selected));
}

checkBox_output0.addEventListener("click",changeListener);
checkBox_output1.addEventListener("click",changeListener);
checkBox_output2.addEventListener("click",changeListener);
checkBox_output3.addEventListener("click",changeListener);
checkBox_output4.addEventListener("click",changeListener);
checkBox_output5.addEventListener("click",changeListener);
checkBox_output6.addEventListener("click",changeListener);
checkBox_output7.addEventListener("click",changeListener);

////////////////////////////////////////////////////////////
// XML code below

// Create XML socket on localhost, port 13001
var socket:XMLSocket = new XMLSocket();

// Try to connect
if (!socket.connect("127.0.0.1", 13001)) {
	trace ("Connection failed!");
} else {
	trace ("Connection succeeded!");
}

// function to deal with incoming XML
socket.onXML = function (doc) {
	node = doc.firstChild;
	if(node.attributes.type=="digital_in") {
		// account for different boolean representations - Flash is bad at automatically doing this
		if (node.attributes.value == "True" || node.attributes.value == "true" || node.attributes.value == "1")
			_root["checkBox_input"+node.attributes.index].selected = true;
		else
			_root["checkBox_input"+node.attributes.index].selected = false;
		trace(node.attributes.value);
	}
	else if(node.attributes.type=="analog_in") {
		_root["stepper_input"+node.attributes.index].value = node.attributes.value;
	}
	// handle other inputs here
	else {
		trace("Don't know what to do with the received XML input: "+node.toString());
	}
	
}

// function to send XML messages out
sendOutputXML =function(index:Number,val:Boolean) {
	var my_xml:XML = new XML();
	var node:XMLNode = my_xml.createElement("event");
	node.attributes.type = "digital_out";
	node.attributes.index = index;
	node.attributes.value = val;
	my_xml.appendChild(node);
	socket.send(my_xml);
}
