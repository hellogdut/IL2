using Editor.ControlLib.OpenEdiotr;
using EditorCommon.Manager;
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
namespace SPEditorUI
{
    public partial class App : System.Windows.Application
    {
        public static EditMemory<EditMemoryFile> EditMemory;
        private bool _contentLoaded;
        public App()
        {
            Option.SetCurrentIDE(EnumEditorIDE.UI);
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            System.Windows.Forms.Application.EnableVisualStyles();
            App.Start();
            base.ShutdownMode = ShutdownMode.OnMainWindowClose;
            if (base.MainWindow != null)
            {
                base.MainWindow.DataContextChanged += new DependencyPropertyChangedEventHandler(this.MainWindow_DataContextChanged);
            }
            this.JudgeOpenProject(e);
        }
        private void JudgeOpenProject(StartupEventArgs e)
        {
            App.EditMemory = new EditMemory<EditMemoryFile>("CocoStudio");
            string text = (e.Args.Length > 0) ? e.Args[0] : string.Empty;
            if (!string.IsNullOrWhiteSpace(text) && File.Exists(text))
            {
                bool flag = true;
                EditFile editFile = null;
                EditMemoryFile editMemoryFile = App.EditMemory.LoadMemoryFile();
                if (editMemoryFile != null)
                {
                    EditInfo editInfo = editMemoryFile.Projects.FirstOrDefault((EditInfo i) => i.EditName == Enum.GetName(typeof(EnumEditorIDE), Option.CurrentEditorIDE));
                    if (editInfo != null)
                    {
                        this.JudgeExsitEdit(editMemoryFile, editInfo);
                        FileInfo fileInfo = new FileInfo(text);
                        string newfilename = fileInfo.FullName;
                        if (string.Equals(fileInfo.Extension, ".ui", StringComparison.InvariantCultureIgnoreCase))
                        {
                            newfilename = newfilename.Replace(".xml.ui", "");
                        }
                        else
                        {
                            if (string.Equals(fileInfo.Extension, ".xml", StringComparison.InvariantCultureIgnoreCase))
                            {
                                newfilename = newfilename.Replace(".xml", "");
                            }
                        }
                        editFile = editInfo.Projects.FirstOrDefault((EditFile i) => i.ProjectPath == newfilename);
                        if (editFile != null && editFile.ProjectPath == newfilename)
                        {
                            flag = false;
                        }
                    }
                }
                if (flag)
                {
                    ProjectOption.OpenProject(text, true, true);
                    return;
                }
                if (editFile != null)
                {
                    EdiotrHelper.ShowWindow(editFile.Handle);
                    System.Windows.Application.Current.Dispatcher.InvokeShutdown();
                }
            }
        }
        private void MainWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Window window = sender as Window;
            if (window != null && App.EditMemory != null)
            {
                IntPtr hwnd = new WindowInteropHelper(window).Handle;
                EditMemoryFile editMemoryFile = App.EditMemory.LoadMemoryFile();
                EditInfo editInfo;
                if (editMemoryFile == null)
                {
                    string projectPath = Path.Combine(ProjectOption.CurrentProject.ProjectDir, e.NewValue.ToString());
                    editMemoryFile = new EditMemoryFile
                    {
                        MemoryFileInfo = "CocoStudio"
                    };
                    editInfo = new EditInfo(Enum.GetName(typeof(EnumEditorIDE), Option.CurrentEditorIDE));
                    EditFile editFile = new EditFile();
                    editFile.Handle = hwnd;
                    editFile.ProjectPath = projectPath;
                    editInfo.Projects.Add(editFile);
                    editMemoryFile.Projects.Add(editInfo);
                    App.EditMemory.SaveMemoryFile(editMemoryFile);
                    return;
                }
                editInfo = editMemoryFile.Projects.FirstOrDefault((EditInfo i) => i.EditName == Enum.GetName(typeof(EnumEditorIDE), Option.CurrentEditorIDE));
                if (editInfo != null)
                {
                    this.JudgeExsitEdit(editMemoryFile, editInfo);
                    if (e.OldValue != null)
                    {
                        EditFile editFile2 = editInfo.Projects.FirstOrDefault((EditFile i) => i.Handle == hwnd);
                        if (editFile2 != null)
                        {
                            FileInfo fileInfo = new FileInfo(editFile2.ProjectPath);
                            if (fileInfo.Name.Equals(e.OldValue.ToString()))
                            {
                                editInfo.Projects.Remove(editFile2);
                            }
                        }
                    }
                    if (e.NewValue != null)
                    {
                        string newFullPath = Path.Combine(ProjectOption.CurrentProject.ProjectDir, e.NewValue.ToString());
                        EditFile editFile = editInfo.Projects.FirstOrDefault((EditFile i) => i.ProjectPath == newFullPath);
                        if (editFile != null)
                        {
                            EdiotrHelper.ShowWindow(editFile.Handle);
                            System.Windows.Application.Current.Dispatcher.InvokeShutdown();
                        }
                        else
                        {
                            editFile = new EditFile();
                            editFile.Handle = hwnd;
                            editFile.ProjectPath = newFullPath;
                            editInfo.Projects.Add(editFile);
                        }
                    }
                    App.EditMemory.SaveMemoryFile(editMemoryFile);
                    return;
                }
                if (e.NewValue != null)
                {
                    string projectPath2 = Path.Combine(ProjectOption.CurrentProject.ProjectDir, e.NewValue.ToString() + ".xml.ui");
                    editInfo = new EditInfo(Enum.GetName(typeof(EnumEditorIDE), Option.CurrentEditorIDE));
                    EditFile editFile = new EditFile();
                    editFile.Handle = hwnd;
                    editFile.ProjectPath = projectPath2;
                    editInfo.Projects.Add(editFile);
                    editMemoryFile.Projects.Add(editInfo);
                    App.EditMemory.SaveMemoryFile(editMemoryFile);
                }
            }
        }
        public void JudgeExsitEdit(EditMemoryFile editMomoryFile, EditInfo editInfo)
        {
            if (editInfo != null && editInfo.Projects.Count > 0)
            {
                int count = editInfo.Projects.Count;
                int i = count - 1;
                while (i >= 0)
                {
                    EditFile editFile = editInfo.Projects[i];
                    if (editFile.ProjectPath == null)
                    {
                        goto IL_9F;
                    }
                    FileInfo fileInfo = new FileInfo(editFile.ProjectPath);
                    string windowText = EdiotrHelper.GetWindowText(editFile.Handle);
                    if (!windowText.Contains("--"))
                    {
                        goto IL_9F;
                    }
                    string text = windowText.Split(new string[]
                    {
                        "--"
                    }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault<string>();
                    if (text == null || !(text == fileInfo.Name))
                    {
                        goto IL_9F;
                    }
                IL_AC:
                    i--;
                    continue;
                IL_9F:
                    editInfo.Projects.Remove(editFile);
                    goto IL_AC;
                }
                App.EditMemory.SaveMemoryFile(editMomoryFile);
            }
        }
        private static void Start()
        {
            SPEditorUIBootstrapper sPEditorUIBootstrapper = new SPEditorUIBootstrapper();
            sPEditorUIBootstrapper.Run();
        }
        [GeneratedCode("PresentationBuildTasks", "4.0.0.0"), DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
            {
                return;
            }
            this._contentLoaded = true;
            Uri resourceLocator = new Uri("/CocoStudioUIEditor;component/app.xaml", UriKind.Relative);
            System.Windows.Application.LoadComponent(this, resourceLocator);
        }
        [GeneratedCode("PresentationBuildTasks", "4.0.0.0"), DebuggerNonUserCode, STAThread]
        public static void Main()
        {
            App app = new App();
            app.InitializeComponent();
            app.Run();
        }
    }
}
