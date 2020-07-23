using Articy.Api;
using Articy.Api.Plugins;
using System.Collections.Generic;
using System.Windows.Media;

//using Texts = LIds.MyCompany.TestArticy;

namespace MyCompany.TestArticy
{
    /// <summary>
    /// public implementation part of plugin code, contains all overrides of the plugin class.
    /// </summary>
    public partial class Plugin : MacroPlugin
	{
		public ExportManager ExportManager;

		public override string DisplayName
		{
            get { return "MyCompany.TestArticy.Plugin.DisplayName"; }
		}

		public override string ContextName
		{
            get { return "MyCompany.TestArticy.Plugin.ContextName"; }
		}

        private void ConfigureTemplate(MacroCommandDescriptor aDescriptor, List<ObjectProxy> aSelectedobjects)
        {
			ExportManager = ExportManager ?? new ExportManager(Session);
			ExportManager.Export();
		}

		public override List<MacroCommandDescriptor> GetMenuEntries(List<ObjectProxy> aSelectedObjects, ContextMenuContext aContext )
		{
			var result = new List<MacroCommandDescriptor>();
			ExportManager = ExportManager ?? new ExportManager(Session);

			result.Add(new MacroCommandDescriptor()
            {
				CaptionLid = "Transfer to Unity",
				Execute = ConfigureTemplate
			});
			
			result.Add(new MacroCommandDescriptor()
            {
				CaptionLid = "Set Unity Project Folder",
				Execute = ExportManager.SetUnityFolderFolder
			});

			var flow = Session.RunQuery("SELECT * FROM Flow WHERE ObjectType=FlowFragment");
			var entitiesRoot = Session.GetSystemFolder(SystemFolderNames.Entities);

			var t = Session.GetSystemFolder(SystemFolderNames.Assets);

			foreach (var obj in flow.Rows)
			{
				var list = obj.GetChildren();
			}

			return result;
		}

        public override Brush GetIcon(string aIconName)
		{
			return null;
		}
	}
}
