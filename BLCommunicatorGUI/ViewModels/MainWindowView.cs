using BLCommunicatorGUI.Constants;
using BLCommunicatorGUI.Logic;
using BLCommunicatorGUI.Models;
using BLCommunicatorGUI.Views;

namespace BLCommunicatorGUI.ViewModels
{
    public class MainWindowVm : BaseViewModel<MainWindow>
    {
        #region Properties
        //text to be output on screen
        public string OutputText
        {
            get => _outputText;
            set
            {
                _outputText = value;
                NotifyPropertyChanged();
            }
        }

        //text to be sent to the serial port through the command line
        public string InputText
        {
            get => _inputText;
            set
            {
                _inputText = value;
                NotifyPropertyChanged();
            }
        }

        //should the serial monitor scroll down?
        public bool ShouldScrollBottom
        {
            get => _shouldScrollBottom;
            set
            {
                _shouldScrollBottom = value;
                NotifyPropertyChanged();
            }
        }

        //should the output be filtered?
        public bool ShouldFilterOutput
        {
            get => _shouldFilterOutput;
            set
            {
                _shouldFilterOutput = value;
                _serialCommunicator.ShouldFilterOutput = value;
                NotifyPropertyChanged();
            }
        }

        //how many moving cycles should be sent to the bot?
        public byte CyclesCount
        {
            get => _cyclesCount;
            set
            {
                _cyclesCount = value;
                UpdateDirectionText();
                NotifyPropertyChanged();
            }
        }

        //string containing movement commands
        public string CommandText
        {
            get => _commandText;
            set
            {
                _commandText = value;
                NotifyPropertyChanged();
            }
        }

        //can we connect to a serial port?
        public bool CanConnect
        {
            get => _canConnect;
            set
            {
                _canConnect = value;
                NotifyPropertyChanged();
            }
        }

        //what serial port are we connected to?
        public int SelectedPort
        {
            get => _selectedPort;
            set
            {
                _selectedPort = value;
                ComPortChanged();
                NotifyPropertyChanged();
            }
        }

        //what serial ports can we connect to?
        public string[] PortNames
        {
            get => _portNames;
            set
            {
                _portNames = value;
                NotifyPropertyChanged();
            }
        }

        //what is the distance from the right ultrasonic sensor? 
        public string? RightDistanceText
        {
            get => _rightDistanceText;
            set
            {
                _rightDistanceText = value;
                NotifyPropertyChanged();
            }
        }
        
        //what is the distance from the left ultrasonic sensor?
        public string? LeftDistanceText
        {
            get => _leftDistanceText;
            set
            {
                _leftDistanceText = value;
                NotifyPropertyChanged();
            }
        }

        //what is the median distance from both ultrasonic sensors?
        public string? TotalMedianDistanceText
        {
            get => _totalMedianDistanceText;
            set
            {
                _totalMedianDistanceText = value;
                NotifyPropertyChanged();
            }
        }
        #endregion

        #region Commands
        public BasicCommand ReconnectCommand => new(Reconnect);
        public BasicCommand OnReturnCommand => new(OnReturn);
        public BasicCommand StandUpCommand => new(StandUp);
        public BasicCommand LieDownCommand => new(LieDown);
        public BasicCommand FrontCommand => new(()=>CreateDirection(Direction.Front));
        public BasicCommand BackCommand => new(()=>CreateDirection(Direction.Back));
        public BasicCommand LeftCommand => new(()=>CreateDirection(Direction.Left));
        public BasicCommand RightCommand => new(()=>CreateDirection(Direction.Right));
        public BasicCommand SendDirectionCommand => new(SendDirection);
        public BasicCommand ClearDirectionCommand => new(ClearDirection);
        public BasicCommand ClearOutputCommand => new(ClearOutput);
        #endregion

        #region Backing Property Fields
        private string _outputText = "";
        private string _inputText = "";
        private bool _shouldScrollBottom = true;
        private bool _shouldFilterOutput = true;
        private byte _cyclesCount = 1;
        private string _commandText = "Front 1";
        private bool _canConnect;
        private int _selectedPort = 2;
        private string[] _portNames = new []{"COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8"};
        private string? _rightDistanceText;
        private string? _leftDistanceText;
        private string? _totalMedianDistanceText;
        #endregion
        
        private SerialCommunicator _serialCommunicator;
        private string _currentCmd = "Front"; //default direction
        private string _currentPortName = "COM5"; //default port name
        private bool _isMoving; //is the bot moving? 

        //connect to the default serial port and show result
        public MainWindowVm(MainWindow window) : base(window)
        {
            _serialCommunicator = new SerialCommunicator(DisplayData, ReceivedPingData, out var hasConnected, _currentPortName, ShouldFilterOutput);

            CanConnect = !hasConnected;
        }

        //reconnect to the bot on the selected serial port and show result
        private void Reconnect()
        {
            _serialCommunicator = new SerialCommunicator(DisplayData, ReceivedPingData, out var hasConnected, _currentPortName, ShouldFilterOutput);
            
            CanConnect = !hasConnected;
        }

        //when the serial port changes, close the connection and ask to reconnect
        private void ComPortChanged()
        {
            CanConnect = true;
            _currentPortName = PortNames[SelectedPort];
            _serialCommunicator.CloseConnection();
            DisplayData("COM Port changed, please reconnect to correctly communicate with the bot!\n");
        }

        //when the serial monitor input field receives a return send the input text to the Bot
        private void OnReturn()
        {
            var data = InputText;

            InputText = "";

            SendData(data);
        }

        //send the stand up command to the Bot
        private void StandUp()
        {
            SendData("1");
        }

        //send the lie down command to the Bot
        private void LieDown()
        {
            SendData("0");
        }

        //sets the direction to be used when sending movement commands and update UI
        private void CreateDirection(Direction direction)
        {
            _currentCmd = direction.ToString();

            UpdateDirectionText();
        }

        //updates the current set direction
        private void UpdateDirectionText()
        {
            CommandText = $"{_currentCmd} {CyclesCount.ToString()}";
        }

        //send the direction to the Bot
        private void SendDirection()
        {
            var stringToSend = "2 ";

            stringToSend += CommandText.Replace("Front", "0").Replace("Back", "1").Replace("Left", "2").Replace("Right", "3");

            _isMoving = true;
            SendData(stringToSend);
        }

        //clear the current selected direction
        private void ClearDirection()
        {
            CommandText = "";
        }
        
        //clears the output of the serial monitor
        private void ClearOutput()
        {
            OutputText = "";
        }

        //send data to the serial port
        private void SendData(string dataToSend)
        {
            _serialCommunicator.SendData(dataToSend);
        }

        //when distance data is received
        //0: right sensor, 1: left sensor, 2: avg of both
        private void ReceivedPingData(int[] data)
        {
            RightDistanceText = data[0].ToString();
            LeftDistanceText = data[1].ToString();

            TotalMedianDistanceText = data[2].ToString();

            if (data[2] < 30 && _isMoving) //if the bot is 30 cm or closer to an obstacle, it stops
            {
                _isMoving = false;
                DisplayData("Tool: Bot encountered obstacle, stopping... \n");
                StandUp();
            }
        }

        //display data on the Serial Monitor
        private void DisplayData(string data)
        {
            OutputText += data;
            if (ShouldScrollBottom) ViewWindow.Dispatcher.Invoke(ViewWindow.OutputViewer.ScrollToEnd);
        }
    }
}