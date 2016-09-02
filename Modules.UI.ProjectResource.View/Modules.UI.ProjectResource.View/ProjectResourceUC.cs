using EditorCommon.Behaviors;
using EditorCommon.Manager;
using EditorCommon.Manager.ProjectXml;
using Modules.Communal.MultiLanguage;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;
namespace Modules.UI.ProjectResource.View
{
	[ViewExport(RegionName = "UI_ProjectResource"), PartCreationPolicy(CreationPolicy.Shared)]
	public partial class ProjectResourceUC : UserControl, IComponentConnector, IStyleConnector
	{
		private MenuItem menuItemAddComponent;
		private MenuItem menuItemDeleteComponent;
		private MenuItem menuItemCopyComponent;
		private MenuItem menuItemPasteComponent;
		private MenuItem menuItemRenameComponent;
		private string _currentEditingName = string.Empty;
		private bool isEditState = false;
		private TextBox m_ItemTextBox;
		private TextBlock m_ItemTextBlock;
		private string inEditingName = null;
		private CanvasStateFile oldSeletedObject;
		private BrushConverter brushConverter = new BrushConverter();
		private string jsoncopylist;
		private CanvasStateFile SelectCanvas = null;
		//internal ListBox multiSelectTree;
		//private bool _contentLoaded;
		public RoutedUICommand RenameShortcutKey
		{
			get;
			private set;
		}
		[Import]
		private ProjectResourceUCViewModel ViewModel
		{
			get
			{
				return (ProjectResourceUCViewModel)base.DataContext;
			}
			set
			{
				base.DataContext = value;
				this.InitMenuItem();
			}
		}
		public ProjectResourceUC()
		{
			this.InitializeComponent();
			this.InitEvent();
		}
		private void InitEvent()
		{
			this.multiSelectTree.SelectionChanged += new SelectionChangedEventHandler(this.multiSelectTree_SelectionChanged);
			this.multiSelectTree.PreviewMouseDown += new MouseButtonEventHandler(this.multiSelectTree_PreviewMouseDown);
			this.multiSelectTree.MouseDoubleClick += new MouseButtonEventHandler(this.multiSelectTree_MouseDoubleClick);
			CommandBinding commandBinding = new CommandBinding(ApplicationCommands.Copy);
			commandBinding.Executed += new ExecutedRoutedEventHandler(this.CopyCmd_Executed);
			base.CommandBindings.Add(commandBinding);
			CommandBinding commandBinding2 = new CommandBinding(ApplicationCommands.Paste);
			commandBinding2.Executed += new ExecutedRoutedEventHandler(this.PasteCmd_Executed);
			base.CommandBindings.Add(commandBinding2);
			CommandBinding commandBinding3 = new CommandBinding(ApplicationCommands.Delete);
			commandBinding3.Executed += new ExecutedRoutedEventHandler(this.DeleteCmd_Executed);
			base.CommandBindings.Add(commandBinding3);
			this.RenameShortcutKey = new RoutedUICommand();
			CommandBinding commandBinding4 = new CommandBinding(this.RenameShortcutKey);
			commandBinding4.Executed += new ExecutedRoutedEventHandler(this.menuItemRenameObject_Click);
			this.RenameShortcutKey.InputGestures.Add(new KeyGesture(Key.F2));
			base.CommandBindings.Add(commandBinding4);
		}
		private void InitMenuItem()
		{
			this.ViewModel.ObjectContextMenu.Items.Clear();
			this.menuItemAddComponent = this.CreatMenuItem(LanguageInfo.UI_ProjectContexMenu_NewCavas);
			this.menuItemAddComponent.Click += new RoutedEventHandler(this.menuItemAddObject_Click);
			this.menuItemRenameComponent = this.CreatMenuItem(LanguageInfo.UI_ProjectContexMenu_Rename);
			this.menuItemRenameComponent.Click += new RoutedEventHandler(this.menuItemRenameObject_Click);
			this.menuItemDeleteComponent = this.CreatMenuItem(LanguageInfo.UI_ProjectContexMenu_Del);
			this.menuItemDeleteComponent.Click += new RoutedEventHandler(this.menuItemDeleteObject_Click);
			this.menuItemCopyComponent = this.CreatMenuItem(LanguageInfo.UI_ProjectContexMenu_Copy);
			this.menuItemCopyComponent.Click += new RoutedEventHandler(this.menuItemCopyObject_Click);
			this.menuItemPasteComponent = this.CreatMenuItem(LanguageInfo.UI_ProjectContexMenu_Paste);
			this.menuItemPasteComponent.Click += new RoutedEventHandler(this.menuItemPasteObject_Click);
		}
		private MenuItem CreatMenuItem(string header)
		{
			MenuItem menuItem = new MenuItem();
			menuItem.Header = header;
			this.ViewModel.ObjectContextMenu.Items.Add(menuItem);
			return menuItem;
		}
		private void ReloadMenuItem()
		{
			if (ProjectOption.CurrentProject == null)
			{
				this.menuItemAddComponent.IsEnabled = false;
				this.menuItemRenameComponent.IsEnabled = false;
				this.menuItemDeleteComponent.IsEnabled = false;
				this.menuItemCopyComponent.IsEnabled = false;
				this.menuItemPasteComponent.IsEnabled = false;
			}
			else
			{
				this.menuItemAddComponent.IsEnabled = true;
				this.menuItemCopyComponent.IsEnabled = true;
				this.menuItemDeleteComponent.IsEnabled = true;
				this.menuItemRenameComponent.IsEnabled = true;
				if (this.jsoncopylist != null)
				{
					this.menuItemPasteComponent.IsEnabled = true;
				}
				else
				{
					this.menuItemPasteComponent.IsEnabled = false;
				}
			}
		}
		private void ListBoxItem_Loaded(object sender, RoutedEventArgs e)
		{
			ListBoxItem obj = sender as ListBoxItem;
			TextBox textBox = this.FindVisualChild<TextBox>(obj, "textBoxRename");
			if (textBox != null)
			{
				textBox.LostFocus += new RoutedEventHandler(this.txbNewName_LostFocus);
				textBox.KeyUp += new KeyEventHandler(this.txbNewName_KeyUp);
			}
		}
		private void CopyCmd_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			UIProject uIProject = ProjectOption.CurrentProject as UIProject;
			if (uIProject != null)
			{
				this.jsoncopylist = uIProject.CurrentUIJson;
			}
		}
		private void PasteCmd_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (this.jsoncopylist != null && ProjectOption.CurrentProject != null)
			{
				this.ViewModel.PasteCanvas(this.jsoncopylist);
			}
		}
		private void DeleteCmd_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			this.DeleteCanvasPrompt();
		}
		private void menuItemAddObject_Click(object sender, RoutedEventArgs e)
		{
			this.ViewModel.AddNewCanvas();
		}
		private void menuItemRenameObject_Click(object sender, RoutedEventArgs e)
		{
			if (this.multiSelectTree.SelectedItem != null)
			{
				DependencyObject parent = this.multiSelectTree.ItemContainerGenerator.ContainerFromItem(this.multiSelectTree.SelectedItem);
				this.ChangeToEditState(parent);
			}
		}
		private void menuItemCopyObject_Click(object sender, RoutedEventArgs e)
		{
			UIProject uIProject = ProjectOption.CurrentProject as UIProject;
			this.jsoncopylist = uIProject.CurrentUIJson;
		}
		private void menuItemPasteObject_Click(object sender, RoutedEventArgs e)
		{
			this.ViewModel.PasteCanvas(this.jsoncopylist);
		}
		private void menuItemDeleteObject_Click(object sender, RoutedEventArgs e)
		{
			this.DeleteCanvasPrompt();
		}
		private void multiSelectTree_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			this.ReloadMenuItem();
		}
		private void DeleteCanvasPrompt()
		{
			if (this.multiSelectTree.Items.Count > 1)
			{
				MessageBoxResult messageBoxResult = Xceed.Wpf.Toolkit.MessageBox.Show(LanguageInfo.MessageBox_Content105, LanguageInfo.MessageBox_Content5, MessageBoxButton.YesNo, MessageBoxImage.Asterisk);
				MessageBoxResult messageBoxResult2 = messageBoxResult;
				if (messageBoxResult2 != MessageBoxResult.Cancel)
				{
					if (messageBoxResult2 == MessageBoxResult.Yes)
					{
						this.ViewModel.DeleteCanvas();
					}
				}
			}
			else
			{
				Xceed.Wpf.Toolkit.MessageBox.Show(LanguageInfo.MessageBox_Content108, LanguageInfo.MessageBox_Content5, MessageBoxButton.OK, MessageBoxImage.Asterisk);
			}
		}
		private void multiSelectTree_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (this.multiSelectTree.SelectedItem != null)
			{
				DependencyObject parent = this.multiSelectTree.ItemContainerGenerator.ContainerFromItem(this.multiSelectTree.SelectedItem);
				this.ChangeToEditState(parent);
			}
		}
		private void multiSelectTree_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count == 1)
			{
				foreach (CanvasStateFile selectCanvas in e.AddedItems)
				{
					this.SelectCanvas = selectCanvas;
				}
			}
			if (this.multiSelectTree.SelectedItem == null)
			{
				this.multiSelectTree.SelectedItem = this.SelectCanvas;
			}
			else
			{
				ProjectOption.CurrentProject.SaveProject();
				DependencyObject obj = this.multiSelectTree.ItemContainerGenerator.ContainerFromItem(this.multiSelectTree.SelectedItem);
				TextBox itemTextBox = this.FindVisualChild<TextBox>(obj, "textBoxRename");
				TextBlock itemTextBlock = this.FindVisualChild<TextBlock>(obj, "textBlockName");
				this.m_ItemTextBox = itemTextBox;
				this.m_ItemTextBlock = itemTextBlock;
				CanvasStateFile canvasStateFile = this.multiSelectTree.SelectedItem as CanvasStateFile;
				if (this.oldSeletedObject != null)
				{
					this.oldSeletedObject.CanvasBrush = (Brush)this.brushConverter.ConvertFromString("White");
				}
				if (canvasStateFile != null)
				{
					canvasStateFile.CanvasBrush = (Brush)this.brushConverter.ConvertFromString("Black");
				}
				this.oldSeletedObject = canvasStateFile;
				this.ViewModel.SwitchCanvas(this.multiSelectTree.SelectedItem);
			}
		}
		private void txbNewName_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				this.Rename();
			}
		}
		private void txbNewName_LostFocus(object sender, RoutedEventArgs e)
		{
			this.Rename();
		}
		private void Rename()
		{
			if (this.isEditState && this.multiSelectTree.SelectedItem != null)
			{
				DependencyObject dependencyObject = this.multiSelectTree.ItemContainerGenerator.ContainerFromItem(this.multiSelectTree.SelectedItem);
				if (dependencyObject != null)
				{
					TextBox textBox = this.FindVisualChild<TextBox>(dependencyObject, "textBoxRename");
					TextBlock textBlock = this.FindVisualChild<TextBlock>(dependencyObject, "textBlockName");
					string text = this.m_ItemTextBox.Text;
					if (string.IsNullOrWhiteSpace(text))
					{
						this.ErrorRename(LanguageInfo.MessageBox_Content103);
					}
					else
					{
						if (this._currentEditingName == text)
						{
							this.ErrorRename(null);
						}
						else
						{
							if (!this.ValidateName(text))
							{
								this.ErrorRename(string.Format(LanguageInfo.MessageBox_Content102, text));
							}
							else
							{
								if (Regex.IsMatch(text, "[\\*\\\\/:?<>|\"]"))
								{
									this.ErrorRename(LanguageInfo.MessageBox_Content104);
								}
								else
								{
									this.ViewModel.RenameCanvas(this.inEditingName, text);
									textBlock.Text = text;
									this.isEditState = false;
									textBox.Visibility = Visibility.Hidden;
									textBlock.Visibility = Visibility.Visible;
								}
							}
						}
					}
				}
			}
		}
		public bool ValidateName(string newName)
		{
			UIProject uIProject = ProjectOption.CurrentProject as UIProject;
			bool result = true;
			foreach (string current in uIProject.JsonList)
			{
				if (newName.Equals(Path.GetFileNameWithoutExtension(current)))
				{
					result = false;
					break;
				}
			}
			return result;
		}
		private void ErrorRename(string message = null)
		{
			if (message != null)
			{
				Xceed.Wpf.Toolkit.MessageBox.Show(message, LanguageInfo.MessageBox_Content5, MessageBoxButton.OK, MessageBoxImage.Exclamation);
			}
			this.m_ItemTextBlock.Text = this._currentEditingName;
			this.m_ItemTextBlock.Visibility = Visibility.Visible;
			this.m_ItemTextBox.Text = this._currentEditingName;
			this.m_ItemTextBox.Visibility = Visibility.Hidden;
		}
		private void ChangeToEditState(DependencyObject parent)
		{
			this.m_ItemTextBox = this.FindVisualChild<TextBox>(parent, "textBoxRename");
			this.m_ItemTextBlock = this.FindVisualChild<TextBlock>(parent, "textBlockName");
			this.m_ItemTextBlock.Visibility = Visibility.Hidden;
			this.m_ItemTextBox.Visibility = Visibility.Visible;
			this.m_ItemTextBox.SelectionLength = this.m_ItemTextBox.Text.Length;
			this.m_ItemTextBox.Focus();
			this._currentEditingName = this.m_ItemTextBlock.Text;
			this.inEditingName = (this.multiSelectTree.SelectedItem as CanvasStateFile).Name;
			this.isEditState = true;
		}
		private void ChangeToUneditState()
		{
			this.m_ItemTextBox.Visibility = Visibility.Hidden;
			this.m_ItemTextBlock.Visibility = Visibility.Visible;
		}
		public T FindVisualChild<T>(DependencyObject obj, string childName) where T : DependencyObject
		{
			int i = 0;
			T result;
			while (i < VisualTreeHelper.GetChildrenCount(obj))
			{
				DependencyObject child = VisualTreeHelper.GetChild(obj, i);
				if (child != null && child is T && child.GetValue(FrameworkElement.NameProperty).ToString() == childName)
				{
					result = (T)((object)child);
				}
				else
				{
					T t = this.FindVisualChild<T>(child, childName);
					if (t == null)
					{
						i++;
						continue;
					}
					result = t;
				}
				return result;
			}
			result = default(T);
			return result;
		}
        //[GeneratedCode("PresentationBuildTasks", "4.0.0.0"), DebuggerNonUserCode]
        //public void InitializeComponent()
        //{
        //    if (!this._contentLoaded)
        //    {
        //        this._contentLoaded = true;
        //        Uri resourceLocator = new Uri("/Modules.UI.ProjectResource;component/view/projectresourceuc.xaml", UriKind.Relative);
        //        Application.LoadComponent(this, resourceLocator);
        //    }
        //}
        //[GeneratedCode("PresentationBuildTasks", "4.0.0.0"), EditorBrowsable(EditorBrowsableState.Never), DebuggerNonUserCode]
        //void IComponentConnector.Connect(int connectionId, object target)
        //{
        //    if (connectionId != 1)
        //    {
        //        this._contentLoaded = true;
        //    }
        //    else
        //    {
        //        this.multiSelectTree = (ListBox)target;
        //    }
        //}
		[GeneratedCode("PresentationBuildTasks", "4.0.0.0"), EditorBrowsable(EditorBrowsableState.Never), DebuggerNonUserCode]
		void IStyleConnector.Connect(int connectionId, object target)
		{
			if (connectionId == 2)
			{
				EventSetter eventSetter = new EventSetter();
				eventSetter.Event = FrameworkElement.LoadedEvent;
				eventSetter.Handler = new RoutedEventHandler(this.ListBoxItem_Loaded);
				((Style)target).Setters.Add(eventSetter);
			}
		}
	}
}
