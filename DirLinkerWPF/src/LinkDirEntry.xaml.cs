using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public static readonly DependencyProperty LinkDirNameProperty = DependencyProperty.Register(
            "LinkDirName",
            typeof(string),
            typeof(LinkDirEntry),
            new PropertyMetadata(null)
        );
        private MainWindow _window;
        public Configuration.LinkDir Blob;
        public bool IsDemo => _window == null;
        private Dictionary<string, Configuration.LinkBlob> _blobs = new Dictionary<string, Configuration.LinkBlob>();

        public string LinkDirName
        {
            get => Blob.Directory;
            set => SetValue(LinkDirNameProperty, Blob.Directory = value);
        }

        public LinkDirEntry(MainWindow window, Configuration.LinkDir blob)
        {
            DataContext = this;
            InitializeComponent();
            _window = window;
            Blob = blob;
            LinkDirName = LinkDirName;
        }

        public LinkBlobEntry Add(Configuration.LinkBlob blob)
        {
            var entry = new LinkBlobEntry(_window, this, blob);
            LinkList.Children.Add(entry);
            blob.Entry = entry;
            _blobs[blob.LinkName] = blob;
            UpdateHeight();
            Blob.Links.Add(blob);
            return entry;
        }

        public LinkBlobEntry GetOrCreateLink(string linkName, DirectoryInfo targetDir)
        {
            _blobs.TryGetValue(linkName, out Configuration.LinkBlob add);
            if (add != null)
                return add.Entry;
            return Add(new Configuration.LinkBlob { LinkName = linkName, TargetDir = targetDir });
        }

        private void UpdateHeight()
        {
            var count = LinkList.Children.Cast<LinkBlobEntry>().Count();
            ListRow.Height = new GridLength(count * 35);
        }

        private void Button_Edit(object sender, RoutedEventArgs e)
        {
            //todo Window.StartEditDirectory(this);
        }

        public bool Equals(object other)
        {
            return other switch
            {
                LinkDirEntry entry => entry.LinkDirName.Equals(LinkDirName),
                Configuration.LinkDir dir => dir.Directory.Equals(LinkDirName),
                _ => base.Equals(other)
            };
        }
    }
}
