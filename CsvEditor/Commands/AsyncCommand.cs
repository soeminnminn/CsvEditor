using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CsvEditor.Commands
{
    /// <summary>
	/// Interface for Async Command
	/// </summary>
	public interface IAsyncCommand : ICommand
    {
        /// <summary>
        /// Execute the command async.
        /// </summary>
        /// <returns>Task to be awaited on.</returns>
        Task ExecuteAsync();

        /// <summary>
        /// Raise a CanExecute change event.
        /// </summary>
        void RaiseCanExecuteChanged();
    }

    /// <summary>
    /// Interface for Async Command with parameter
    /// </summary>
    public interface IAsyncCommand<T> : ICommand
    {
        /// <summary>
        /// Execute the command async.
        /// </summary>
        /// <param name="parameter">Parameter to pass to command</param>
        /// <returns>Task to be awaited on.</returns>
        Task ExecuteAsync(T parameter);

        /// <summary>
        /// Raise a CanExecute change event.
        /// </summary>
        void RaiseCanExecuteChanged();
    }

    /// <summary>
	/// Implementation of an Async Command
	/// </summary>
	public class AsyncCommand : IAsyncCommand
    {
        private readonly Func<Task> execute;
        private readonly Func<object, bool> canExecute;
        private readonly Action<Exception> onException;
        private readonly bool continueOnCapturedContext;
        private readonly WeakEventManager weakEventManager = new WeakEventManager();

        /// <summary>
        /// Create a new AsyncCommand
        /// </summary>
        /// <param name="execute">Function to execute</param>
        /// <param name="canExecute">Function to call to determine if it can be executed</param>
        /// <param name="onException">Action callback when an exception occurs</param>
        /// <param name="continueOnCapturedContext">If the context should be captured on exception</param>
        public AsyncCommand(Func<Task> execute,
                            Func<object, bool> canExecute = null,
                            Action<Exception> onException = null,
                            bool continueOnCapturedContext = false)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
            this.onException = onException;
            this.continueOnCapturedContext = continueOnCapturedContext;
        }

        /// <summary>
        /// Event triggered when Can Excecute changes.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { weakEventManager.AddEventHandler(value); }
            remove { weakEventManager.RemoveEventHandler(value); }
        }

        /// <summary>
        /// Invoke the CanExecute method and return if it can be executed.
        /// </summary>
        /// <param name="parameter">Parameter to pass to CanExecute.</param>
        /// <returns>If it can be executed.</returns>
        public bool CanExecute(object parameter) => canExecute?.Invoke(parameter) ?? true;

        /// <summary>
        /// Execute the command async.
        /// </summary>
        /// <returns>Task of action being executed that can be awaited.</returns>
        public Task ExecuteAsync() => execute();

        /// <summary>
        /// Raise a CanExecute change event.
        /// </summary>
        public void RaiseCanExecuteChanged() => weakEventManager.HandleEvent(this, EventArgs.Empty, nameof(CanExecuteChanged));

        #region Explicit implementations
        void ICommand.Execute(object parameter) => ExecuteAsync().SafeFireAndForget(onException, continueOnCapturedContext);
        #endregion
    }
    /// <summary>
    /// Implementation of a generic Async Command
    /// </summary>
    public class AsyncCommand<T> : IAsyncCommand<T>
    {

        private readonly Func<T, Task> execute;
        private readonly Func<object, bool> canExecute;
        private readonly Action<Exception> onException;
        private readonly bool continueOnCapturedContext;
        private readonly WeakEventManager weakEventManager = new WeakEventManager();

        /// <summary>
        /// Create a new AsyncCommand
        /// </summary>
        /// <param name="execute">Function to execute</param>
        /// <param name="canExecute">Function to call to determine if it can be executed</param>
        /// <param name="onException">Action callback when an exception occurs</param>
        /// <param name="continueOnCapturedContext">If the context should be captured on exception</param>
        public AsyncCommand(Func<T, Task> execute,
                            Func<object, bool> canExecute = null,
                            Action<Exception> onException = null,
                            bool continueOnCapturedContext = false)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
            this.onException = onException;
            this.continueOnCapturedContext = continueOnCapturedContext;
        }

        /// <summary>
        /// Event triggered when Can Excecute changes.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { weakEventManager.AddEventHandler(value); }
            remove { weakEventManager.RemoveEventHandler(value); }
        }

        /// <summary>
        /// Invoke the CanExecute method and return if it can be executed.
        /// </summary>
        /// <param name="parameter">Parameter to pass to CanExecute.</param>
        /// <returns>If it can be executed</returns>
        public bool CanExecute(object parameter) => canExecute?.Invoke(parameter) ?? true;

        /// <summary>
        /// Execute the command async.
        /// </summary>
        /// <returns>Task that is executing and can be awaited.</returns>
        public Task ExecuteAsync(T parameter) => execute(parameter);

        /// <summary>
        /// Raise a CanExecute change event.
        /// </summary>
        public void RaiseCanExecuteChanged() => weakEventManager.HandleEvent(this, EventArgs.Empty, nameof(CanExecuteChanged));

        #region Explicit implementations

        void ICommand.Execute(object parameter)
        {
            if (CommandUtils.IsValidCommandParameter<T>(parameter))
                ExecuteAsync((T)parameter).SafeFireAndForget(onException, continueOnCapturedContext);

        }
        #endregion
    }
}
