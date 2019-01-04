using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Graphics.Printing;
using Windows.Graphics.Printing.OptionDetails;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Printing;

namespace Friendica_Mobile.UWP
{
    public class PrintHelper
    {
        ResourceLoader loader = ResourceLoader.GetForCurrentView();

        // property retrieving photo data from flyout to use in the print process
        private BitmapImage _photoData;
        public BitmapImage PhotoData
        {
            get { return _photoData; }
            set
            {
                _photoData = value;
                // this class is persistent over flipping through images, so we clear previous settings if new photo data is provided
                ClearPageCollection();
                currentPageDescription = null;
                // reset setting to the standards (always first entry in dropdown selections)
                photoSize = PhotoSize.Size9x13;
                photoScale = Scaling.ShrinkToFit;
                photoPlacement = Placement.Center;
            }
        }

        // needed for calculating the size of the image depending on the selected size
        private const int DPI96 = 96;

        // enums with the possible selections for user
        public enum PhotoSize : byte
        {
            SizeFullPage,  //8x12 in
            Size9x13,   // 3,5x5,25 in
            Size10x15, // 4x6 in
            Size13x18  // 5x7 in
        }

        public enum Scaling : byte
        {
            ShrinkToFit,
            Crop
        }

        public enum Placement : byte
        {
            Center,
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }

        // stores the selection of the user
        private PhotoSize photoSize;
        private Scaling photoScale;
        private Placement photoPlacement;

        // used for locking the page object
        private static object printSync = new object();
        private long requestCount;

        // properties for storing the created pages, print documents, settings etc.
        private Dictionary<int, UIElement> pageCollection = new Dictionary<int, UIElement>();
        private PageDescription currentPageDescription;
        protected PrintDocument printDocument;
        protected IPrintDocumentSource printDocumentSource;
        protected Page scenarioPage;


        public PrintHelper(Page scenarioPage)
        {
            // provide photo page - used for dispatcher operations on finalized print task
            this.scenarioPage = scenarioPage;
        }


        public virtual void RegisterForPrinting()
        {
            printDocument = new PrintDocument();
            printDocumentSource = printDocument.DocumentSource;
            printDocument.Paginate += CreatePrintPreviewPages;
            printDocument.GetPreviewPage += GetPrintPreviewPage;
            printDocument.AddPages += AddPrintPages;

            PrintManager printMan = PrintManager.GetForCurrentView();
            printMan.PrintTaskRequested += PrintTaskRequested;
        }


        public virtual void UnregisterForPrinting()
        {
            if (printDocument == null)
                return;

            printDocument.Paginate -= CreatePrintPreviewPages;
            printDocument.GetPreviewPage -= GetPrintPreviewPage;
            printDocument.AddPages -= AddPrintPages;

            PrintManager printMan = PrintManager.GetForCurrentView();
            printMan.PrintTaskRequested -= PrintTaskRequested;
        }


