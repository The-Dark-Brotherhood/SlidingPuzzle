using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Graphics.Imaging;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Input;
using System.Windows;

using Windows.Foundation;
using System.Collections.Generic;


namespace SlidingPuzzle
{
    class ImagePanel
    {
        public Image imageInthePanel = null;
        public int ShouldBeInIndex = -1;

 
        
        public ImagePanel(SoftwareBitmapSource source, int shouldBe)
        {
            imageInthePanel.Source = source;
            ShouldBeInIndex = shouldBe;
        }
    }
}
