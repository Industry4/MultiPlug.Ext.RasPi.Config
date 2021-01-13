using System.Reflection;
using MultiPlug.Ext.RasPi.Config.Models.Components.About;

namespace MultiPlug.Ext.RasPi.Config.Components.About
{
    internal class AboutComponent : AboutProperties
    {
        internal AboutComponent RepopulateAndGetProperties()
        {
            var ExecutingAssembly = Assembly.GetExecutingAssembly();

            Title = ExecutingAssembly.GetCustomAttribute<AssemblyTitleAttribute>().Title;
            Description = ExecutingAssembly.GetCustomAttribute<AssemblyDescriptionAttribute>().Description;
            Company = ExecutingAssembly.GetCustomAttribute<AssemblyCompanyAttribute>().Company;
            Product = ExecutingAssembly.GetCustomAttribute<AssemblyProductAttribute>().Product;
            Copyright = ExecutingAssembly.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;
            Trademark = ExecutingAssembly.GetCustomAttribute<AssemblyTrademarkAttribute>().Trademark;
            Version = ExecutingAssembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version;

            return this;
        }
    }
}
