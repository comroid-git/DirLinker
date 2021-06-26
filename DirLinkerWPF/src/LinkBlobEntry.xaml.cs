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
    /// Interaction logic for LinkBlobEntry.xaml
    /// </summary>
    public partial class LinkBlobEntry : UserControl
    {
        public static readonly DependencyProperty LinkNameProperty = DependencyProperty.Register(
            "LinkName",
            typeof(string),
            typeof(LinkBlobEntry),
            new PropertyMetadata(null)
        );
        public static readonly DependencyProperty TargetNameProperty = DependencyProperty.Register(
            "TargetName",
            typeof(string),
            typeof(LinkBlobEntry),
            new PropertyMetadata(null)
        );

        private readonly MainWindow _window;
        public readonly LinkDirEntry LinkDirEntry;
        public readonly Configuration.LinkBlob Blob;
        public bool IsDemo => _window == null;

        public string LinkName
        {
            get => Blob.LinkName;
            set => SetValue(LinkNameProperty, Blob.LinkName = value);
        }
        public string TargetName
        {
            get => Blob.TargetDirectory;
            set => SetValue(TargetNameProperty, Blob.TargetDirectory = value);
        }

        public LinkBlobEntry(MainWindow window, LinkDirEntry linkDirEntry, Configuration.LinkBlob blob)
        {
            DataContext = this;
            InitializeComponent();
            _window = window;
            LinkDirEntry = linkDirEntry;
            Blob = blob;
            LinkName = LinkName;
            TargetName = TargetName;
        }

        private void Button_Edit(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
