using AvalonDock;
using AvalonDock.Layout;
using EditorCommon.Manager.LayoutMgr;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
namespace SPEditorUI.View
{
    public partial class  DockManagerUC : UserControl, IDefaultDockMgrUC, IComponentConnector
    {
        //internal DockingManager dockManager;
        //internal LayoutAnchorable UI_AnimationList;
        //internal LayoutAnchorable UI_KeyFrame;
        //private bool _contentLoaded;
        public DockingManager DockManager
        {
            get
            {
                return this.dockManager;
            }
        }
        public DockManagerUC()
        {
            this.InitializeComponent();
        }
        //[GeneratedCode("PresentationBuildTasks", "4.0.0.0"), DebuggerNonUserCode]
        //public void InitializeComponent()
        //{
        //    if (this._contentLoaded)
        //    {
        //        return;
        //    }
        //    this._contentLoaded = true;
        //    Uri resourceLocator = new Uri("/CocoStudioUIEditor;component/view/dockmanageruc.xaml", UriKind.Relative);
        //    Application.LoadComponent(this, resourceLocator);
        //}
        //[GeneratedCode("PresentationBuildTasks", "4.0.0.0"), EditorBrowsable(EditorBrowsableState.Never), DebuggerNonUserCode]
        //void IComponentConnector.Connect(int connectionId, object target)
        //{
        //    switch (connectionId)
        //    {
        //        case 1:
        //            this.dockManager = (DockingManager)target;
        //            return;
        //        case 2:
        //            this.UI_AnimationList = (LayoutAnchorable)target;
        //            return;
        //        case 3:
        //            this.UI_KeyFrame = (LayoutAnchorable)target;
        //            return;
        //        default:
        //            this._contentLoaded = true;
        //            return;
        //    }
        //}
    }
}
