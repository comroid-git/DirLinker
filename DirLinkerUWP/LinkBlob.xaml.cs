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
    public sealed partial class LinkBlob : UserControl
    {
        public static readonly DependencyProperty LinkNameProperty = DependencyProperty.Register(
            "LinkName",
            typeof(string),
            typeof(LinkBlob),
            new PropertyMetadata(null)
        );
        public static readonly DependencyProperty TargetDirectoryProperty = DependencyProperty.Register(
            "TargetDirectory",
            typeof(string),
            typeof(LinkBlob),
            new PropertyMetadata(null)
        );

        public string LinkName
        {
            get => GetValue(LinkNameProperty).ToString();
            set => SetValue(LinkNameProperty, value);
        }

        public string TargetDirectory
        {
            get => GetValue(TargetDirectoryProperty).ToString();
            set => SetValue(TargetDirectoryProperty, value);
        }

        public LinkBlob()
        {
            this.InitializeComponent();
        }
    }
}
