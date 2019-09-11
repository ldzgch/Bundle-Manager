﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BundleFormat;
using ModelViewer.SceneData;
using OpenTK.Graphics.OpenGL;

namespace ModelViewer
{
    public partial class ModelViewerForm : Form, IEntryEditor
    {
		public SceneRenderControl Renderer => rndMain;

        public ModelViewerForm()
        {
            InitializeComponent();
        }

        public static void ShowModelViewer(Scene scene)
        {
            ModelViewerForm viewer = new ModelViewerForm();
            viewer.rndMain.Scene = scene;
            viewer.ShowDialog();
        }

        public static void ShowModelViewer(IWin32Window owner, Scene scene)
        {
            ModelViewerForm viewer = new ModelViewerForm();
            viewer.rndMain.Scene = scene;
            viewer.ShowDialog(owner);
        }

        public void ExportCollada14()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Collada Document|*.dae|All Files|*.*";
            DialogResult result = sfd.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                if (string.IsNullOrEmpty(sfd.FileName))
                    return;

                rndMain.Scene.ExportCollada14(sfd.FileName);
            }
        }

        public void ExportWavefrontObj()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Wavefront OBJ|*.obj|All Files|*.*";
            DialogResult result = sfd.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                if (string.IsNullOrEmpty(sfd.FileName))
                    return;

                rndMain.Scene.ExportWavefrontObj(sfd.FileName);
            }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void collada14ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportCollada14();
        }

        private void wavefrontOBJToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportWavefrontObj();
        }
    }
}
