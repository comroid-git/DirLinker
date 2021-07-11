using System;
using System.Collections.Generic;
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
    /// Interaction logic for LinkDirEdit.xaml
    /// </summary>
    public partial class LinkDirEdit : Window
    {
        public static readonly DependencyProperty DirectoryProperty = DependencyProperty.Register(
            "Directory",
            typeof(string),
            typeof(LinkDirEdit),
            new PropertyMetadata(null)
        );
        public readonly Configuration.LinkDir Blob;

        public string Directory
        {
            get => Blob.Directory;
            set => SetValue(DirectoryProperty, Blob.Directory = value);
        }

        public LinkDirEdit(Configuration.LinkDir blob)
        {
            Blob = blob;
            DataContext = this;
            InitializeComponent();
        }

        private void Button_OK(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
