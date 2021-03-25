
namespace MultiPlug.Ext.RasPi.Config.Models.Components.About
{
    public class AboutProperties : SharedProperties
    {
        public object Version { get; internal set; }
        public string Company { get; internal set; }
        public string Copyright { get; internal set; }
        public string Description { get; internal set; }
        public string Product { get; internal set; }
        public string Title { get; internal set; }
        public string Trademark { get; internal set; }
        public string Log { get; internal set; }
    }
}
