using System;
using System.Reflection;
using System.Threading.Tasks;

namespace CsvEditor.Commands
{
    internal static class CommandUtils
    {
        internal static bool IsValidCommandParameter<T>(object o)
        {
            bool valid;
            if (o != null)
            {
                // The parameter isn't null, so we don't have to worry whether null is a valid option
                valid = o is T;

                if (!valid)
                    throw new InvalidCommandParameterException(typeof(T), o.GetType());

                return valid;
            }

            var t = typeof(T);

            // The parameter is null. Is T Nullable?
            if (Nullable.GetUnderlyingType(t) != null)
            {
                return true;
            }

            // Not a Nullable, if it's a value type then null is not valid
            valid = !t.GetTypeInfo().IsValueType;

            if (!valid)
                throw new InvalidCommandParameterException(typeof(T));

            return valid;
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        /// <summary>
        /// Attempts to await on the task and catches exception
        /// </summary>
        /// <param name="task">Task to execute</param>
        /// <param name="onException">What to do when method has an exception</param>
        /// <param name="continueOnCapturedContext">If the context should be captured.</param>
        internal static async void SafeFireAndForget(this Task task, Action<Exception> onException = null, bool continueOnCapturedContext = false)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            try
            {
                await task.ConfigureAwait(continueOnCapturedContext);
            }
            catch (Exception ex) when (onException != null)
            {
                onException.Invoke(ex);
            }
        }
    }
}
