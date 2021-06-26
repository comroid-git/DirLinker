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
            UpdateHeight();
            blob.Entry = entry;
            return entry;
        }

        public LinkBlobEntry GetOrCreateLink(string linkName, DirectoryInfo targetDir)
        {
            return LinkList.Children.Cast<LinkBlobEntry>()
                       .FirstOrDefault(it => it.LinkName.Equals(linkName))
                   ?? Add(new Configuration.LinkBlob { LinkName = linkName, TargetDir = targetDir });
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
    }
}
