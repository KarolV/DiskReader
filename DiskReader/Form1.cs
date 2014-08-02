using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using DiskReader.Properties;

namespace DiskReader
{
	public partial class Form1 : Form
	{
		private DriveInfo driveInfo;

		public Form1()
		{
			InitializeComponent();

			InitializeControls();

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
					this.treeView1.Nodes.Clear();
					this.ShowDriveInfoStatus();

					if (this.driveInfo.IsReady)
						BuildTree(this.driveInfo.RootDirectory, this.treeView1.Nodes);

					this.treeView1.ExpandAll();
				};

			this.comboBox1.SelectedItem = this.driveInfo;

			this.ShowDriveInfoStatus();
		}

		private void InitializeControls()
		{
			this.Text = Resources.Form_Title_DiskReader;
			this.label1.Text = Resources.Label_Title_FocusOnDisk;
			this.button1.Text = Resources.Button_Title_Reload;
			this.checkBox1.Text = Resources.CheckBox_Title_DirectoriesOnly;
			this.checkBox1.Checked = true;
			this.comboBox1.DataSource =
				DriveInfo.GetDrives()
				         .Where(d => d.DriveType == DriveType.CDRom)
				         .ToList();
		}

		private void ShowDriveInfoStatus()
		{
			this.toolStripStatusLabel1.Text = this.driveInfo == null
				? string.Empty
				: string.Format("{0} [{1}]",
				                this.driveInfo.Name,
				                this.driveInfo.IsReady
					                ? this.driveInfo.VolumeLabel
					                : Resources.DiskSatatus_InsertDisk);
		}

		private void BuildTree(DirectoryInfo directoryInfo, TreeNodeCollection treeNodeCollection)
		{
			var currentNode = treeNodeCollection.Add(directoryInfo.Name);

			if(!this.checkBox1.Checked)
				foreach (var fileInfo in directoryInfo.GetFiles())
					currentNode.Nodes.Add(fileInfo.FullName, fileInfo.Name);

			foreach (var subdir in directoryInfo.GetDirectories())
				BuildTree(subdir, currentNode.Nodes);
		}
	}
}