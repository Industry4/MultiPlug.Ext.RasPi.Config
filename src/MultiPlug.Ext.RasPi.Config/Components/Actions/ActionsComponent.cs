using MultiPlug.Ext.RasPi.Config.Models.Components;
using System;

namespace MultiPlug.Ext.RasPi.Config.Components.Actions
{
    internal class ActionsComponent: SharedProperties
    {
        internal event Action<Diagnostics.EventLogEntryCodes, string[]> Log;
        internal event Action DoSystemRestart;

        internal void RestartSystem()
        {
            if( Utils.Hardware.RebootUserPrompt )
            {
                Log?.Invoke(Diagnostics.EventLogEntryCodes.SystemShutdown, null);
                DoSystemRestart?.Invoke();
            }
        }
    }
}
