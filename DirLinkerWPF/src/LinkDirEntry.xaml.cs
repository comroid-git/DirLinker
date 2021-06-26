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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DirLinkerWPF.src
{
    /// <summary>
    /// Interaction logic for LinkDirEntry.xaml
    /// </summary>
    public partial class LinkDirEntry : UserControl
    {
        public static readonly DependencyProperty LinkDirNameProp = DependencyProperty.Register(
            "LinkDirName",
            typeof(string),
            typeof(LinkDirEntry),
            new PropertyMetadata(null)
        );
        public MainWindow Window;
        public Configuration.LinkDir Blob;
        private List<Configuration.LinkBlob> _blobs;
        public bool IsDemo => Blob == null;

        public string LinkDirName
        {
            get => Blob.Directory;
            set => SetValue(LinkDirNameProp, Blob.Directory = value);
        }

        public LinkDirEntry()
        {
            DataContext = this;
            InitializeComponent();
        }

        public void Add(Configuration.LinkBlob blob)
        {
            var entry = new LinkBlobEntry(Window, this, blob);
            LinkList.Children.Add(entry);
            blob.Entry = entry;
            _blobs.Add(blob);
        }

        private void Button_Edit(object sender, RoutedEventArgs e)
        {
            Window.StartEditDirectory(this);
        }
    }
}
