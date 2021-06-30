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
using DirLinkerConfig;

namespace DirLinkerWPF
{
    /// <summary>
    /// Interaction logic for ConfigPopup.xaml
    /// </summary>
    public partial class ConfigPopup : Window
    {
        private readonly DirLinker _main;

        public ConfigPopup(DirLinker main)
        {
            _main = main;
            InitializeComponent();
        }

        private void SaveAndOpen(object sender, RoutedEventArgs e)
        {
            _main.CleanupConfig();
            Configuration.SaveConfig();
            Process.Start("explorer.exe", Configuration.ConfigFile);
        }
    }
}
