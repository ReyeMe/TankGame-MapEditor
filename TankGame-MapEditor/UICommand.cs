namespace TankGame_MapEditor
{
    using System;
    using System.Windows.Input;

    /// <summary>
    /// Base UI command
    /// </summary>
    public class UICommand : ICommand
    {
        /// <summary>
        /// Action to execute
        /// </summary>
        private Action<object> action;

        /// <summary>
        /// Function to check if action can be executed
        /// </summary>
        private Func<object, bool> canExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="UICommand"/> class
        /// </summary>
        /// <param name="action">Action to execute</param>
        public UICommand(Action<object> action) : this(action, null)
        {
            // Do nothing
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UICommand"/> class
        /// </summary>
        /// <param name="action">Action to execute</param>
        /// <param name="canExecute">Function to check if action can be executed</param>
        public UICommand(Action<object> action, Func<object, bool> canExecute)
        {
            this.action = action;
            this.canExecute = canExecute;
        }

        /// <summary>
        /// Can execute action status changed
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Check if action can be executed
        /// </summary>
        /// <param name="parameter">Execution parameter</param>
        /// <returns>True if action can be executed</returns>
        public bool CanExecute(object parameter)
        {
            return parameter == null || this.canExecute == null || this.canExecute.Invoke(parameter);
        }

        /// <summary>
        /// Execute action
        /// </summary>
        /// <param name="parameter">Action parameter</param>
        public void Execute(object parameter)
        {
            this.action?.Invoke(parameter);
        }

        /// <summary>
        /// Change can execute
        /// </summary>
        public void ChangeCanExecute()
        {
            this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}