        protected virtual void PrintTaskRequested(PrintManager sender, PrintTaskRequestedEventArgs e)
        {
            PrintTask printTask = null;

            // Friendica Mobile is used as the print dialog header and as a standard filename on Windows 10 Mobile when printing
            // as a PDF (user is not asked on Mobile for a filename)
            printTask = e.Request.CreatePrintTask("Friendica Mobile", sourceRequested =>
            {
                PrintTaskOptionDetails printDetailedOptions = PrintTaskOptionDetails.GetFromPrintTaskOptions(printTask.Options);

                // Choose the printer options to be shown
                // The order in which the options are appended determines the order in which they appear in the UI
                printDetailedOptions.DisplayedOptions.Clear();
                printDetailedOptions.DisplayedOptions.Add(StandardPrintTaskOptions.MediaSize);
                printDetailedOptions.DisplayedOptions.Add(StandardPrintTaskOptions.Copies);

                // Create a new list option for photo size.
                PrintCustomItemListOptionDetails photoSize = printDetailedOptions.CreateItemListOption("photoSize", loader.GetString("textPhotosPrintDialogPhotoSizeHeader"));
                photoSize.AddItem("Size9x13", loader.GetString("textPhotosPrintDialogPhotoSize9x13"));
                photoSize.AddItem("Size10x15", loader.GetString("textPhotosPrintDialogPhotoSize10x15"));
                photoSize.AddItem("Size13x18", loader.GetString("textPhotosPrintDialogPhotoSize13x18"));
                photoSize.AddItem("SizeFullPage", loader.GetString("textPhotosPrintDialogPhotoSizeFullPage"));
                printDetailedOptions.DisplayedOptions.Add("photoSize");

                // create a new list option for scaling options
                PrintCustomItemListOptionDetails scaling = printDetailedOptions.CreateItemListOption("scaling", loader.GetString("textPhotosPrintDialogPhotoScalingHeader"));
                scaling.AddItem("ShrinkToFit", loader.GetString("textPhotosPrintDialogPhotoScalingShrinkToFit"));
                scaling.AddItem("Crop", loader.GetString("textPhotosPrintDialogPhotoScalingCrop"));
                printDetailedOptions.DisplayedOptions.Add("scaling");

                // create a new list option for placement of picture
                PrintCustomItemListOptionDetails placement = printDetailedOptions.CreateItemListOption("placement", loader.GetString("textPhotosPrintDialogPlacementHeader"));
                placement.AddItem("Center", loader.GetString("textPhotosPrintDialogPlacementCenter"));
                placement.AddItem("TopLeft", loader.GetString("textPhotosPrintDialogPlacementTopLeft"));
                placement.AddItem("TopRight", loader.GetString("textPhotosPrintDialogPlacementTopRight"));
                placement.AddItem("BottomLeft", loader.GetString("textPhotosPrintDialogPlacementBottomLeft"));
                placement.AddItem("BottomRight", loader.GetString("textPhotosPrintDialogPlacementBottomRight"));
                printDetailedOptions.DisplayedOptions.Add("placement");

                // Set default orientation to landscape.
                printTask.Options.Orientation = PrintOrientation.Landscape;

                // Register for print task option changed notifications.
                printDetailedOptions.OptionChanged += PrintDetailedOptionsOptionChanged;

                // Register for print task Completed notification.
                // Print Task event handler is invoked when the print job is completed.
                printTask.Completed += async (s, args) =>
                {
                    await scenarioPage.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                    {
                        ClearPageCollection();

                        // Reset image options to default values.
                        this.photoScale = Scaling.ShrinkToFit;
                        this.photoSize = PhotoSize.SizeFullPage;
                        this.photoPlacement = Placement.Center;

                        // Reset the current page description
                        currentPageDescription = null;

                        // Notify the user when the print operation fails.
                        if (args.Completion == PrintTaskCompletion.Failed)
                        {
                            string errorMsg = loader.GetString("messageDialogPhotosErrorOnPrinting");
                            var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
                            await dialog.ShowDialog(0, 0);
                        }
                    });
                };

                sourceRequested.SetSource(printDocumentSource);
            });
        }


        private void ClearPageCollection()
        {
            lock (printSync)
            {
                pageCollection.Clear();
            }
        }


