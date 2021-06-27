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
using DirLinkerConfig;

namespace DirLinkerWPF
{
    /// <summary>
    /// Interaction logic for LinkDirEntry.xaml
    /// </summary>
    public partial class LinkDirEntry : UserControl, ILinkDirEntry
    {
        public static readonly DependencyProperty LinkDirNameProperty = DependencyProperty.Register(
            "LinkDirName",
            typeof(string),
            typeof(LinkDirEntry),
            new PropertyMetadata(null)
        );
        private DirLinker _window;
        public Configuration.LinkDir Blob;
        public bool IsDemo => _window == null;
        internal Dictionary<string, Configuration.LinkBlob> Blobs = new Dictionary<string, Configuration.LinkBlob>();

        public string LinkDirName
        {
            get => Blob.Directory;
            set => SetValue(LinkDirNameProperty, Blob.Directory = value);
        }

        public LinkDirEntry(DirLinker window, Configuration.LinkDir blob)
        {
            DataContext = this;
            InitializeComponent();
            _window = window;
            Blob = blob;
            Blob.UpdateHandler += UpdateHeight;
            LinkDirName = LinkDirName;
        }

        private void UpdateHeight()
        {
            var count = LinkList.Children.Cast<LinkBlobEntry>().Count();
            ListRow.Height = new GridLength(count * 35);
        }

        private void Button_AddLink(object sender, RoutedEventArgs e)
        {
            _window.StartAddLink(LinkDirName);
        }

        private void Button_Edit(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Button_Remove(object sender, RoutedEventArgs e)
        {
            _window.Button_RemoveDir(this);
        }
    }
}
