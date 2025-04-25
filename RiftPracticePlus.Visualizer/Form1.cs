using System;
using System.Windows.Forms;

namespace RiftPracticePlus.Visualizer;

public partial class Form1 : Form {
    private Visualizer visualizer;

    public Form1() {
        InitializeComponent();
        visualizer = new Visualizer(new GraphicsPanel(this, panel, 8, 8));
    }

    private void Form1_Load(object sender, EventArgs e) => visualizer.Start();
}