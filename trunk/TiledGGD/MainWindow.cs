using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TiledGGD
{
    public partial class MainWindow : Form
    {

        private static GraphicsData graphicsData;
        internal static GraphicsData GraphData { get { return graphicsData; } }
        private static PaletteData paletteData;
        internal static PaletteData PalData { get { return paletteData; } }
        private static DataPanelFiller datafiller;

        private static Font fnt;
        public override Font Font { get { return base.Font; } set { base.Font = value; fnt = value; } }
        public static Font MenuFont { get { return fnt; } }

        private static MainWindow mainWindow;

        private Size previousSize;

        #region constructor
        public MainWindow()
        {
            InitializeComponent();

            this.DoubleBuffered = true;
            mainWindow = this;

            fnt = this.Font;

            GraphicsPanel.Paint += new PaintEventHandler(GraphicsPanel_Paint);
            PalettePanel.Paint += new PaintEventHandler(PalettePanel_Paint);
            DataPanel.Paint += new PaintEventHandler(DataPanel_Paint);

            datafiller = new DataPanelFiller(this.DataPanel);

            paletteData = new PaletteData(PaletteFormat.FORMAT_3BPP, PaletteOrder.ORDER_BGR);
            graphicsData = new GraphicsData(paletteData);
            GraphicsData.GraphFormat = GraphicsFormat.FORMAT_16BPP;
            GraphicsData.Tiled = false;
            GraphicsData.WidthSkipSize = 8;
            GraphicsData.Zoom = 2;

            

            //this.GraphicsPanel.Height = 1000;

            GraphicsPanel.DragEnter += new DragEventHandler(palGraphDragEnter);
            PalettePanel.DragEnter += new DragEventHandler(palGraphDragEnter);

            GraphicsPanel.DragDrop += new DragEventHandler(GraphicsPanel_DragDrop);
            PalettePanel.DragDrop += new DragEventHandler(PalettePanel_DragDrop);

            paletteData.load("D:/Sprites/Sonic/PAL1A.dat");
            graphicsData.load("D:/Sprites/Sonic/OVL1A.BIN");
            //paletteData.load("H:/PLT/DrScheme.exe");
            //graphicsData.load("H:/PLT/DrScheme.exe");
            PaletteData.SkipSize = 16;
            PaletteData.SkipMetric = PaletteSkipMetric.METRIC_COLOURS;

            this.KeyDown += new KeyEventHandler(MainWindow_KeyDown);

            this.ResizeEnd += new EventHandler(ReconfigurePanels);

            this.previousSize = this.Size;

            updateMenu();
        }
        #endregion

        #region method: ReconfigurePanels
        /// <summary>
        /// resizes and relocates the panels when the window is resized
        /// </summary>
        void ReconfigurePanels(object sender, EventArgs e)
        {
            int dw = this.Size.Width - this.previousSize.Width;

            this.PalettePanel.Location = new Point(this.PalettePanel.Location.X + dw, this.PalettePanel.Location.Y);
            this.DataPanel.Location = new Point(this.DataPanel.Location.X + dw, this.DataPanel.Location.Y);
            this.GraphicsPanel.Size = new Size(this.GraphicsPanel.Size.Width + dw, this.GraphicsPanel.Size.Height);

            this.previousSize = this.Size;
        }
        #endregion

        #region Methods: Quit
        void Quit(object sender, EventArgs e)
        {
            this.Quit();
        }
        void Quit()
        {
            Application.Exit();
        }
        #endregion

        #region KeyDown event handler
        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.P: paletteData.TogglePaletteOrder(); break;
                case Keys.N: paletteData.DoSkip(true); break;
                case Keys.M: paletteData.DoSkip(false); break;
                case Keys.Up: graphicsData.decreaseHeight(); break;
                case Keys.Down: graphicsData.increaseHeight(); break;
                case Keys.Left: graphicsData.decreaseWidth(); break;
                case Keys.Right: graphicsData.increaseWidth(); break;
                case Keys.Subtract: GraphicsData.Zoom /= 2; break;
                case Keys.Add: GraphicsData.Zoom *= 2; break;
                case Keys.PageDown: graphicsData.DoSkip(true); break;
                case Keys.PageUp: graphicsData.DoSkip(false); break;
                case Keys.B: graphicsData.toggleGraphicsFormat(); break;
                case Keys.F: graphicsData.toggleTiled(); break;
                case Keys.E: if (e.Control) graphicsData.toggleEndianness(); else if (e.Shift) paletteData.toggleEndianness(); break;
                case Keys.L: if (e.Control) graphicsData.toggleSkipSize(); else if (e.Shift) paletteData.toggleSkipSize(); break;
            }
            updateMenu();
        }
        #endregion

        #region method: updateMenu
        /// <summary>
        /// Updates the checks in the menu
        /// </summary>
        private void updateMenu()
        {
            // graphics format
            foreach (ToolStripMenuItem tsme in this.graphFormatTSMI.DropDownItems)
                tsme.Checked = false;
            (this.graphFormatTSMI.DropDownItems[(int)GraphicsData.GraphFormat - 1] as ToolStripMenuItem).Checked = true;

            // palette format
            foreach (ToolStripMenuItem tsme in this.palFormatTSMI.DropDownItems)
                tsme.Checked = false;
            (this.palFormatTSMI.DropDownItems[(int)PaletteData.PalFormat - 5] as ToolStripMenuItem).Checked = true;

            // graphics endianness
            graphEndian_littleTSMI.Checked = !(graphEndian_bigTSMI.Checked = GraphicsData.IsBigEndian);

            // palette endianness
            palEndian_littleTSMI.Checked = !(palEndian_bigTSMI.Checked = PaletteData.IsBigEndian);

            // graphics mode
            graphMode_LinearTSMI.Checked = !(graphMode_tiledTSMI.Checked = GraphicsData.Tiled);

            // graphics skip size
            foreach (ToolStripMenuItem tsme in this.graphSSTSMI.DropDownItems)
                tsme.Checked = false;
            switch (GraphicsData.SkipMetric)
            {
                case GraphicsSkipMetric.METRIC_BYTES:
                    switch (GraphicsData.SkipSize)
                    {
                        case 1: graphSS_1byteTSMI.Checked = true; break;
                        case 2: graphSS_2bytesTSMI.Checked = true; break;
                        case 4: graphSS_4bytesTSMI.Checked = true; break;
                        default: throw new Exception("Unknown graphics skip size: " + GraphicsData.SkipSize + " bytes");
                    }
                    break;
                case GraphicsSkipMetric.METRIC_YPIX:
                    switch (GraphicsData.SkipSize)
                    {
                        case 1: palSS_1colTSMI.Checked = true; break;
                        case -1: graphSS_1trTSMI.Checked = true; break;
                        default: throw new Exception("Unknown graphics skip size: " + GraphicsData.SkipSize + " rows");
                    }
                    break;
                case GraphicsSkipMetric.METRIC_WIDTH: graphSS_widthTSMI.Checked = true; break;
                case GraphicsSkipMetric.METRIC_HEIGHT: graphSS_heightTSMI.Checked = true; break;
                default: throw new Exception("Unknown graphics skip metric: " + GraphicsData.SkipMetric.ToString());
            }

            // palette alpha location
            palAlpha_endTSMI.Checked = !(palAlpha_startTSMI.Checked = (PaletteData.alphaLoc != AlphaLocation.END));

            // palette order
            foreach (ToolStripMenuItem tsme in palOrderTSMI.DropDownItems)
                tsme.Checked = false;
            (palOrderTSMI.DropDownItems[(int)PaletteData.PalOrder] as ToolStripMenuItem).Checked = true;

            // palette skip size
            foreach (ToolStripMenuItem tsme in this.palSSTSMI.DropDownItems)
                tsme.Checked = false;
            switch (PaletteData.SkipMetric)
            {
                case PaletteSkipMetric.METRIC_BYTES:
                    switch (PaletteData.SkipSize)
                    {
                        case 1: palSS_1byteTSMI.Checked = true; break;
                        case 0x10000: palSS_64kbytesTSMI.Checked = true; break;
                        default: throw new Exception("Unknown palette skip size: " + PaletteData.SkipSize + " bytes");
                    }
                    break;
                case PaletteSkipMetric.METRIC_COLOURS:
                    switch (PaletteData.SkipSize)
                    {
                        case 1: palSS_1colTSMI.Checked = true; break;
                        case 16: palSS_16colTSMI.Checked = true; break;
                        case 256: palSS_256colTSMI.Checked = true; break;
                        default: throw new Exception("Unknown palette skip size: " + PaletteData.SkipSize + " colours");
                    }
                    break;
                default: throw new Exception("Unknown palette skip metric: " + PaletteData.SkipMetric.ToString());
            }
        }
        #endregion

        #region drag methods
        void PalettePanel_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                paletteData.load(((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString());
                DoRefresh();
            }
            catch (Exception ex)
            {
                Console.Write(ex.StackTrace);
            }
        }

        void GraphicsPanel_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                graphicsData.load(((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString());
                DoRefresh();
            }
            catch (Exception ex)
            {
                Console.Write(ex.StackTrace);
            }
        }

        void palGraphDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }
        #endregion

        #region paint methods
        void DataPanel_Paint(object sender, PaintEventArgs e)
        {
            // TODO
            if (false)
                throw new Exception("TODO");
        }

        void PalettePanel_Paint(object sender, PaintEventArgs e)
        {
            paletteData.paint(this, e);
        }

        void GraphicsPanel_Paint(object sender, PaintEventArgs e)
        {
            graphicsData.paint(this, e);
        }
        #endregion

        #region method: DoRefresh
        /// <summary>
        /// Refresh the Main Window
        /// </summary>
        public static void DoRefresh()
        {
            mainWindow.Refresh();
            datafiller.refresh();
        }
        #endregion

        #region toolstrip response methods

        #region about box
        private void aboutTSMI_Click(object sender, EventArgs e)
        {
            if (this.aboutBox == null || this.aboutBox.IsDisposed)
                this.aboutBox = new AboutBox();
            this.aboutBox.Visible = true;
        }
        #endregion

        #region graphical format
        private void graphicalFormatTSMI_Click(object sender, EventArgs e)
        {
            foreach (ToolStripMenuItem tsme in this.graphFormatTSMI.DropDownItems)
                tsme.Checked = false;
            (sender as ToolStripMenuItem).Checked = true;

            if (sender == graphFormat_1bppTSMI)
                GraphicsData.GraphFormat = GraphicsFormat.FORMAT_1BPP;
            else if (sender == graphFormat_2bppTSMI)
                GraphicsData.GraphFormat = GraphicsFormat.FORMAT_2BPP;
            else if (sender == graphFormat_4bppTSMI)
                GraphicsData.GraphFormat = GraphicsFormat.FORMAT_4BPP;
            else if (sender == graphFormat_8bppTSMI)
                GraphicsData.GraphFormat = GraphicsFormat.FORMAT_8BPP;
            else if (sender == graphFormat_16bppTSMI)
                GraphicsData.GraphFormat = GraphicsFormat.FORMAT_16BPP;
            else if (sender == graphFormat_24bppTSMI)
                GraphicsData.GraphFormat = GraphicsFormat.FORMAT_24BPP;
            else if (sender == graphFormat_32bppTSMI)
                GraphicsData.GraphFormat = GraphicsFormat.FORMAT_32BPP;
            else
                throw new Exception("Invalid Graphcial format action");
            DoRefresh();
            updateMenu();
        }
        #endregion

        #region copy to clipboard
        private void copyToClipboard(object sender, EventArgs e)
        {
            if (sender == copyGraphicsToolStripMenuItem)
                graphicsData.copyToClipboard();
            else if (sender == copyPaletteToolStripMenuItem)
                paletteData.copyToClipboard();
            else
                throw new Exception("Invalid Copy To Clipboard action");
        }
        #endregion

        #region graphical mode
        private void graphicalModeTSMI_Click(object sender, EventArgs e)
        {
            if (sender == graphMode_LinearTSMI)
                GraphicsData.Tiled = false;
            else if (sender == graphMode_tiledTSMI)
                GraphicsData.Tiled = true;
            else
                throw new Exception("Invalid Linear/Tiled action");
            DoRefresh();
            updateMenu();
        }
        #endregion

        #region shortcuts
        private void shortcutsTSMI_Click(object sender, EventArgs e)
        {
            if (this.controlShortBox == null || this.controlShortBox.IsDisposed)
                this.controlShortBox = new ControlShorts();
            this.controlShortBox.Visible = true;
        }
        #endregion

        #region raphical endianness
        private void graphEndianTSMI_Click(object sender, EventArgs e)
        {
            if (sender == graphEndian_bigTSMI)
                GraphicsData.IsBigEndian = true;
            else if (sender == graphEndian_littleTSMI)
                GraphicsData.IsBigEndian = false;
            else
                throw new Exception("Invalid graphical endianness action");
            DoRefresh();
            updateMenu();
        }
        #endregion

        #region graphical skip size
        private void graphSSTSMI_Click(object sender, EventArgs e)
        {
            if (sender == graphSS_1byteTSMI)
            {
                GraphicsData.SkipSize = 1;
                GraphicsData.SkipMetric = GraphicsSkipMetric.METRIC_BYTES;
            }
            else if (sender == graphSS_2bytesTSMI)
            {
                GraphicsData.SkipMetric = GraphicsSkipMetric.METRIC_BYTES;
                GraphicsData.SkipSize = 2;
            }
            else if (sender == graphSS_4bytesTSMI)
            {
                GraphicsData.SkipSize = 4;
                GraphicsData.SkipMetric = GraphicsSkipMetric.METRIC_BYTES;
            }
            else if (sender == graphSS_1pixTSMI)
            {
                GraphicsData.SkipSize = 1;
                GraphicsData.SkipMetric = GraphicsSkipMetric.METRIC_YPIX;
            }
            else if (sender == graphSS_1trTSMI)
            {
                GraphicsData.SkipSize = -1;
                GraphicsData.SkipMetric = GraphicsSkipMetric.METRIC_YPIX;
            }
            else if (sender == graphSS_widthTSMI)
                GraphicsData.SkipMetric = GraphicsSkipMetric.METRIC_WIDTH;
            else if (sender == graphSS_heightTSMI)
                GraphicsData.SkipMetric = GraphicsSkipMetric.METRIC_HEIGHT;
            else
                throw new Exception("Invalid Graphics Skip Size action");

            updateMenu();
        }
        #endregion

        #region palette format
        private void palFormatTSMI_Click(object sender, EventArgs e)
        {
            if (sender == palFormat_2BpcTSMI)
                PaletteData.PalFormat = PaletteFormat.FORMAT_2BPP;
            else if (sender == palFormat_3BpcTSMI)
                PaletteData.PalFormat = PaletteFormat.FORMAT_3BPP;
            else if (sender == palFormat_4BpcTSMI)
                PaletteData.PalFormat = PaletteFormat.FORMAT_4BPP;
            else
                throw new Exception("Invalid Palette Format action");
            DoRefresh();
            updateMenu();
        }
        #endregion

        #region palette endianness
        private void palEndianTSMI_Click(object sender, EventArgs e)
        {
            if (sender == palEndian_bigTSMI)
                PaletteData.IsBigEndian = true;
            else if (sender == palEndian_littleTSMI)
                PaletteData.IsBigEndian = false;
            else
                throw new Exception("Invalid Palette Endianness action");
            DoRefresh();
            updateMenu();
        }
        #endregion

        #region palette alpha location
        private void palAlphaTSMI_Click(object sender, EventArgs e)
        {
            if (sender == palAlpha_endTSMI)
                PaletteData.alphaLoc = AlphaLocation.END;
            else if (sender == palAlpha_startTSMI)
                PaletteData.alphaLoc = AlphaLocation.START;
            else 
                throw new Exception("Invalid Alpha location action");
            DoRefresh();
            updateMenu();
        }
        #endregion

        #region palette order
        private void palOrderTSMI_Click(object sender, EventArgs e)
        {
            if (sender == palOrder_bgrTSMI)
                PaletteData.PalOrder = PaletteOrder.ORDER_BGR;
            else if (sender == palOrder_brgTSMI)
                PaletteData.PalOrder = PaletteOrder.ORDER_BRG;
            else if (sender == palOrder_gbrTSMI)
                PaletteData.PalOrder = PaletteOrder.ORDER_GBR;
            else if (sender == palOrder_grbTSMI)
                PaletteData.PalOrder = PaletteOrder.ORDER_GRB;
            else if (sender == palOrder_rbgTSMI)
                PaletteData.PalOrder = PaletteOrder.ORDER_RBG;
            else if (sender == palOrder_rgbTSMI)
                PaletteData.PalOrder = PaletteOrder.ORDER_RGB;
            else
                throw new Exception("Invalid palette order action");
            DoRefresh();
            updateMenu();
        }
        #endregion

        #region palette skip size
        private void palSSTSMI_Click(object sender, EventArgs e)
        {
            if (sender == palSS_1byteTSMI)
            {
                PaletteData.SkipSize = 1;
                PaletteData.SkipMetric = PaletteSkipMetric.METRIC_BYTES;
            }
            else if (sender == palSS_1colTSMI)
            {
                PaletteData.SkipSize = 1;
                PaletteData.SkipMetric = PaletteSkipMetric.METRIC_COLOURS;
            }
            else if (sender == palSS_16colTSMI)
            {
                PaletteData.SkipSize = 16;
                PaletteData.SkipMetric = PaletteSkipMetric.METRIC_COLOURS;
            }
            else if (sender == palSS_256colTSMI)
            {
                PaletteData.SkipSize = 256;
                PaletteData.SkipMetric = PaletteSkipMetric.METRIC_COLOURS;
            }
            else if (sender == palSS_64kbytesTSMI)
            {
                PaletteData.SkipSize = 0x10000;
                PaletteData.SkipMetric = PaletteSkipMetric.METRIC_BYTES;
            }
            else
                throw new Exception("Invalid palette skip size action");
            updateMenu();
        }
        #endregion

        #region save graphics
        private void saveGraphTSMI_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PNG file (*.png)|*.png";
            sfd.DefaultExt = "png";
            sfd.Title = "Save Graphics as PNG";
            sfd.SupportMultiDottedExtensions = true;
            sfd.ShowHelp = false;
            sfd.OverwritePrompt = true;
            sfd.AddExtension = true;
            sfd.RestoreDirectory = true;
            DialogResult res = sfd.ShowDialog();

            if (res == DialogResult.OK || res == DialogResult.Yes)
            {
                string flnm = sfd.FileName;
                graphicsData.toBitmap().Save(flnm);
            }
        }
        #endregion

        #region save palette
        private void savePalTSMI_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PNG file (*.png)|*.png";
            sfd.DefaultExt = "png";
            sfd.Title = "Save Palette as PNG";
            sfd.SupportMultiDottedExtensions = true;
            sfd.ShowHelp = false;
            sfd.OverwritePrompt = true;
            sfd.AddExtension = true;
            sfd.RestoreDirectory = true;
            DialogResult res = sfd.ShowDialog();

            if (res == DialogResult.OK || res == DialogResult.Yes)
            {
                string flnm = sfd.FileName;
                paletteData.toBitmap().Save(flnm);
            }
        }
        #endregion

        #region load graphics
        private void openGraphTSMI_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Any file (*.*)|*.*";
            ofd.RestoreDirectory = true;
            ofd.ShowHelp = false;
            ofd.Multiselect = false;
            ofd.Title = "Open file as Graphics";
            DialogResult res = ofd.ShowDialog();
            if (res == DialogResult.OK || res == DialogResult.Yes)
            {
                graphicsData.load(ofd.FileName);
                DoRefresh();
            }
        }
        #endregion

        #region load palette
        private void openPalTSMI_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Any file (*.*)|*.*";
            ofd.RestoreDirectory = true;
            ofd.ShowHelp = false;
            ofd.Multiselect = false;
            ofd.Title = "Open file as Palette";
            DialogResult res = ofd.ShowDialog();
            if (res == DialogResult.OK || res == DialogResult.Yes)
            {
                paletteData.load(ofd.FileName);
                DoRefresh();
            }
        }
        #endregion

        #endregion

    }
}