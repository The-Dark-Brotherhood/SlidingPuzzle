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
using System.Collections.Generic;
using System.Linq;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SlidingPuzzle
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        List<Image> AllGridPanels = null;
        List<ImagePanel> ImagePosition = null;
        //Tuple<int, int> BlankLocation = new Tuple<int, int>(3,3);

        public MainPage()
        {
            this.InitializeComponent();
            ImagePosition = new List<ImagePanel>();

            AllGridPanels = new List<Image>();
            AllGridPanels.Add(cropImg0);
            AllGridPanels.Add(cropImg1);
            AllGridPanels.Add(cropImg2);
            AllGridPanels.Add(cropImg3);
            AllGridPanels.Add(cropImg4);
            AllGridPanels.Add(cropImg5);
            AllGridPanels.Add(cropImg6);
            AllGridPanels.Add(cropImg7);
            AllGridPanels.Add(cropImg8);
            AllGridPanels.Add(cropImg9);
            AllGridPanels.Add(cropImg10);
            AllGridPanels.Add(cropImg11);
            AllGridPanels.Add(cropImg12);
            AllGridPanels.Add(cropImg13);

        }


        private async void Button_Click(object sender, RoutedEventArgs e)
        {

            // Reference: https://docs.microsoft.com/en-us/windows/uwp/audio-video-camera/imaging
            // Select image
            FileOpenPicker fileOpenPicker = new FileOpenPicker();
            fileOpenPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            fileOpenPicker.FileTypeFilter.Add(".png");
            fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;

            var inputFile = await fileOpenPicker.PickSingleFileAsync();
            if (inputFile == null)
            {
                // The user cancelled the picking operation
                return;
            }

            SoftwareBitmap softwareBitmap;
            using (IRandomAccessStream stream = await inputFile.OpenAsync(FileAccessMode.Read))
            {
                // Create the decoder from the stream
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);

                // Get the SoftwareBitmap representation of the file
                softwareBitmap = await decoder.GetSoftwareBitmapAsync();
            }
            await CropImagesAsync(softwareBitmap);

            /*
            List<Tuple<int, int>> randomPos = new List<Tuple<int, int>>
            {
               new Tuple<int, int>(0,0),
               new Tuple<int, int>(0,1),
               new Tuple<int, int>(0,2),
               new Tuple<int, int>(0,3),
               new Tuple<int, int>(1,0),
               new Tuple<int, int>(1,1),
               new Tuple<int, int>(1,2),
               new Tuple<int, int>(1,3),
               new Tuple<int, int>(2,0),
               new Tuple<int, int>(2,1),
               new Tuple<int, int>(2,2),
               new Tuple<int, int>(2,3),
               new Tuple<int, int>(3,0),
               new Tuple<int, int>(3,1),
               new Tuple<int, int>(3,2),
            };

            Random rng = new Random();

            for (int counter = 0; counter < AllGridPanels.Count - 1 ; counter++)
            {
                int number = rng.Next(randomPos.Count);
                AllGridPanels[counter].SetValue(Grid.ColumnProperty, (int)randomPos[number].Item1);
                AllGridPanels[counter].SetValue(Grid.RowProperty, (int)randomPos[number].Item2);

                randomPos.RemoveAt(number);
            }*/
            }

        //-------------------------CROPPING--------------------------//
        private async Task CropImagesAsync(SoftwareBitmap softwareBitmap)
        {
            // How many grid elements: 16 (4x4)
            const int grid = 4;
            uint posX = 0;
            uint posY = 0;

            uint imgWidth = (uint)softwareBitmap.PixelWidth;
            uint imgHeight = (uint)softwareBitmap.PixelHeight;

            uint eachGridHeight = imgHeight / grid;
            uint eachGridWidth = imgWidth / grid;

            for (int counter = 0; counter < (AllGridPanels.Count - 1); counter++)
            {
                SoftwareBitmap croppedBitmap;
                using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
                {
                    BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.BmpEncoderId, stream);

                    encoder.SetSoftwareBitmap(softwareBitmap);

                    encoder.BitmapTransform.Bounds = new BitmapBounds()
                    {
                        X = posX,
                        Y = posY,
                        Height = eachGridHeight,
                        Width = eachGridWidth
                    };

                    posX += eachGridWidth;

                    if (posX == imgWidth)
                    {
                        posY += eachGridHeight;
                        posX = 0;
                    }

                    await encoder.FlushAsync();

                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
                    croppedBitmap = await decoder.GetSoftwareBitmapAsync(softwareBitmap.BitmapPixelFormat, softwareBitmap.BitmapAlphaMode);

                    if (croppedBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 ||
                     croppedBitmap.BitmapAlphaMode == BitmapAlphaMode.Straight)
                    {
                        croppedBitmap = SoftwareBitmap.Convert(croppedBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                    }


                    SoftwareBitmapSource source = new SoftwareBitmapSource();
                    await source.SetBitmapAsync(croppedBitmap);

                    AllGridPanels[counter].Source = source;
                }
            }
        }

        private void notBlank_Click(object sender, RoutedEventArgs e)
        {
            Button image = (Button)sender;
            Button blankSpace = blank;              // Programmaticaly do this ????

            Tuple<int, int> imageLocation = new Tuple<int, int>
            (
                (int)image.GetValue(Grid.RowProperty),
                (int)image.GetValue(Grid.ColumnProperty)
            );

            Tuple<int, int> blankLocation = new Tuple<int, int>
            (
                (int)blankSpace.GetValue(Grid.RowProperty),
                (int)blankSpace.GetValue(Grid.ColumnProperty)
            );

            // Neighbors positions 
            if (BlankIsNeighbor(imageLocation, blankLocation))
            {
                // Swap
                image.SetValue(Grid.RowProperty, blankLocation.Item1);
                image.SetValue(Grid.ColumnProperty, blankLocation.Item2);

                blankSpace.SetValue(Grid.RowProperty, imageLocation.Item1);
                blankSpace.SetValue(Grid.ColumnProperty, imageLocation.Item2);
            }
            else
            {
                // Dont move
            }
        }

        private bool BlankIsNeighbor(Tuple<int,int> imageLocation, Tuple<int, int> blankLocation)
        {
            bool isNeighbor = false;

            if ((imageLocation.Item1 == blankLocation.Item1 && (imageLocation.Item2 + 1) == blankLocation.Item2) ||
                (imageLocation.Item1 == blankLocation.Item1 && (imageLocation.Item2 - 1) == blankLocation.Item2) ||
                (imageLocation.Item2 == blankLocation.Item2 && (imageLocation.Item1 + 1) == blankLocation.Item1) ||
                (imageLocation.Item2 == blankLocation.Item2 && (imageLocation.Item1 - 1) == blankLocation.Item1))
            {
                isNeighbor = true;
            }

            return isNeighbor;
        }


    }
}
