using Microsoft.Practices.Prism.ViewModel;
using System;
using System.Windows.Media;
namespace Modules.UI.ProjectResource.View
{
	public class CanvasStateFile : NotificationObject
	{
		private Brush _canvasBrush;
		public string Name
		{
			get;
			set;
		}
		public string FileName
		{
			get;
			set;
		}
		public bool IsSelected
		{
			get;
			set;
		}
		public Brush CanvasBrush
		{
			get
			{
				return this._canvasBrush;
			}
			set
			{
				this._canvasBrush = value;
				base.RaisePropertyChanged<Brush>(() => this.CanvasBrush);
			}
		}
		public CanvasStateFile()
		{
		}
		public CanvasStateFile(string name, string fileName)
		{
			this.Name = name;
			this.FileName = fileName;
			BrushConverter brushConverter = new BrushConverter();
			this.CanvasBrush = (Brush)brushConverter.ConvertFromString("White");
		}
	}
}
