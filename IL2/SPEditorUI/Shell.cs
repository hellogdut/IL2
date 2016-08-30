using EditorCommon;
using EditorCommon.Event;
using EditorCommon.Interface.LayoutMgr;
using EditorCommon.Manager.LayoutMgr;
using EditorCommon.ViewModel.Component.GUI;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.ServiceLocation;
using Modules.Communal.MultiLanguage;
using SPEditorUI.View;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
namespace SPEditorUI
{
    [Export(typeof(IShell))]
    public partial class Shell : Window, IShell, IComponentConnector
    {
        private bool isFirst;
        private Widget widget;
        internal Grid gridMenu;
        internal Grid gridMainTool;
        internal Grid gridDockManager;
        internal Grid gridStatus;
        private bool _contentLoaded;
        public Shell()
        {
            this.InitializeComponent();
            IEventAggregator instance = ServiceLocator.Current.GetInstance<IEventAggregator>();
            instance.GetEvent<ProjectChangeEvent>().Subscribe(new Action<ProjectChangeEventArgs>(this.ProjectChangeEventHandle));
            base.Loaded += new RoutedEventHandler(this.Shell_Loaded);
            base.Title = LanguageInfo.Title_UIEditor;
        }
        private void Shell_Loaded(object sender, RoutedEventArgs e)
        {
            //delegate
            //{
            AddEditAction.AddEditorUsed("UIEditor");
            //}.BeginInvoke(null, null);
            ServiceLocator.Current.GetInstance<ILayoutManager>().HideAnchorView("UI_AnimationList");
            ServiceLocator.Current.GetInstance<ILayoutManager>().HideAnchorView("UI_KeyFrame");
        }
        public IDefaultDockMgrUC GetDefaultDockManagerUC()
        {
            return new DockManagerUC();
        }
        public void AddDockManager(Control control)
        {
            this.gridDockManager.Children.Clear();
            this.gridDockManager.Children.Add(control);
        }
        private void ProjectChangeEventHandle(ProjectChangeEventArgs args)
        {
            if (args != null && args.Project != null)
            {
                if (args.ChangeType == EnumProjectChangeType.Close)
                {
                    base.Title = LanguageInfo.Title_UIEditor;
                    return;
                }
                base.DataContext = args.Project.Name;
                base.Title = args.Project.Name + "--" + LanguageInfo.Title_UIEditor;
            }
        }
        [GeneratedCode("PresentationBuildTasks", "4.0.0.0"), DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
            {
                return;
            }
            this._contentLoaded = true;
            Uri resourceLocator = new Uri("/CocoStudioUIEditor;component/shell.xaml", UriKind.Relative);
            Application.LoadComponent(this, resourceLocator);
        }
        [GeneratedCode("PresentationBuildTasks", "4.0.0.0"), EditorBrowsable(EditorBrowsableState.Never), DebuggerNonUserCode]
        void IComponentConnector.Connect(int connectionId, object target)
        {
            switch (connectionId)
            {
                case 1:
                    this.gridMenu = (Grid)target;
                    return;
                case 2:
                    this.gridMainTool = (Grid)target;
                    return;
                case 3:
                    this.gridDockManager = (Grid)target;
                    return;
                case 4:
                    this.gridStatus = (Grid)target;
                    return;
                default:
                    this._contentLoaded = true;
                    return;
            }
        }
        //void IShell.add_Closing(CancelEventHandler value)
        //{
        //    base.Closing += value;
        //}
        //void IShell.remove_Closing(CancelEventHandler value)
        //{
        //    base.Closing -= value;
        //}
    }
}
