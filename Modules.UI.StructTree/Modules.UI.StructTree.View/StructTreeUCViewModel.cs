using EditorCommon;
using EditorCommon.Event;
using EditorCommon.Interface.ObjectContextMenu;
using EditorCommon.Manager;
using EditorCommon.ViewModel.Component.GUI;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Controls;
namespace Modules.UI.StructTree.View
{
	[Export(typeof(StructTreeUCViewModel))]
	internal class StructTreeUCViewModel : NotificationObject
	{
		private IEventAggregator eventAggregator;
		private VisualObject rootVisualObject;
		private ContextMenu objectContextMenu;
		public List<VisualObject> RootVisualObject
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
		[ImportingConstructor]
		public StructTreeUCViewModel(IEventAggregator eventAggregator, IObjectContextMenuWPF contextMenu)
		{
			this.eventAggregator = eventAggregator;
			this.objectContextMenu = (contextMenu as ContextMenu);
			this.RootVisualObject = new List<VisualObject>();
			this.eventAggregator.GetEvent<SelectedVisualObjectsChangeEvent>().Subscribe(new Action<SelectedVisualObjectsChangeEventArgs>(this.OnSelectedVisualObjectChange));
			this.eventAggregator.GetEvent<ProjectChangeEvent>().Subscribe(new Action<ProjectChangeEventArgs>(this.ProjectChangeEventHandle));
		}
		private void OnSelectedVisualObjectChange(SelectedVisualObjectsChangeEventArgs args)
		{
			if (args.SelectedObject == null || this.rootVisualObject == null)
			{
				return;
			}
			foreach (VisualObject current in args.SelectedObject)
			{
				if (current is Widget)
				{
					this.ExpandParents(current as Widget);
				}
			}
		}
		private void ExpandParents(Widget part)
		{
			if (part.ParentPart != null)
			{
				part.ParentPart.IsExpanded = true;
				this.ExpandParents(part.ParentPart);
			}
		}
		private void ProjectChangeEventHandle(ProjectChangeEventArgs args)
		{
			this.RootVisualObject = new List<VisualObject>();
			if (args.Project != null && args.Project.ProjectType != EnumProjectType.GameProject && args.ChangeType != EnumProjectChangeType.Close)
			{
				ComGUI comGUI = GlobalManager.Instance.IRenderUC.RootVisualObject as ComGUI;
				if (comGUI != null)
				{
					this.rootVisualObject = comGUI.RootGUIControl;
					this.RootVisualObject.Add(this.rootVisualObject);
				}
				else
				{
					this.rootVisualObject = null;
				}
			}
			if (args.ChangeType != EnumProjectChangeType.Save)
			{
				base.RaisePropertyChanged<List<VisualObject>>(() => this.RootVisualObject);
			}
		}
		internal void RaiseSelecteItemsChange(List<VisualObject> objectList)
		{
			List<VisualObject> list = new List<VisualObject>();
			this.FilterConveredChildren(this.rootVisualObject, list);
			SelectedVisualObjectsChangeEvent @event = this.eventAggregator.GetEvent<SelectedVisualObjectsChangeEvent>();
			@event.Unsubscribe(new Action<SelectedVisualObjectsChangeEventArgs>(this.OnSelectedVisualObjectChange));
			@event.Publish(new SelectedVisualObjectsChangeEventArgs(objectList.AsReadOnly(), list.AsReadOnly(), false));
			@event.Subscribe(new Action<SelectedVisualObjectsChangeEventArgs>(this.OnSelectedVisualObjectChange));
		}
		private void FilterConveredChildren(VisualObject parentObject, List<VisualObject> selectedParentObject)
		{
			if (parentObject == null)
			{
				return;
			}
			if (parentObject.IsSelected)
			{
				selectedParentObject.Add(parentObject);
				return;
			}
			Widget widget = parentObject as Widget;
			if (widget == null)
			{
				return;
			}
			IEnumerable<VisualObject> visualChildren = widget.GetVisualChildren();
			if (visualChildren != null)
			{
				foreach (VisualObject current in visualChildren)
				{
					this.FilterConveredChildren(current, selectedParentObject);
				}
			}
		}
		internal void DeleteGameObject(List<VisualObject> objectList)
		{
			this.eventAggregator.GetEvent<DeleteVisualObjectsEvent>().Publish(objectList.AsReadOnly());
		}
		internal void CopyGameObject(List<VisualObject> objectList)
		{
			this.eventAggregator.GetEvent<CopyVisualObjectsEvent>().Publish(objectList.AsReadOnly());
		}
		internal void PasteGameObject()
		{
			this.eventAggregator.GetEvent<PasteVisualObjectsEvent>().Publish("粘贴");
		}
	}
}
