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
        private MainWindow _window;
        public Configuration.LinkDir Blob;
        private List<Configuration.LinkBlob> _blobs;
        public bool IsDemo => _window == null;

        public string LinkDirName
        {
            get => Blob.Directory;
            set => SetValue(LinkDirNameProp, Blob.Directory = value);
        }

        public LinkDirEntry()
        {
            DataContext = this;
            InitializeComponent();
            Blob = new Configuration.LinkDir { Entry = this };
        }

        public LinkDirEntry(MainWindow window, Configuration.LinkDir blob)
        {
            DataContext = this;
            InitializeComponent();

            _window = window;
            Blob = blob;
        }

        public void Add(Configuration.LinkBlob blob)
        {
            var entry = new LinkBlobEntry(_window, this, blob);
            LinkList.Children.Add(entry);
            blob.Entry = entry;
            _blobs.Add(blob);
        }

        private void Button_Edit(object sender, RoutedEventArgs e)
        {
            //todo Window.StartEditDirectory(this);
        }
    }
}
