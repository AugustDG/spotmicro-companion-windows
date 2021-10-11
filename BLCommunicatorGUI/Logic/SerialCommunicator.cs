using System;
using System.IO.Ports;
using System.Linq;

namespace BLCommunicatorGUI.Logic
{
    /// <summary>
    /// Class used to communicate through the serial port with the Bot. It's semi-specific to the current use case.
    /// </summary>
    public class SerialCommunicator
    {
        public bool ShouldFilterOutput; //should the output be filtered? (i.e. to remove sensor data) 

        private SerialPort? _serialPort; //serial port object to communicate with the bot
        
        private Action<string>? OnReceiveData; //action called when data is received from the serial port
        private Action<int[]>? OnPingData; //returns rolling median ping from last 5 pings

        private int[] _lastAvgPings = new int[5]; //array used to contain the ultrasonic ping buffer (for averaging)
        private int _numberOPings; //how many pings in the group do we have?

        /// <summary>
        /// Default SerialCommunicator constructor, creates the object with specified parameters
        /// </summary>
        /// <param name="onReceiveData">Callback when serial data is received.</param>
        /// <param name="onPingData">Callback when ultrasonic data is received.</param>
        /// <param name="hasConnected">Out bool, indicates if the serial communication initialized successfully.</param>
        /// <param name="portName">Desired port name to connect to.</param>
        /// <param name="filterOut">Should incoming data be initially be filtered?</param>
        public SerialCommunicator(Action<string>? onReceiveData, Action<int[]>? onPingData, out bool hasConnected, string portName = "COM3", bool filterOut = false)
        {
            try
            {
                OnReceiveData = onReceiveData;
                OnPingData = onPingData;
                ShouldFilterOutput = filterOut;

                //IMPORTANT: Baud rate must be the same as on the Arduino sketch
                _serialPort = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One)
                {
                    Handshake = Handshake.None
                };

                _serialPort.DataReceived += SerialPortOnDataReceived;

                _serialPort.Open(); //opens serial port with correct settings 

                hasConnected = true;
                OnReceiveData?.Invoke($"Successfully opened port: {_serialPort.PortName}, with baud rate: {_serialPort.BaudRate}\n");
            }
            catch (Exception e)
            {
                hasConnected = false;
                OnReceiveData?.Invoke($"An error occured: {e.Message}\n"); //communicates that there was an error on initialization
            }
        }

        /// <summary>
        /// Sends the passed string through the serial port
        /// </summary>
        /// <param name="data">Data to be sent</param>
        public void SendData(string data)
        {
            try
            {
                _serialPort?.Write(data + '\n'); //sends data to the Bot
            }
            catch (Exception e)
            {
                OnReceiveData?.Invoke($"An error occured: {e.Message}\n"); //communicates that there was an error on sending data
            }
        }

        /// <summary>
        /// Closes connection with the serial port.
        /// </summary>
        public void CloseConnection()
        {
            _serialPort?.Close();
        }

        //when data is received process it and send it through the OnReceiveData callback
        private void SerialPortOnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (_serialPort == null) return; //if the serial port doesn't exist, just get out

            string outString = DateTime.Now.ToString("hh:mm:ss:fff"); //start with a timestamp

            var serialLine = _serialPort.ReadLine(); //get the line of text from the serial port

            //TODO: make so that if we're filtering output, still display ping data to the UI
            // filters output to avoid spamming the tool's console
            if (ShouldFilterOutput)
            {
                if (serialLine.Length < 1) return; //if it's noise, forget it
                if (serialLine[0] == '0') //if the data identifier is '0', then it's a **ultrasonic ping**
                {
                    var allDistances = new int[3]; //0: distance from right sensor, 1: distance from left sensor, 2: median distance from both 

                    serialLine = serialLine.Remove(0, 1); //remove the data identifier

                    var splits = serialLine.Split(' '); //split the incoming string, to get individual bits of data

                    try
                    {
                        //TODO: make this *not* hard coded
                        allDistances[0] = int.Parse(splits[0].Remove(0, 1)); //remove the 'R' indicating it's the right sensor
                        allDistances[1] = int.Parse(splits[1].Remove(0, 1)); //remove the 'L' indicating it's the left sensor

                        _lastAvgPings[_numberOPings] = (allDistances[0] + allDistances[1]) / 2; //get average distance from both sensors
                        
                        if (_numberOPings < 4) _numberOPings++;
                        else _numberOPings = 0;

                        allDistances[2] = (int)_lastAvgPings.AsQueryable().Average(); //gets the average of the last 5 pings
                        OnPingData?.Invoke(allDistances); //sends the data along
                    }
                    catch (Exception exception)
                    {
                        // ignored, could be Arduino Serial corruption
                    }
                }   
            }
            else
            {
                //if we're not filtering data, remove the data identifier from the beginning
                serialLine = serialLine[0] switch
                {
                    '0' => serialLine.Remove(0, 1),
                    _   => serialLine
                };

                //formats it properly and sends it through the callback
                outString += $" -> {serialLine}";
                OnReceiveData?.Invoke(outString);   
            }
        }
    }
}