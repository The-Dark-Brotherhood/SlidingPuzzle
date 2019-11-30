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
using System.IO;



// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SlidingPuzzle
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 
    public sealed partial class MainPage : Page
    {
        ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
    
    private string ImageSource { get; set; }

        List<Button> AllGridPanels = null;
        List<Tuple<int, int>> statePosition = new List<Tuple<int, int>>();
        List<Tuple<int, int>> winPosition = new List<Tuple<int, int>>()
        {
            Tuple.Create(0, 0),
            Tuple.Create(0, 1),
            Tuple.Create(0, 2),
            Tuple.Create(0, 3),
            Tuple.Create(1, 0),
            Tuple.Create(1, 1),
            Tuple.Create(1, 2),
            Tuple.Create(1, 3),
            Tuple.Create(2, 0),
            Tuple.Create(2, 1),
            Tuple.Create(2, 2),
            Tuple.Create(2, 3),
            Tuple.Create(3, 0),
            Tuple.Create(3, 1),
            Tuple.Create(3, 2),
            Tuple.Create(3, 3)
        };

        public MainPage()
        {
            this.InitializeComponent();
            AllGridPanels = new List<Button>();
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
            AllGridPanels.Add(blankButton);
            ImageSource = @"https://media.giphy.com/media/Is1O1TWV0LEJi/giphy.gif";
        }


        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            // Reference: https://docs.microsoft.com/en-us/windows/uwp/audio-video-camera/imaging
            // Select image
            FileOpenPicker fileOpenPicker = new FileOpenPicker();
            fileOpenPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            fileOpenPicker.FileTypeFilter.Add(".png");
            fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;

            StorageFile inputFile = await fileOpenPicker.PickSingleFileAsync();

            if (inputFile == null)
            {
                // The user cancelled the picking operation
                return;
            }
 
            SoftwareBitmap softwareBitmap = await LoadBitmapAsync(inputFile);
            softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

            // Send to source to display
            SoftwareBitmapSource originalSource = new SoftwareBitmapSource();
            await originalSource.SetBitmapAsync(softwareBitmap);
            OriginalImage.Source = originalSource;

            // Crop and Randomize
            await CropImagesAsync(softwareBitmap);

            if(inputFile.Path != localSettings.Values["LastImagePath"] as string)       // If the image being loaded is a current game -> Do NOT RANDOMIZE 
            {
                // Save state image into local settings -> Used for recovering state
                localSettings.Values["LastImagePath"] = (string)inputFile.Path;
                RandomizeTiles();
                SavePositionState(GetCurrentPosition());
            }
            else
            {
                LoadPrevious();
            }
        }

        private async Task<SoftwareBitmap> LoadBitmapAsync(StorageFile inputFile)
        {
            SoftwareBitmap softwareBitmap;
            using (IRandomAccessStream stream = await inputFile.OpenAsync(FileAccessMode.Read))
            {
                // Create the decoder from the stream
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);

                // Get the SoftwareBitmap representation of the file
                softwareBitmap = await decoder.GetSoftwareBitmapAsync();
            }

            return softwareBitmap;
        }

        private void RandomizeTiles()
        {
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

            for (int counter = 0; counter < AllGridPanels.Count - 1; counter++)
            {
                int number = rng.Next(randomPos.Count);

                AllGridPanels[counter].SetValue(Grid.RowProperty, (int)randomPos[number].Item1);
                AllGridPanels[counter].SetValue(Grid.ColumnProperty, (int)randomPos[number].Item2);

                randomPos.RemoveAt(number);
            }

            // Reset blank square
            AllGridPanels[AllGridPanels.Count - 1].SetValue(Grid.RowProperty, 3);
            AllGridPanels[AllGridPanels.Count - 1].SetValue(Grid.ColumnProperty, 3);
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

                    var brush = new ImageBrush();
                    brush.ImageSource = source;

                    AllGridPanels[counter].Background = brush;
                }
            }

            gameContainer.Visibility = Visibility.Visible;

        }

        // Title movement button handler
        private void notBlank_Click(object sender, RoutedEventArgs e)
        {
            Button image = (Button)sender;
            Button blankSpace = this.blankButton;
            List<Tuple<int, int>> currentPositions = new List<Tuple<int, int>>();

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

                ClickCount++;
            }

            currentPositions = CheckWin();
            if(currentPositions.Count == 0)             // Win scenario -> 0 memebers
            {
                ShowPopupOffsetClicked(sender, e);
                localSettings.Values["LastImagePath"] = null;
            }
            else
            {
                SavePositionState(currentPositions);    // Lose scenario -> Save state
            }

        private List<Tuple<int, int>> CheckWin()
        {
            bool isWin = true;
            List<Tuple<int, int>> currentPositions = GetCurrentPosition();

            for (int counter = 0; counter < currentPositions.Count; counter++)
            {
                if (!currentPositions[counter].Equals(winPosition[counter]))
                {
                    isWin = false;
                    break;
                }
            }

            if(isWin)           // Return a list with 0 members
            {
                currentPositions.Clear();
            }

            return currentPositions;
        }

        private List<Tuple<int, int>> GetCurrentPosition()
        {
            List <Tuple<int, int>> currentPositions = new List<Tuple<int, int>>();
            foreach (Button cropImg in AllGridPanels)
            {
                currentPositions.Add(new Tuple<int, int>
                (
                    (int)cropImg.GetValue(Grid.RowProperty),
                    (int)cropImg.GetValue(Grid.ColumnProperty)
                ));
            }

            return currentPositions;
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

        private void SavePositionState(List<Tuple<int, int>> currentPositions)
        {
            for (int counter = 0; counter < currentPositions.Count; counter++)
            {
                localSettings.Values[counter.ToString()] = currentPositions[counter].Item1 + "," + currentPositions[counter].Item2;
            }
        }

        private void LoadPrevious()
        {
            // Check if there was a previous game
            String lastImage = localSettings.Values["LastImagePath"] as string;
            if (lastImage == null)
            {
                return;
            }

            // Load the position state data
            for (int counter = 0; counter < AllGridPanels.Count; counter++)
            {
                string conversion = (string)localSettings.Values[counter.ToString()];
                if (conversion == null)
                {
                    break;
                }
                string[] values = conversion.Split(',');
                Tuple<int, int> newTuple = new Tuple<int, int>(Int32.Parse(values[0]), Int32.Parse(values[1]));
                statePosition.Add(newTuple);
            }

            // Load the postion state
            for (int counter = 0; counter < AllGridPanels.Count; counter++)
            {
                AllGridPanels[counter].SetValue(Grid.RowProperty, statePosition[counter].Item1);
                AllGridPanels[counter].SetValue(Grid.ColumnProperty, statePosition[counter].Item2);
            }
        }

        // CAMERA CAPABILITY FUNCTION
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

        private void ClosePopupClicked(object sender, RoutedEventArgs e)
        {
            // if the Popup is open, then close it 
            if (StandardPopup.IsOpen) { StandardPopup.IsOpen = false; }
        }

        // Handles the Click event on the Button on the page and opens the Popup. 
        private void ShowPopupOffsetClicked(object sender, RoutedEventArgs e)
        {
            // open the Popup if it isn't open already 
            if (!StandardPopup.IsOpen) { StandardPopup.IsOpen = true; }


        }
    }
}