        private async void PrintDetailedOptionsOptionChanged(PrintTaskOptionDetails sender, PrintTaskOptionChangedEventArgs args)
        {
            bool invalidatePreview = false;

            // For this scenario we are interested only when the 3 custom options change (photoSize & scaling & placement) in order 
            // to trigger a preview refresh. Default options that change page aspect will trigger preview invalidation (refresh) 
            // automatically. It is safe to ignore verifying other options and(or) combinations here because during Paginate 
            // event(CreatePrintPreviewPages) we check if the PageDescription changed.
            if (args.OptionId == null)
                return;

            string optionId = args.OptionId.ToString();

            if (optionId == "photoSize")
            {
                IPrintOptionDetails photoSizeOption = sender.Options[optionId];
                string photoSizeValue = photoSizeOption.Value as string;

                if (!string.IsNullOrEmpty(photoSizeValue))
                {
                    switch (photoSizeValue)
                    {
                        case "SizeFullPage":
                            photoSize = PhotoSize.SizeFullPage;
                            break;
                        case "Size9x13":
                            photoSize = PhotoSize.Size9x13;
                            break;
                        case "Size10x15":
                            photoSize = PhotoSize.Size10x15;
                            break;
                        case "Size13x18":
                            photoSize = PhotoSize.Size13x18;
                            break;
                    }
                    invalidatePreview = true;
                }
            }

            if (optionId == "scaling")
            {
                IPrintOptionDetails scalingOption = sender.Options[optionId];
                string scalingValue = scalingOption.Value as string;

                if (!string.IsNullOrEmpty(scalingValue))
                {
                    switch (scalingValue)
                    {
                        case "Crop":
                            photoScale = Scaling.Crop;
                            break;
                        case "ShrinkToFit":
                            photoScale = Scaling.ShrinkToFit;
                            break;
                    }
                    invalidatePreview = true;
                }
            }

            if (optionId == "placement")
            {
                IPrintOptionDetails placementOption = sender.Options[optionId];
                string placementValue = placementOption.Value as string;

                if (!string.IsNullOrEmpty(placementValue))
                {
                    switch (placementValue)
                    {
                        case "Center":
                            photoPlacement = Placement.Center;
                            break;
                        case "TopLeft":
                            photoPlacement = Placement.TopLeft;
                            break;
                        case "TopRight":
                            photoPlacement = Placement.TopRight;
                            break;
                        case "BottomLeft":
                            photoPlacement = Placement.BottomLeft;
                            break;
                        case "BottomRight":
                            photoPlacement = Placement.BottomRight;
                            break;
                    }
                    invalidatePreview = true;
                }
            }

            // Invalidate preview if one of the 3 options (photoSize, scaling, placement) changed.
            if (invalidatePreview)
            {
                await scenarioPage.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, printDocument.InvalidatePreview);
            }
        }


        private async void GetPrintPreviewPage(object sender, GetPreviewPageEventArgs e)
        {
            // Store a local copy of the request count to use later to determine if the computed page is out of date.
            // If the page preview is unavailable an async operation will generate the content.
            // When the operation completes there is a chance that a pagination request was already made therefore making this page obsolete.
            // If the page is obsolete throw away the result (don't call SetPreviewPage) since a new GetPrintPreviewPage will server that request.
            long requestNumber = 0;
            Interlocked.Exchange(ref requestNumber, requestCount);
            if (e.PageNumber != 1)
                throw new IndexOutOfRangeException();
            int pageNumber = 1; // we can fix the number to 1 as for photo printing we have always one single page

            UIElement page;
            bool pageReady = false;

            // Try to get the page if it was previously generated.
            lock (printSync)
            {
                pageReady = pageCollection.TryGetValue(pageNumber - 1, out page);
            }

            if (!pageReady)
            {
                // The page is not available yet.
                page = await GeneratePageAsync(pageNumber, currentPageDescription);

                // If the ticket changed discard the result since the content is outdated.
                if (Interlocked.CompareExchange(ref requestNumber, requestNumber, requestCount) != requestCount)
                    return;

                lock (printSync)
                {
                    pageCollection[pageNumber - 1] = page;
                }
            }

            PrintDocument printDoc = (PrintDocument)sender;

            // Send the page to preview.
            printDoc.SetPreviewPage(pageNumber, page);
        }


        private async Task<UIElement> GeneratePageAsync(int photoNumber, PageDescription pageDescription)
        {
            // created content: Image "image" -> Grid "photoView" -> Canvas "viewablePage" -> Canvas "page"

            Canvas page = new Canvas
            {
                Width = pageDescription.PageSize.Width,
                Height = pageDescription.PageSize.Height
            };

            Canvas viewablePage = new Canvas()
            {
                Width = pageDescription.ViewablePageSize.Width,
                Height = pageDescription.ViewablePageSize.Height
            };

            viewablePage.SetValue(Canvas.LeftProperty, pageDescription.Margin.Width);
            viewablePage.SetValue(Canvas.TopProperty, pageDescription.Margin.Height);

            // The image "frame" which also acts as a viewport
            Grid photoView = new Grid
            {
                Width = pageDescription.PictureViewSize.Width,
                Height = pageDescription.PictureViewSize.Height
            };

            // place the "photo" as user selected in options (in fact we place the surrounding Grid "photoView")
            switch (photoPlacement)
            {
                case Placement.Center:
                    photoView.SetValue(Canvas.LeftProperty, (viewablePage.Width - photoView.Width) / 2);
                    photoView.SetValue(Canvas.TopProperty, (viewablePage.Height - photoView.Height) / 2);
                    break;
                case Placement.TopLeft:
                    photoView.SetValue(Canvas.LeftProperty, 0);
                    photoView.SetValue(Canvas.TopProperty, 0);
                    break;
                case Placement.TopRight:
                    photoView.SetValue(Canvas.LeftProperty, (viewablePage.Width - photoView.Width));
                    photoView.SetValue(Canvas.TopProperty, 0);
                    break;
                case Placement.BottomLeft:
                    photoView.SetValue(Canvas.LeftProperty, 0);
                    photoView.SetValue(Canvas.TopProperty, (viewablePage.Height - photoView.Height));
                    break;
                case Placement.BottomRight:
                    photoView.SetValue(Canvas.LeftProperty, (viewablePage.Width - photoView.Width));
                    photoView.SetValue(Canvas.TopProperty, (viewablePage.Height - photoView.Height));
                    break;
            }

            if (PhotoData != null)
            {
                VerticalAlignment verticalAlignment = VerticalAlignment.Center;

                // in case of fullsize of a landscape image in a portrait page, the image would always be centered
                // this is confusing for user selecting top or bottom positions, therefore we need to change it here
                switch (photoPlacement)
                {
                    case Placement.TopLeft:
                    case Placement.TopRight:
                        verticalAlignment = VerticalAlignment.Top;
                        break;
                    case Placement.BottomLeft:
                    case Placement.BottomRight:
                        verticalAlignment = VerticalAlignment.Bottom;
                        break;
                }

                Image image = new Image
                {
                    Source = PhotoData,
                    // by default the image is always centered within its frame (the frame is sized indeed)
                    HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center,
                    VerticalAlignment = verticalAlignment
                };

                // Use the real image size when croping or if the image is smaller than the target area (prevent a scale-up)
                if (photoScale == Scaling.Crop || (PhotoData.PixelWidth <= pageDescription.PictureViewSize.Width &&
                    PhotoData.PixelHeight <= pageDescription.PictureViewSize.Height))
                {
                    image.Stretch = Stretch.None;
                    image.Width = PhotoData.PixelWidth;
                    image.Height = PhotoData.PixelHeight;
                }

                // Add the newly created image to the visual root.
                photoView.Children.Add(image);
                viewablePage.Children.Add(photoView);
                page.Children.Add(viewablePage);
            }

            // Return the page with the image centered.
            return page;
        }
        

        private async void AddPrintPages(object sender, AddPagesEventArgs e)
        {
            PrintDocument printDoc = (PrintDocument)sender;

            // Loop over all of the preview pages - we do have only one for photos, therefore fixed to 1
            for (int i = 0; i < 1; i++)
            {
                UIElement page = null;
                bool pageReady = false;

                lock (printSync)
                {
                    pageReady = pageCollection.TryGetValue(i, out page);
                }

                if (!pageReady)
                {
                    // If the page is not ready create a task that will generate its content.
                    page = await GeneratePageAsync(i + 1, currentPageDescription);
                }

                printDoc.AddPage(page);
            }

            // Indicate that all of the print pages have been provided.
            printDoc.AddPagesComplete();

            // Reset the current page description as soon as possible since the PrintTask.Completed event might fire later (long running job)
            currentPageDescription = null;
        }


        private void CreatePrintPreviewPages(object sender, PaginateEventArgs e)
        {
            PrintDocument printDoc = (PrintDocument)sender;
            
            // A new "session" starts with each paginate event.
            Interlocked.Increment(ref requestCount);

            PageDescription pageDescription = new PageDescription();

            // Get printer's page description.
            PrintTaskOptionDetails printDetailedOptions = PrintTaskOptionDetails.GetFromPrintTaskOptions(e.PrintTaskOptions);
            PrintPageDescription printPageDescription = e.PrintTaskOptions.GetPageDescription(0);

            // Reset the error state
            printDetailedOptions.Options["photoSize"].ErrorText = string.Empty;

            // Compute the printing page description (page size & center printable area)
            pageDescription.PageSize = printPageDescription.PageSize;

            pageDescription.Margin.Width = Math.Max(
                printPageDescription.ImageableRect.Left,
                printPageDescription.ImageableRect.Right - printPageDescription.PageSize.Width);

            pageDescription.Margin.Height = Math.Max(
                printPageDescription.ImageableRect.Top,
                printPageDescription.ImageableRect.Bottom - printPageDescription.PageSize.Height);

            pageDescription.ViewablePageSize.Width = printPageDescription.PageSize.Width - pageDescription.Margin.Width * 2;
            pageDescription.ViewablePageSize.Height = printPageDescription.PageSize.Height - pageDescription.Margin.Height * 2;

            // Compute print photo area - depending on the orientation of the photo (we keep the orientation)
            double width = 0.0;
            double height = 0.0;
            switch (photoSize)
            {
                case PhotoSize.Size9x13:
                    width = ((PhotoData.PixelWidth > PhotoData.PixelHeight) ? 5.25 : 3.5) * DPI96;
                    height = ((PhotoData.PixelWidth > PhotoData.PixelHeight) ? 3.5 : 5.25) * DPI96;
                    break;
                case PhotoSize.Size10x15:
                    width = ((PhotoData.PixelWidth > PhotoData.PixelHeight) ? 6 : 4) * DPI96;
                    height = ((PhotoData.PixelWidth > PhotoData.PixelHeight) ? 4 : 6) * DPI96;
                    break;
                case PhotoSize.Size13x18:
                    width = ((PhotoData.PixelWidth > PhotoData.PixelHeight) ? 7 : 5) * DPI96;
                    height = ((PhotoData.PixelWidth > PhotoData.PixelHeight) ? 5 : 7) * DPI96;
                    break;
                case PhotoSize.SizeFullPage:
                    width = pageDescription.ViewablePageSize.Width;
                    height = pageDescription.ViewablePageSize.Height;
                    break;
            }
            pageDescription.PictureViewSize.Width = width;
            pageDescription.PictureViewSize.Height = height;

            // set scale and placement from user selection
            pageDescription.IsContentCropped = photoScale == Scaling.Crop;
            pageDescription.PhotoPlacement = photoPlacement;

            // Recreate content only when :
            // - there is no current page description
            // - the current page description doesn't match the new one
            if (currentPageDescription == null || !currentPageDescription.Equals(pageDescription))
            {
                ClearPageCollection();

                if (pageDescription.PictureViewSize.Width > pageDescription.ViewablePageSize.Width ||
                    pageDescription.PictureViewSize.Height > pageDescription.ViewablePageSize.Height)
                {
                    printDetailedOptions.Options["photoSize"].ErrorText = loader.GetString("textPhotosPrintPhotoSizeBiggerThanPage");

                    // Inform preview that it has only 1 page to show.
                    printDoc.SetPreviewPageCount(1, PreviewPageCountType.Intermediate);

                    // Add a custom "preview" unavailable page
                    lock (printSync)
                    {
                        pageCollection[0] = new PreviewUnavailable(pageDescription.PageSize, pageDescription.ViewablePageSize);
                    }
                }
                else
                {
                    if (PhotoData.PixelWidth <= pageDescription.PictureViewSize.Width &&
                        PhotoData.PixelHeight <= pageDescription.PictureViewSize.Height)
                    {
                        printDetailedOptions.Options["photoSize"].ErrorText = loader.GetString("textPhotosPrintSizeBiggerThanPhotoSize");
                    }

                    // Inform preview that is has #NumberOfPhotos pages to show.
                    printDoc.SetPreviewPageCount(1, PreviewPageCountType.Intermediate);
                }

                currentPageDescription = pageDescription;
            }
       }

    }
}
