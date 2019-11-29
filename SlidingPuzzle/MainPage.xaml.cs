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
using Windows.Media.Capture;
using Windows.Foundation;




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
            //AllGridPanels = ShuffleList(AllGridPanels);
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

            for (int counter = 0; counter < (AllGridPanels.Count); counter++)
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

            gameContainer.Visibility = Visibility.Visible;


        }

        private void notBlank_Click(object sender, RoutedEventArgs e)
        {
            Button image = (Button)sender;
            Button blankSpace = this.blankButton;

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

        private bool BlankIsNeighbor(Tuple<int, int> imageLocation, Tuple<int, int> blankLocation)
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
        private static Random rng = new Random();

        //http://www.vcskicks.com/randomize_array.php
        private List<E> ShuffleList<E>(List<E> inputList)
        {
            List<E> randomList = new List<E>();

            Random r = new Random();
            int randomIndex = 0;
            while (inputList.Count > 0)
            {
                randomIndex = r.Next(0, inputList.Count); //Choose a random object in the list
                randomList.Add(inputList[randomIndex]); //add it to the new, random list
                inputList.RemoveAt(randomIndex); //remove to avoid duplicates
            }

            return randomList; //return the new random list
        }
        private async void Use_Photo(object sender, RoutedEventArgs e)
        {
            CameraCaptureUI captureUI = new CameraCaptureUI();
            captureUI.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Png;
            captureUI.PhotoSettings.CroppedSizeInPixels = new Size(400, 400);

            StorageFile photo = await captureUI.CaptureFileAsync(CameraCaptureUIMode.Photo);

            if (photo == null)
            {
                // User cancelled photo capture
                return;
            }
            IRandomAccessStream stream = await photo.OpenAsync(FileAccessMode.Read);
            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
            SoftwareBitmap softwareBitmap = await decoder.GetSoftwareBitmapAsync();

            SoftwareBitmap softwareBitmapBGR8 = SoftwareBitmap.Convert(softwareBitmap,
            BitmapPixelFormat.Bgra8,
            BitmapAlphaMode.Premultiplied);

            SoftwareBitmapSource bitmapSource = new SoftwareBitmapSource();
            await bitmapSource.SetBitmapAsync(softwareBitmapBGR8);

            await CropImagesAsync(softwareBitmap);
        }

        public static void Shuffle(ref Image[,] img)
        {
            // Get the dimensions.
            int num_rows = img.GetUpperBound(0) + 1;
            int num_cols = img.GetUpperBound(1) + 1;
            int num_cells = num_rows * num_cols;
            Image temp = new Image();
            // Randomize the array.
            Random rand = new Random();
            for (int i = 0; i < num_cells - 1; i++)
            {
                // Pick a random cell between i and the end of the array.
                int j = rand.Next(i, num_cells);

                // Convert to row/column indexes.
                int row_i = i / num_cols;
                int col_i = i % num_cols;
                int row_j = j / num_cols;
                int col_j = j % num_cols;

                // Swap cells i and j.
                temp.Source = img[row_i, col_i].Source;
                img[row_i, col_i].Source = img[row_j, col_j].Source;
                img[row_j, col_j].Source = temp.Source;
            }
        }
        //private void Button_Click_Shuffle(object sender, RoutedEventArgs e)
        //{
        //    Shuffle(ref images);
        //}
        private void ListViewMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GridMain.Children.Clear();

            switch (((ListViewItem)((ListView)sender).SelectedItem).Name)
            {
                case "OpenFile":
                    Button_Click(sender, e);
                    break;
                case "Camera":
                    Use_Photo(sender, e);
                    break;
                case "Shuffle":

                    break;
                default:
                    break;
            }
        }

    }

}
