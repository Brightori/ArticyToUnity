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
			switch (aIconName)
			{
				// if you have specified the "IconFile" in the PluginManifest.xml you don't need this case
				// unless you want to have an icon that differs when the plugin is loaded from the non-loaded case
				// or you want to put all icons within the resources of your plugin assembly
				/*
				case "$self":
					// get the main icon for the plugin
					return Session.CreateBrushFromFile(Manifest.ManifestPath+"Resources\\Icon.png");
				*/
			}
			return null;
		}
	}
}
