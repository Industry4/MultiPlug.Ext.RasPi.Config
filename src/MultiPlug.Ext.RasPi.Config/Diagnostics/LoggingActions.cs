using System;
using System.Threading.Tasks;
using MultiPlug.Ext.RasPi.Config.Utils.Swan;

namespace MultiPlug.Ext.RasPi.Config.Diagnostics
{
    internal static class LoggingActions
    {
        internal static void LogTaskResult(Action<EventLogEntryCodes, string[]> theLogAction, Task<ProcessResult> theTask, EventLogEntryCodes theErroredEventLogCode)
        {
            if (theTask != null)
            {
                if ( ! theTask.Result.Okay()) { theLogAction?.Invoke(theErroredEventLogCode, new string[] { theTask.Result.GetOutput() }); }
            }
        }

        internal static void LogTaskResult(Action<EventLogEntryCodes, string[]> theLogAction, Task<ProcessResult> theTask, EventLogEntryCodes theCompleteEventLogCode, EventLogEntryCodes theErroredEventLogCode)
        {
            if (theTask != null)
            {
                if (theTask.Result.Okay()) { theLogAction?.Invoke(theCompleteEventLogCode, null); }
                else { theLogAction?.Invoke(theErroredEventLogCode, new string[] { theTask.Result.GetOutput() }); }
            }
        }

        internal static void LogTaskResult(Action<EventLogEntryCodes, string[]> theLogAction, Task<ProcessResult> theTask, bool theState, EventLogEntryCodes theTrueCompleteEventLogCode, EventLogEntryCodes theFalseCompleteEventLogCode, EventLogEntryCodes theErroredEventLogCode)
        {
            if (theTask != null)
            {
                if (theTask.Result.Okay())
                {
                    if( theState ) { theLogAction?.Invoke(theTrueCompleteEventLogCode, null); }
                    else { theLogAction?.Invoke(theFalseCompleteEventLogCode, null); }
                }
                else { theLogAction?.Invoke(theErroredEventLogCode, new string[] { theTask.Result.GetOutput() }); }
            }
        }

        internal static void LogTaskAction(Action<EventLogEntryCodes, string[]> theLogAction, bool theState, EventLogEntryCodes theTrueEventLogCode, EventLogEntryCodes theFalseEventLogCode)
        {
            if (theState) { theLogAction?.Invoke(theTrueEventLogCode, null); }
            else { theLogAction?.Invoke(theFalseEventLogCode, null); }
        }

        internal static void LogTaskAction(Action<EventLogEntryCodes, string[]> theLogAction, int theInt, EventLogEntryCodes[] theEventLogEntryCodes)
        {
            if( theEventLogEntryCodes != null)
            {
                EventLogEntryCodes EventLogEntryCode;

                try
                {
                    EventLogEntryCode = theEventLogEntryCodes[theInt];
                }
                catch (IndexOutOfRangeException)
                {
                    return;
                }

                theLogAction?.Invoke(EventLogEntryCode, null);
            }
        }
    }
}
