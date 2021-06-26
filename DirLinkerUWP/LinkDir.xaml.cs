using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace DirLinker
{
    public sealed partial class LinkDir : UserControl
    {
        public static readonly DependencyProperty DirectoryProperty = DependencyProperty.Register(
            "Directory",
            typeof(string),
            typeof(LinkDir),
            new PropertyMetadata(null)
        );

        public string Directory
        {
            get => GetValue(DirectoryProperty).ToString();
            set => SetValue(DirectoryProperty, value);
        }

        public LinkDir()
        {
            DataContext = this;
            InitializeComponent();
        }
    }
}
