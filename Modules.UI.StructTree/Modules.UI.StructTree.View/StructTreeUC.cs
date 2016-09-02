using Editor.ControlLib.MultiSelectTree;
using EditorCommon;
using EditorCommon.Behaviors;
using Modules.Communal.MultiLanguage;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using Xceed.Wpf.Toolkit;
namespace Modules.UI.StructTree.View
{
	[ViewExport(RegionName = "UI_StructTree"), PartCreationPolicy(CreationPolicy.Shared)]
	public partial class StructTreeUC : UserControl, IComponentConnector, IStyleConnector
	{
		public static StructTreeUC Instance;
		private bool isClickBeforeSelectChange;
		//internal MultiSelectTree multiSelectTree;
		//private bool _contentLoaded;
		[Import]
		private StructTreeUCViewModel ViewModel
		{
			get
			{
				return (StructTreeUCViewModel)base.DataContext;
			}
			set
			{
				base.DataContext = value;
			}
		}
		public StructTreeUC()
		{
			this.InitializeComponent();
			this.Init();
			StructTreeUC.Instance = this;
		}
		private void Init()
		{
			this.multiSelectTree.ChildrenBinding = new Binding("ChildrenPart");
			this.multiSelectTree.IsExpandedPath = "IsExpanded";
			this.InitEvent();
		}
		private void InitEvent()
		{
			this.multiSelectTree.SelectionChanged += new SelectionChangedEventHandler(this.multiSelectTree_SelectionChanged);
			this.multiSelectTree.PreviewMouseDown += new MouseButtonEventHandler(this.multiSelectTree_PreviewMouseDown);
			CommandBinding commandBinding = new CommandBinding(ApplicationCommands.Delete);
			commandBinding.Executed += new ExecutedRoutedEventHandler(this.deleteCmd_Executed);
			base.CommandBindings.Add(commandBinding);
			CommandBinding commandBinding2 = new CommandBinding(ApplicationCommands.Copy);
			commandBinding2.Executed += new ExecutedRoutedEventHandler(this.CopyCmd_Executed);
			base.CommandBindings.Add(commandBinding2);
			CommandBinding commandBinding3 = new CommandBinding(ApplicationCommands.Paste);
			commandBinding3.Executed += new ExecutedRoutedEventHandler(this.PasteCmd_Executed);
			base.CommandBindings.Add(commandBinding3);
		}
		private void CopyCmd_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			List<VisualObject> objectList = this.multiSelectTree.SelectedItems.Cast<VisualObject>().ToList<VisualObject>();
			this.ViewModel.CopyGameObject(objectList);
		}
		private void PasteCmd_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			this.ViewModel.PasteGameObject();
		}
		private void deleteCmd_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			List<VisualObject> list = this.multiSelectTree.SelectedItems.Cast<VisualObject>().ToList<VisualObject>();
			if (list != null && list.Count > 0)
			{
				MessageBoxResult messageBoxResult = Xceed.Wpf.Toolkit.MessageBox.Show(LanguageInfo.MessageBox_Content21, LanguageInfo.MessageBox_Content5, MessageBoxButton.OKCancel, MessageBoxImage.None);
				if (messageBoxResult == MessageBoxResult.OK)
				{
					this.ViewModel.DeleteGameObject(list);
				}
			}
		}
		private void multiSelectTree_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			this.isClickBeforeSelectChange = true;
		}
		private void multiSelectTree_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			this.multiSelectTree.SelectionChanged -= new SelectionChangedEventHandler(this.multiSelectTree_SelectionChanged);
			if (this.isClickBeforeSelectChange || this.multiSelectTree.SelectedItems.Count == 0)
			{
				List<VisualObject> list = this.multiSelectTree.SelectedItems.Cast<VisualObject>().ToList<VisualObject>();
				for (int i = list.Count - 1; i >= 0; i--)
				{
					if (!list[i].CanEdit)
					{
						list[i].IsSelected = false;
						list.RemoveAt(i);
					}
				}
				this.ViewModel.RaiseSelecteItemsChange(list);
				this.isClickBeforeSelectChange = false;
			}
			this.multiSelectTree.SelectionChanged += new SelectionChangedEventHandler(this.multiSelectTree_SelectionChanged);
		}
		private void toggleButtonVisiable_Checked(object sender, RoutedEventArgs e)
		{
			this.SetToolTip(sender, "toggleButtonHide");
		}
		private void toggleButtonVisiable_Unchecked(object sender, RoutedEventArgs e)
		{
			this.SetToolTip(sender, "toggleButtonShow");
		}
		private void toggleButtonLock_Checked(object sender, RoutedEventArgs e)
		{
			this.SetToolTip(sender, "toggleButtonLock");
		}
		private void toggleButtonLock_Unchecked(object sender, RoutedEventArgs e)
		{
			this.SetToolTip(sender, "toggleButtonUnLock");
			this.multiSelectTree.SelectionChanged -= new SelectionChangedEventHandler(this.multiSelectTree_SelectionChanged);
			if (this.isClickBeforeSelectChange || this.multiSelectTree.SelectedItems.Count == 0)
			{
				List<VisualObject> list = this.multiSelectTree.SelectedItems.Cast<VisualObject>().ToList<VisualObject>();
				for (int i = list.Count - 1; i >= 0; i--)
				{
					if (!list[i].CanEdit)
					{
						list[i].IsSelected = false;
						list.RemoveAt(i);
					}
				}
				this.ViewModel.RaiseSelecteItemsChange(list);
				this.isClickBeforeSelectChange = false;
			}
			this.multiSelectTree.SelectionChanged += new SelectionChangedEventHandler(this.multiSelectTree_SelectionChanged);
		}
		private void SetToolTip(object sender, string info)
		{
			Control control = sender as Control;
			control.ToolTip = LanguageOption.GetValueBykey(info);
		}
    //    [GeneratedCode("PresentationBuildTasks", "4.0.0.0"), DebuggerNonUserCode]
    //    public void InitializeComponent()
    //    {
    //        if (this._contentLoaded)
    //        {
    //            return;
    //        }
    //        this._contentLoaded = true;
    //        Uri resourceLocator = new Uri("/Modules.UI.StructTree;component/view/structtreeuc.xaml", UriKind.Relative);
    //        Application.LoadComponent(this, resourceLocator);
    //    }
    //    [GeneratedCode("PresentationBuildTasks", "4.0.0.0"), EditorBrowsable(EditorBrowsableState.Never), DebuggerNonUserCode]
    //    void IComponentConnector.Connect(int connectionId, object target)
    //    {
    //        if (connectionId == 3)
    //        {
    //            this.multiSelectTree = (MultiSelectTree)target;
    //            return;
    //        }
    //        this._contentLoaded = true;
    //    }
        [GeneratedCode("PresentationBuildTasks", "4.0.0.0"), EditorBrowsable(EditorBrowsableState.Never), DebuggerNonUserCode]
        void IStyleConnector.Connect(int connectionId, object target)
        {
            switch (connectionId)
            {
                case 1:
                    ((ToggleButton)target).Checked += new RoutedEventHandler(this.toggleButtonVisiable_Checked);
                    ((ToggleButton)target).Unchecked += new RoutedEventHandler(this.toggleButtonVisiable_Unchecked);
                    return;
                case 2:
                    ((ToggleButton)target).Checked += new RoutedEventHandler(this.toggleButtonLock_Checked);
                    ((ToggleButton)target).Unchecked += new RoutedEventHandler(this.toggleButtonLock_Unchecked);
                    return;
                default:
                    return;
            }
        }
    }
}
