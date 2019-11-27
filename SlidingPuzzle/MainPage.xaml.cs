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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SlidingPuzzle
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private TransformGroup transforms;
        private MatrixTransform previousTransform;
        private CompositeTransform deltaTransform;
        List<Image> AllGridPanels = null;
        List<ImagePanel> ImagePosition = null;

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
            AllGridPanels.Add(cropImg14);
            AllGridPanels.Add(cropImg15);

            InitManipulationTransforms();
            cropImg0.ManipulationDelta += new ManipulationDeltaEventHandler(ManipulateMe_ManipulationDelta);
            cropImg0.ManipulationMode = ManipulationModes.TranslateY | ManipulationModes.TranslateX;

            InitManipulationTransforms1();
            cropImg1.ManipulationDelta += new ManipulationDeltaEventHandler(ManipulateMe_ManipulationDelta1);
            cropImg1.ManipulationMode = ManipulationModes.TranslateY | ManipulationModes.TranslateX;

            InitManipulationTransforms2();
            cropImg2.ManipulationDelta += new ManipulationDeltaEventHandler(ManipulateMe_ManipulationDelta2);
            cropImg2.ManipulationMode = ManipulationModes.TranslateY | ManipulationModes.TranslateX;



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

        /// MOVING -------------------------------------//
        private void InitManipulationTransforms()
        {
            transforms = new TransformGroup();
            previousTransform = new MatrixTransform() { Matrix = Matrix.Identity };
            deltaTransform = new CompositeTransform();

            transforms.Children.Add(previousTransform);
            transforms.Children.Add(deltaTransform);

            // Set the render transform on the rect
            //foreach (Image croppedImg in AllGridPanels)
         
            cropImg0.RenderTransform = transforms;
            
        }

        private void InitManipulationTransforms1()
        {
            transforms = new TransformGroup();
            previousTransform = new MatrixTransform() { Matrix = Matrix.Identity };
            deltaTransform = new CompositeTransform();

            transforms.Children.Add(previousTransform);
            transforms.Children.Add(deltaTransform);

            // Set the render transform on the rect
            //foreach (Image croppedImg in AllGridPanels)

            cropImg1.RenderTransform = transforms;

        }

        private void InitManipulationTransforms2()
        {
            transforms = new TransformGroup();
            previousTransform = new MatrixTransform() { Matrix = Matrix.Identity };
            deltaTransform = new CompositeTransform();

            transforms.Children.Add(previousTransform);
            transforms.Children.Add(deltaTransform);

            // Set the render transform on the rect
            //foreach (Image croppedImg in AllGridPanels)

            cropImg2.RenderTransform = transforms;

        }



        // Process the change resulting from a manipulation
        void ManipulateMe_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            previousTransform.Matrix = transforms.Value;
            // Look at the Delta property of the ManipulationDeltaRoutedEventArgs to retrieve
            // the rotation, scale, X, and Y changes
            deltaTransform.TranslateX = e.Delta.Translation.X;
            deltaTransform.TranslateY = e.Delta.Translation.Y;
        }
        void ManipulateMe_ManipulationDelta1(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            previousTransform.Matrix = transforms.Value;
            // Look at the Delta property of the ManipulationDeltaRoutedEventArgs to retrieve
            // the rotation, scale, X, and Y changes
            deltaTransform.TranslateX = e.Delta.Translation.X;
            deltaTransform.TranslateY = e.Delta.Translation.Y;
        }
        void ManipulateMe_ManipulationDelta2(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            previousTransform.Matrix = transforms.Value;
            // Look at the Delta property of the ManipulationDeltaRoutedEventArgs to retrieve
            // the rotation, scale, X, and Y changes
            deltaTransform.TranslateX = e.Delta.Translation.X;
            deltaTransform.TranslateY = e.Delta.Translation.Y;
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
    }
}
