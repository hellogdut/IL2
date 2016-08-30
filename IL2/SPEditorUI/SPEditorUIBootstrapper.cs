using CocoStudio.EngineAdapterWrap;
using EditorCommon;
using EditorCommon.Behaviors;
using EditorCommon.Interface.LayoutMgr;
using EditorCommon.Manager;
using EditorCommon.Manager.AutoSave;
using EditorCommon.Manager.LayoutMgr;
using EditorProxy.Interface;
using GameEngineInterface;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.MefExtensions;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Modules.Communal.AnimationAggregationFrames;
using Modules.Communal.Guide.Model;
using Modules.Communal.Guide.View;
using Modules.Communal.MultiLanguage;
using Modules.Communal.Output.View;
using Modules.Communal.PropertyGrid.View;
using Modules.Communal.Render.View;
using Modules.Communal.Resource.View;
using Modules.Communal.StartPage.View;
using Modules.Communal.Status.View;
using Modules.Communal.UIAnimation;
using Modules.UI.ComTool.View;
using Modules.UI.MainTool.View;
using Modules.UI.Menu.View;
using Modules.UI.ProjectResource.View;
using Modules.UI.RenderContextMenu;
using Modules.UI.StructTree.View;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using System.Windows;
using WPFUndoManager;
namespace SPEditorUI
{
	internal class SPEditorUIBootstrapper : MefBootstrapper
	{
		private readonly LoggerWarp loggerWarp = new LoggerWarp();
		protected override void InitializeShell()
		{
			base.InitializeShell();
			ServiceLocator.Current.GetInstance<ILayoutManager>().CombineViewPart();
			Application.Current.MainWindow = (Shell)base.Shell;
			Application.Current.MainWindow.Show();
			GlobalManager.Instance.ClearUndoRedoStack();
			if (GuideXMLHelp.GetGuideConfig() == true && GuideXMLHelp.GetImagePathList() != null)
			{
				new GuideUC
				{
					Title = LanguageInfo.guide_Title,
					Owner = Application.Current.MainWindow
				}.ShowDialog();
			}
		}
		protected override IRegionBehaviorFactory ConfigureDefaultRegionBehaviors()
		{
			IRegionBehaviorFactory regionBehaviorFactory = base.ConfigureDefaultRegionBehaviors();
			regionBehaviorFactory.AddIfMissing("AutoPopulateExportedViewsBehavior", typeof(AutoPopulateExportedViewsBehavior));
			return regionBehaviorFactory;
		}
		protected override DependencyObject CreateShell()
		{
			DependencyObject result;
			try
			{
				result = (DependencyObject)base.Container.GetExportedValue<IShell>();
			}
			catch (Exception ex)
			{
				this.loggerWarp.Log(ex.ToString(), Category.Exception, Priority.High);
				result = null;
			}
			return result;
		}
		protected override ILoggerFacade CreateLogger()
		{
			return this.loggerWarp;
		}
		protected override void ConfigureAggregateCatalog()
		{
			base.ConfigureAggregateCatalog();
			FactoryHelp.RegisterFactoryType(typeof(ProxyFactory));
			this.LoadPlugins();
			base.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(base.GetType().Assembly));
			base.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(CSObject).Assembly));
			base.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(GlobalManager).Assembly));
			base.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(UndoMonitor).Assembly));
			base.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(AutoSaveManager).Assembly));
			base.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(ObjectContextMenuWPF).Assembly));
			base.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(StatusUC).Assembly));
			base.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(OutputUC).Assembly));
			base.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(PropertyUC).Assembly));
			base.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(RenderUC).Assembly));
			base.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(ResourceUC).Assembly));
			base.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(ComToolUC).Assembly));
			base.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(MenuUC).Assembly));
			base.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(MainToolUC).Assembly));
			base.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(StructTreeUC).Assembly));
			base.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(AnimationUC).Assembly));
			base.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(AnimationFrameUC).Assembly));
			base.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(ProjectResourceUC).Assembly));
			base.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(StartPageUC).Assembly));
			base.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(AnimationAggregationFramesUC).Assembly));
		}
		protected override void ConfigureContainer()
		{
			base.ConfigureContainer();
		}
		private void LoadPlugins()
		{
			List<Assembly> list = Option.LoadPlugins();
			foreach (Assembly current in list)
			{
				base.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(current));
			}
		}
	}
}
