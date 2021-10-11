using System;
using System.Windows.Input;

namespace BLCommunicatorGUI.Models
{
    /// <summary>
    /// Basic command class to simplify the MVVM architecture. Comes from: https://www.c-sharpcorner.com/UploadFile/e06010/wpf-icommand-in-mvvm/
    /// </summary>
    public class BasicCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        private readonly Action _actionToExecute;
        
        public BasicCommand(Action actionToExecute)
        {
            _actionToExecute = actionToExecute;
        }
        
        public bool CanExecute(object? parameter)
        {
            return true;
        }
        
        public void Execute(object? parameter)
        {
            _actionToExecute.Invoke();
        }
    }
}