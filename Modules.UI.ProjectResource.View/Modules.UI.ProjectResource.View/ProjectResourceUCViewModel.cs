using EditorCommon.Editor;
using EditorCommon.Event;
using EditorCommon.Interface.ObjectContextMenu;
using EditorCommon.JsonModel;
using EditorCommon.Manager;
using EditorCommon.Manager.ProjectXml;
using EditorCommon.ViewModel;
using EditorCommon.ViewModel.Component.GUI;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows.Controls;
namespace Modules.UI.ProjectResource.View
{
	[Export(typeof(ProjectResourceUCViewModel))]
	internal class ProjectResourceUCViewModel : NotificationObject
	{
		private UIProject CurrentUIProject;
		private ComGameObject CurrentObejct;
		private IEventAggregator eventAggregator;
		private ContextMenu objectContextMenu;
		public string JsonSuffix = ".json";
		public List<CanvasStateFile> TreeListBoxItems
		{
			get;
			private set;
		}
		public ContextMenu ObjectContextMenu
		{
			get
			{
				return this.objectContextMenu;
			}
		}
		public FileSystemInfo Info
		{
			get;
			private set;
		}
		[ImportingConstructor]
		public ProjectResourceUCViewModel(IEventAggregator eventAggregator, IObjectContextMenuWPF contextMenu)
		{
			this.eventAggregator = eventAggregator;
			this.objectContextMenu = new ContextMenu();
			this.eventAggregator.GetEvent<ProjectChangeEvent>().Subscribe(new Action<ProjectChangeEventArgs>(this.ProjectChangeEventHandle));
		}
		internal void AddNewCanvas()
		{
			this.CurrentObejct.Visiable = false;
			this.CurrentUIProject.CreateNewJson("");
			this.ReloadTreeItems();
			this.eventAggregator.GetEvent<CanvasChangeEvent>().Publish(new CanvasChangeEventArgs(this.CurrentUIProject.CurrentUIJson, string.Empty, EnumCanvasChangeType.New));
		}
		internal void SwitchCanvas(object file)
		{
			GlobalManager.Instance.TaskService.IsEnable = false;
			CanvasStateFile canvasStateFile = file as CanvasStateFile;
			if (this.CurrentUIProject != null && canvasStateFile != null)
			{
				if (this.CurrentObejct != null)
				{
					this.CurrentObejct.Visiable = false;
					if (this.CurrentObejct.ComponentList.Count > 0)
					{
						ComGUI comGUI = this.CurrentObejct.ComponentList[0] as ComGUI;
						if (comGUI != null && comGUI.RootGUIControl != null)
						{
							comGUI.ResetSelected(comGUI.RootGUIControl.ChildrenPart);
						}
					}
					this.CurrentObejct = null;
				}
				CanvasGameObject canvasObject = GlobalManager.Instance.CanvasObject;
				for (int i = 0; i < canvasObject.ChildrenList.Count; i++)
				{
					ComGameObject comGameObject = canvasObject.ChildrenList[i];
					comGameObject.Visiable = false;
					if (comGameObject != null && comGameObject.Name != null && comGameObject.Name.Equals(canvasStateFile.FileName))
					{
						comGameObject.Visiable = true;
						this.CurrentObejct = comGameObject;
						break;
					}
				}
				if (this.CurrentObejct == null)
				{
					this.CurrentObejct = JsonFileHelp.CreateObjectWithUI(ProjectOption.GetJsonFullPath(canvasStateFile.FileName));
					((IComResourceUser)this.CurrentObejct).InitResouceReference(ProjectOption.ResourceManager);
				}
				else
				{
					ComGUI comGUI = this.CurrentObejct.ComponentList[0] as ComGUI;
					if (comGUI == null)
					{
						return;
					}
					GlobalManager.Instance.IRenderUC.RootVisualObject = comGUI;
				}
				this.CurrentObejct.Visiable = true;
				this.CurrentUIProject.CurrentUIJson = canvasStateFile.FileName;
				GlobalManager.Instance.TaskService.IsEnable = true;
				ProjectOption.RaiseProjectChange(EnumProjectChangeType.SwitchCanvas, true);
				this.eventAggregator.GetEvent<CanvasChangeEvent>().Publish(new CanvasChangeEventArgs(this.CurrentUIProject.CurrentUIJson, string.Empty, EnumCanvasChangeType.Switch));
			}
		}
		public void PasteCanvas(string jsoncopylist)
		{
			if (jsoncopylist != null)
			{
				string jsonFullPath = ProjectOption.GetJsonFullPath(jsoncopylist);
				if (File.Exists(jsonFullPath))
				{
					this.CurrentUIProject.SaveProject();
					this.CurrentObejct.Visiable = false;
					string fileName = this.CurrentUIProject.CreateNewJson(this.CurrentUIProject.CurrentUIJson);
					File.Copy(jsonFullPath, ProjectOption.GetJsonFullPath(fileName));
					this.ReloadTreeItems();
				}
			}
		}
		public void DeleteCanvas()
		{
			string fileName = Path.GetFileName(this.CurrentUIProject.CurrentUIJson);
			this.CurrentUIProject.JsonList.Remove(fileName);
			string currentUIJson = this.CurrentUIProject.CurrentUIJson;
			this.CurrentUIProject.CurrentUIJson = "";
			CanvasGameObject canvasObject = GlobalManager.Instance.CanvasObject;
			canvasObject.ChildrenList.Remove(this.CurrentObejct);
			this.eventAggregator.GetEvent<CanvasChangeEvent>().Publish(new CanvasChangeEventArgs(currentUIJson, string.Empty, EnumCanvasChangeType.Delete));
			this.ReloadTreeItems();
			string jsonFullPath = ProjectOption.GetJsonFullPath(fileName);
			if (File.Exists(jsonFullPath))
			{
				File.Delete(ProjectOption.GetJsonFullPath(fileName));
			}
		}
		public void RenameCanvas(string oldName, string newName)
		{
			UIProject uIProject = ProjectOption.CurrentProject as UIProject;
			string text = newName + this.JsonSuffix;
			string text2 = oldName + this.JsonSuffix;
			string destFileName = Path.Combine(uIProject.JsonFolder, text);
			string text3 = Path.Combine(uIProject.JsonFolder, text2);
			for (int i = 0; i < uIProject.JsonList.Count; i++)
			{
				if (uIProject.JsonList[i].Equals(text2))
				{
					uIProject.CurrentUIJson = text;
					uIProject.JsonList[i] = uIProject.CurrentUIJson;
				}
			}
			for (int i = 0; i < this.TreeListBoxItems.Count; i++)
			{
				CanvasStateFile canvasStateFile = this.TreeListBoxItems[i];
				if (canvasStateFile.FileName == text2)
				{
					canvasStateFile.Name = newName;
					canvasStateFile.IsSelected = true;
					canvasStateFile.FileName = text;
				}
			}
			this.RenameGameObject(text2, text);
			this.eventAggregator.GetEvent<CanvasChangeEvent>().Publish(new CanvasChangeEventArgs(text, text2, EnumCanvasChangeType.Rename));
			if (File.Exists(text3))
			{
				File.Move(text3, destFileName);
			}
			uIProject.SaveProject();
		}
		public bool RenameGameObject(string oldName, string newName)
		{
			CanvasGameObject canvasGameObject = GlobalManager.Instance.IRenderUC.GetCanvasGameObject();
			bool flag = false;
			bool result;
			for (int i = 0; i < canvasGameObject.ChildrenList.Count; i++)
			{
				if (canvasGameObject.ChildrenList[i].Name != null && canvasGameObject.ChildrenList[i].Name.Trim() == oldName.Trim())
				{
					canvasGameObject.ChildrenList[i].Name = newName;
					result = true;
					return result;
				}
			}
			result = flag;
			return result;
		}
		private void ProjectChangeEventHandle(ProjectChangeEventArgs args)
		{
			if (args.Project != null)
			{
				switch (args.ChangeType)
				{
				case EnumProjectChangeType.New:
				case EnumProjectChangeType.Open:
					this.CurrentUIProject = (ProjectOption.CurrentProject as UIProject);
					this.ReloadTreeItems();
					this.SwitchCanvas(this.TreeListBoxItems.Last<CanvasStateFile>());
					break;
				case EnumProjectChangeType.Close:
					this.CurrentUIProject = (args.Project as UIProject);
					this.ReloadTreeItems();
					break;
				}
			}
			base.RaisePropertyChanged<List<CanvasStateFile>>(() => this.TreeListBoxItems);
		}
		private void ReloadTreeItems()
		{
			this.TreeListBoxItems = new List<CanvasStateFile>();
			if (this.CurrentUIProject.JsonList.Count != 0)
			{
				foreach (string current in this.CurrentUIProject.JsonList)
				{
					CanvasStateFile item = new CanvasStateFile(Path.GetFileNameWithoutExtension(current), current);
					this.TreeListBoxItems.Add(item);
				}
			}
			if (this.TreeListBoxItems.Count > 0)
			{
				this.TreeListBoxItems.Last<CanvasStateFile>().IsSelected = true;
			}
			base.RaisePropertyChanged<List<CanvasStateFile>>(() => this.TreeListBoxItems);
		}
	}
}
