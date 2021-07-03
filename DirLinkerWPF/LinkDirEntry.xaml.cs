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

        public string LinkDirName
        {
            get => Blob.Directory;
            set => SetValue(LinkDirNameProperty, Blob.Directory = value);
        }

        public void ReloadView()
        {
            foreach (var each in Blob.Links.Where(it => it.Entry == null))
                each.Entry = _window.CreateBlobEntry(Blob, each);
            ClearView();
            foreach (var each in Blob.Links)
                AddLinkToView(each.Entry);
            EnabledBox.IsChecked = Blob.Enabled;
        }

        public void ClearView()
        {
            LinkList.Children.Clear();
            UpdateHeight();
        }

        public void AddLinkToView(ILinkBlobEntry entry)
        {
            if (entry is LinkBlobEntry value) 
                LinkList.Children.Add(value);
            UpdateHeight();
        }

        public LinkDirEntry(DirLinker window, Configuration.LinkDir blob)
        {
            DataContext = this;
            InitializeComponent();
            _window = window;
            Blob = blob;
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

        private void DoEnable(object sender, RoutedEventArgs e)
        {
            EnabledBox.IsChecked = Blob.Enabled = true;
            foreach (UIElement each in LinkList.Children)
                if (each is LinkBlobEntry entry)
                    entry.DoEnable(sender, e);
        }

        private void DoDisable(object sender, RoutedEventArgs e)
        {
            EnabledBox.IsChecked = Blob.Enabled = false;
            foreach (UIElement each in LinkList.Children)
                if (each is LinkBlobEntry entry)
                    entry.DoDisable(sender, e);
        }
    }
}
