using System;
using System.Windows.Forms;

namespace RiftPracticePlus.Visualizer;

public partial class Form1 : Form {
    public Form1() {
        InitializeComponent();
        _ = new Visualizer(new GraphicsPanel(this, panel, 8, 8));
    }

    private void Form1_Load(object sender, EventArgs e) { }
}