using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

using DiskReader.Properties;

namespace DiskReader
{
	public partial class Form1 : Form
	{
		private DriveInfo driveInfo;

		public Form1()
		{
			this.InitializeComponent();

			this.InitializeControls();

			this.driveInfo = DriveInfo.GetDrives().FirstOrDefault(d => d.DriveType == DriveType.CDRom);

			this.comboBox1.SelectedIndexChanged +=
				(s, e) =>
				{
					this.driveInfo =
						DriveInfo.GetDrives()
						         .FirstOrDefault(d => d.Name == ((DriveInfo) this.comboBox1.SelectedItem).Name);

					this.ShowDriveInfoStatus();
				};

			this.button1.Click +=
				(s, e) =>
				{
					this.toolStripProgressBar1.Visible = true;
					var thread = new Thread(() =>
					                        {
						                        this.ActionInvoker(this.treeView1.Nodes.Clear);
						                        this.ShowDriveInfoStatus();

						                        if (this.driveInfo.IsReady)
						                        {
							                        if (!this.checkBox1.Checked)
								                        this.ActionInvoker(this.treeView1.BeginUpdate);
							                        this.BuildTree(this.driveInfo.RootDirectory, this.treeView1.Nodes);
													if (!this.checkBox1.Checked)
														this.ActionInvoker(this.treeView1.EndUpdate);
						                        }

												if (this.checkBox1.Checked)
													this.ActionInvoker(this.treeView1.ExpandAll);
						                        this.ActionInvoker(() => this.toolStripProgressBar1.Visible = false);
					                        });
					thread.Start();
				};

			this.comboBox1.SelectedItem = this.driveInfo;

			this.ShowDriveInfoStatus();
		}

		private void InitializeControls()
		{
			this.toolStripProgressBar1.Visible = false;
			this.Text = Resources.Form_Title_DiskReader;
			this.label1.Text = Resources.Label_Title_FocusOnDisk;
			this.button1.Text = Resources.Button_Title_Reload;
			this.checkBox1.Text = Resources.CheckBox_Title_DirectoriesOnly;
			this.checkBox1.Checked = true;
			this.comboBox1.DataSource =
				DriveInfo.GetDrives()
				         .Where(d => d.DriveType == DriveType.CDRom)
				         .ToList();
			this.imageList1.Images.Clear();
			this.imageList1.Images.Add("folder", Resources.Folder);
			this.treeView1.ImageList = this.imageList1;
		}

		private void ShowDriveInfoStatus()
		{
			this.toolStripStatusLabel1.Text = this.driveInfo == null
				? string.Empty
				: $"{this.driveInfo.Name} [{(this.driveInfo.IsReady ? this.driveInfo.VolumeLabel : Resources.DiskSatatus_InsertDisk)}]";
		}

		private void BuildTree(DirectoryInfo directoryInfo, TreeNodeCollection treeNodeCollection)
		{
			var currentNode = this.FuncInvoker(() => treeNodeCollection.Add(directoryInfo.FullName, directoryInfo.Name, "folder"));

			if(!this.checkBox1.Checked)
				foreach (var fileInfo in directoryInfo.GetFiles())
				{
					var info = fileInfo;
					if (!this.imageList1.Images.ContainsKey(info.Extension))
						this.ActionInvoker(() => this.imageList1.Images.Add(info.Extension,
						                                                    Icon.ExtractAssociatedIcon(info.FullName) ??
						                                                    SystemIcons.WinLogo));

					this.ActionInvoker(() => currentNode.Nodes.Add(info.FullName, info.Name, info.Extension, info.Extension));
				}

			foreach (var subdir in directoryInfo.GetDirectories())
				this.BuildTree(subdir, currentNode.Nodes);
		}

		#region Methods invokers

		private void ActionInvoker(Action action)
		{
			if (action == null) return;
			this.Invoke(action);
		}

		private T FuncInvoker<T>(Func<T> func)
		{
			if (func == null) return default (T);
			return (T) this.Invoke(func);
		}

		#endregion
	}
}