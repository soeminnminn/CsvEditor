using System;
using System.Windows.Input;

namespace CsvEditor.Commands
{
    public class CommandBindingLink : CommandBinding
    {
        private readonly ICommand linkedCommand;
        private readonly WeakEventManager weakEventManager = new WeakEventManager();

        /// <summary>
        /// Event handler raised when CanExecute changes.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { weakEventManager.AddEventHandler(value); }
            remove { weakEventManager.RemoveEventHandler(value); }
        }

        /// <summary>
        /// Gets the linked System.Windows.Input.ICommand
        /// </summary>
        public ICommand LinkedCommand { get => linkedCommand; }

        public CommandBindingLink(ICommand command, ICommand linkedCommand)
            : base(command)
        {
            if (linkedCommand == null)
            {
                throw new ArgumentNullException("linkedCommand");
            }

            this.linkedCommand = linkedCommand;
            this.linkedCommand.CanExecuteChanged += Command_CanExecuteChanged;

            CanExecute += OnCanExecute;
            Executed += OnExecuted;
        }

        private void Command_CanExecuteChanged(object sender, EventArgs e)
        {
            weakEventManager.HandleEvent(this, EventArgs.Empty, nameof(CanExecuteChanged));
        }

        private void OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (!e.Handled)
            {
                if (e.RoutedEvent == CommandManager.ExecutedEvent)
                {
                    linkedCommand.Execute(e.Parameter);
                    e.Handled = true;
                }
            }
        }

        private void OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (!e.Handled)
            {
                if (e.RoutedEvent == CommandManager.CanExecuteEvent)
                {
                    e.CanExecute = linkedCommand.CanExecute(e.Parameter);
                    if (e.CanExecute)
                    {
                        e.Handled = true; 
                    }
                }
                else if (e.CanExecute)
                {
                    e.CanExecute = true;
                    e.Handled = true;
                }
            }
        }
    }
}
