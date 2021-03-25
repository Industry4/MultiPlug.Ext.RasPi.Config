using System.Reflection;
using MultiPlug.Ext.RasPi.Config.Models.Components.About;
using MultiPlug.Base.Exchange.API;

namespace MultiPlug.Ext.RasPi.Config.Components.About
{
    internal class AboutComponent : AboutProperties
    {
        private ILoggingService m_LoggingService;
        internal AboutComponent(ILoggingService theLoggingService)
        {
            m_LoggingService = theLoggingService;
        }


        internal AboutProperties RepopulateAndGetProperties()
        {
            var ExecutingAssembly = Assembly.GetExecutingAssembly();

            Title = ExecutingAssembly.GetCustomAttribute<AssemblyTitleAttribute>().Title;
            Description = ExecutingAssembly.GetCustomAttribute<AssemblyDescriptionAttribute>().Description;
            Company = ExecutingAssembly.GetCustomAttribute<AssemblyCompanyAttribute>().Company;
            Product = ExecutingAssembly.GetCustomAttribute<AssemblyProductAttribute>().Product;
            Copyright = ExecutingAssembly.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;
            Trademark = ExecutingAssembly.GetCustomAttribute<AssemblyTrademarkAttribute>().Trademark;
            Version = ExecutingAssembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version;

            Log = string.Join("\r", m_LoggingService.Read());

            return this;
        }
    }
}
