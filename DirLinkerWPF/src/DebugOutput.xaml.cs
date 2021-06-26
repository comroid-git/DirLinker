using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DirLinkerWPF
{
    /// <summary>
    /// Interaction logic for DebugOutput.xaml
    /// </summary>
    public partial class DebugOutput : Window
    {
        public DebugOutput()
        {
            DataContext = this;
            InitializeComponent();
        }

        public void WriteLine(object line)
        {
            Output.Text += '\n' + line.ToString();
            Scroll.Height = Scroll.ScrollableHeight;
        }
    }
}
