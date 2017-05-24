/****************************** Module Header ******************************\
 * Module Name:  PhotoSelectedRegion.cs
 * Project:      CSWindowsStoreAppCropBitmap
 * Copyright (c) Microsoft Corporation.
 * 
 * This class represents the selected region. It implements the INotifyPropertyChanged
 * interface and can be bound to the Xaml element
 *  
 * 
 * This source is subject to the Microsoft Public License.
 * See http://www.microsoft.com/en-us/openness/licenses.aspx#MPL
 * All other rights reserved.
 * 
 * THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
 * EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
 * WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
\***************************************************************************/

namespace Friendica_Mobile.Mvvm
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using Windows.Foundation;

    public class PhotoSelectedRegion : INotifyPropertyChanged
    {
        private const string TopLeftCornerCanvasLeftPropertyName = "TopLeftCornerCanvasLeft";
        private const string TopLeftCornerCanvasTopPropertyName = "TopLeftCornerCanvasTop";
        private const string BottomRightCornerCanvasLeftPropertyName = "BottomRightCornerCanvasLeft";
        private const string BottomRightCornerCanvasTopPropertyName = "BottomRightCornerCanvasTop";
        private const string OutterRectPropertyName = "OuterRect";
        private const string SelectedRectPropertyName = "SelectedRect";

        public const string TopLeftCornerName = "TopLeftCorner";
        public const string TopRightCornerName = "TopRightCorner";
        public const string BottomLeftCornerName = "BottomLeftCorner";
        public const string BottomRightCornerName = "BottomRightCorner";

        public event PropertyChangedEventHandler PropertyChanged;


        /// <summary>
        /// The minimum size of the selected region
        /// </summary>
        public double MinSelectRegionSize { get; set; }

        private double topLeftCornerCanvasLeft;

        /// <summary>
        /// The Canvas.Left property of the TopLeft corner.
        /// </summary>
        public double TopLeftCornerCanvasLeft
        {
            get { return topLeftCornerCanvasLeft; }
            protected set
            {
                if (topLeftCornerCanvasLeft != value)
                {
                    topLeftCornerCanvasLeft = value;
                    this.OnPropertyChanged(TopLeftCornerCanvasLeftPropertyName);
                }
            }
        }

        private double topLeftCornerCanvasTop;

        /// <summary>
        /// The Canvas.Top property of the TopLeft corner.
        /// </summary>
        public double TopLeftCornerCanvasTop
        {
            get { return topLeftCornerCanvasTop; }
            protected set
            {
                if (topLeftCornerCanvasTop != value)
                {
                    topLeftCornerCanvasTop = value;
                    this.OnPropertyChanged(TopLeftCornerCanvasTopPropertyName);
                }
            }
        }

        private double bottomRightCornerCanvasLeft;

        /// <summary>
        /// The Canvas.Left property of the BottomRight corner.
        /// </summary>
        public double BottomRightCornerCanvasLeft
        {
            get { return bottomRightCornerCanvasLeft; }
            protected set
            {
                if (bottomRightCornerCanvasLeft != value)
                {
                    bottomRightCornerCanvasLeft = value;

                    this.OnPropertyChanged(BottomRightCornerCanvasLeftPropertyName);
                }
            }
        }

        private double bottomRightCornerCanvasTop;

        /// <summary>
        /// The Canvas.Top property of the BottomRight corner.
        /// </summary>
        public double BottomRightCornerCanvasTop
        {
            get { return bottomRightCornerCanvasTop; }
            protected set
            {
                if (bottomRightCornerCanvasTop != value)
                {
                    bottomRightCornerCanvasTop = value;
                    this.OnPropertyChanged(BottomRightCornerCanvasTopPropertyName);
                }
            }
        }

        private Rect outerRect;

        /// <summary>
        /// The outer rect. The non-selected region can be represented by the 
        /// OuterRect and the SelectedRect.
        /// </summary>
        public Rect OuterRect
        {
            get { return outerRect; }
            set
            {
                if (outerRect != value)
                {
                    outerRect = value;

                    this.OnPropertyChanged(OutterRectPropertyName);
                }
            }
        }

        private Rect selectedRect;

        /// <summary>
        /// The selected region, which is represented by the four corners.
        /// </summary>
        public Rect SelectedRect
        {
            get { return selectedRect; }
            protected set
            {
                if (selectedRect != value)
                {
                    selectedRect = value;

                    this.OnPropertyChanged(SelectedRectPropertyName);
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }

            // When the corner is moved, update the SelectedRect.
            if (propertyName == TopLeftCornerCanvasLeftPropertyName ||
                propertyName == TopLeftCornerCanvasTopPropertyName ||
                propertyName == BottomRightCornerCanvasLeftPropertyName ||
                propertyName == BottomRightCornerCanvasTopPropertyName)
            {
                SelectedRect = new Rect(
                    TopLeftCornerCanvasLeft,
                    TopLeftCornerCanvasTop,
                    BottomRightCornerCanvasLeft - TopLeftCornerCanvasLeft,
                    BottomRightCornerCanvasTop - topLeftCornerCanvasTop);
            }
        }


        public void ResetCorner(double topLeftCornerCanvasLeft, double topLeftCornerCanvasTop,
            double bottomRightCornerCanvasLeft, double bottomRightCornerCanvasTop)
        {
            this.TopLeftCornerCanvasLeft = topLeftCornerCanvasLeft;
            this.TopLeftCornerCanvasTop = topLeftCornerCanvasTop;
            this.BottomRightCornerCanvasLeft = bottomRightCornerCanvasLeft;
            this.BottomRightCornerCanvasTop = bottomRightCornerCanvasTop;
        }

        /// <summary>
        /// Update the Canvas.Top and Canvas.Left of the corner.
        /// </summary>
        public void UpdateCorner(string cornerName, double leftUpdate, double topUpdate)
        {
            UpdateCorner(cornerName, leftUpdate, topUpdate, this.MinSelectRegionSize, this.MinSelectRegionSize);
        }

        /// <summary>
        /// Update the Canvas.Top and Canvas.Left of the corner.
        /// </summary>
        public void UpdateCorner(string cornerName, double leftUpdate, double topUpdate, double minWidthSize, double minHeightSize)
        {
            switch (cornerName)
            {
                case PhotoSelectedRegion.TopLeftCornerName:
                    if (topLeftCornerCanvasLeft + leftUpdate < 0 || topLeftCornerCanvasTop + topUpdate < 0)
                    {
                        leftUpdate = 0;
                        topUpdate = 0;
                    }

                    TopLeftCornerCanvasLeft = ValidateValue(topLeftCornerCanvasLeft + leftUpdate,
                        0, bottomRightCornerCanvasLeft - minWidthSize);
                    TopLeftCornerCanvasTop = ValidateValue(topLeftCornerCanvasTop + topUpdate,
                        0, bottomRightCornerCanvasTop - minHeightSize);
                    break;
                case PhotoSelectedRegion.TopRightCornerName:
                    if (bottomRightCornerCanvasLeft + leftUpdate < topLeftCornerCanvasLeft + minWidthSize || topLeftCornerCanvasTop + topUpdate * -1 < 0)
                    {
                        leftUpdate = 0;
                        topUpdate = 0;
                    }
                    BottomRightCornerCanvasLeft = ValidateValue(bottomRightCornerCanvasLeft + leftUpdate,
                        topLeftCornerCanvasLeft + minWidthSize, outerRect.Width);
                    TopLeftCornerCanvasTop = ValidateValue(topLeftCornerCanvasTop + topUpdate * -1,
                        0, bottomRightCornerCanvasTop - minHeightSize);
                    break;
                case PhotoSelectedRegion.BottomLeftCornerName:
                    if (topLeftCornerCanvasLeft + leftUpdate > bottomRightCornerCanvasLeft - minWidthSize || bottomRightCornerCanvasTop + topUpdate * -1 > outerRect.Height)
                    {
                        leftUpdate = 0;
                        topUpdate = 0;
                    }
                    TopLeftCornerCanvasLeft = ValidateValue(topLeftCornerCanvasLeft + leftUpdate,
                        0, bottomRightCornerCanvasLeft - minWidthSize);
                    BottomRightCornerCanvasTop = ValidateValue(bottomRightCornerCanvasTop + topUpdate * -1,
                        topLeftCornerCanvasTop + minHeightSize, outerRect.Height);
                    break;
                case PhotoSelectedRegion.BottomRightCornerName:
                    if (bottomRightCornerCanvasLeft + leftUpdate > outerRect.Width || bottomRightCornerCanvasTop + topUpdate > outerRect.Height)
                    {
                        leftUpdate = 0;
                        topUpdate = 0;
                    }
                    BottomRightCornerCanvasLeft = ValidateValue(bottomRightCornerCanvasLeft + leftUpdate,
                        topLeftCornerCanvasLeft + minWidthSize, outerRect.Width);
                    BottomRightCornerCanvasTop = ValidateValue(bottomRightCornerCanvasTop + topUpdate,
                        topLeftCornerCanvasTop + minHeightSize, outerRect.Height);
                    break;
                default:
                    throw new ArgumentException("cornerName: " + cornerName + "  is not recognized.");
            }

        }

        public void UpdateCorner2(string cornerName, double leftUpdate, double topUpdate, double minWidthSize, double minHeightSize)
        {
            switch (cornerName)
            {
                case PhotoSelectedRegion.TopLeftCornerName:
                    TopLeftCornerCanvasLeft = ValidateValue(topLeftCornerCanvasLeft + leftUpdate,
                        0, bottomRightCornerCanvasLeft - minWidthSize);
                    TopLeftCornerCanvasTop = ValidateValue(topLeftCornerCanvasTop + topUpdate,
                        0, bottomRightCornerCanvasTop - minHeightSize);
                    break;
                case PhotoSelectedRegion.TopRightCornerName:
                    BottomRightCornerCanvasLeft = ValidateValue(bottomRightCornerCanvasLeft + leftUpdate,
                        topLeftCornerCanvasLeft + minWidthSize, outerRect.Width);
                    TopLeftCornerCanvasTop = ValidateValue(topLeftCornerCanvasTop + topUpdate,
                        0, bottomRightCornerCanvasTop - minHeightSize);
                    break;
                case PhotoSelectedRegion.BottomLeftCornerName:
                    TopLeftCornerCanvasLeft = ValidateValue(topLeftCornerCanvasLeft + leftUpdate,
                        0, bottomRightCornerCanvasLeft - minWidthSize);
                    BottomRightCornerCanvasTop = ValidateValue(bottomRightCornerCanvasTop + topUpdate,
                        topLeftCornerCanvasTop + minHeightSize, outerRect.Height);
                    break;
                case PhotoSelectedRegion.BottomRightCornerName:
                    BottomRightCornerCanvasLeft = ValidateValue(bottomRightCornerCanvasLeft + leftUpdate,
                        topLeftCornerCanvasLeft + minWidthSize, outerRect.Width);
                    BottomRightCornerCanvasTop = ValidateValue(bottomRightCornerCanvasTop + topUpdate,
                        topLeftCornerCanvasTop + minHeightSize, outerRect.Height);
                    break;
                default:
                    throw new ArgumentException("cornerName: " + cornerName + "  is not recognized.");
            }

        }


        private double ValidateValue(double tempValue, double from, double to)
        {
            if (tempValue < from)
            {
                tempValue = from;
            }

            if (tempValue > to)
            {
                tempValue = to;
            }

            return tempValue;
        }

        /// <summary>
        /// Update the SelectedRect when it is moved or scaled.
        /// </summary>
        public void UpdateSelectedRect(double scale, double leftUpdate, double topUpdate)
        {
            double width = bottomRightCornerCanvasLeft - topLeftCornerCanvasLeft;
            double height = bottomRightCornerCanvasTop - topLeftCornerCanvasTop;

            double minWidth = width;
            double minHeight = height;

            // Move towards BottomRight: Move BottomRightCorner first, and then move TopLeftCorner.
            if (leftUpdate >= 0 && topUpdate >= 0)
            {
                this.UpdateCorner2(PhotoSelectedRegion.BottomRightCornerName, leftUpdate, topUpdate, minWidth, minHeight);
                this.UpdateCorner2(PhotoSelectedRegion.TopLeftCornerName, leftUpdate, topUpdate, minWidth, minHeight);
            }

            // Move towards TopRight: Move TopRightCorner first, and then move BottomLeftCorner.
            else if (leftUpdate >= 0 && topUpdate < 0)
            {
                this.UpdateCorner2(PhotoSelectedRegion.TopRightCornerName, leftUpdate, topUpdate, minWidth, minHeight);
                this.UpdateCorner2(PhotoSelectedRegion.BottomLeftCornerName, leftUpdate, topUpdate, minWidth, minHeight);
            }

            // Move towards BottomLeft: Move BottomLeftCorner first, and then move TopRightCorner.
            else if (leftUpdate < 0 && topUpdate >= 0)
            {
                this.UpdateCorner2(PhotoSelectedRegion.BottomLeftCornerName, leftUpdate, topUpdate, minWidth, minHeight);
                this.UpdateCorner2(PhotoSelectedRegion.TopRightCornerName, leftUpdate, topUpdate, minWidth, minHeight);
            }

            // Move towards TopLeft: Move TopLeftCorner first, and then move BottomRightCorner.
            else if (leftUpdate < 0 && topUpdate < 0)
            {
                this.UpdateCorner2(PhotoSelectedRegion.TopLeftCornerName, leftUpdate, topUpdate, minWidth, minHeight);
                this.UpdateCorner2(PhotoSelectedRegion.BottomRightCornerName, leftUpdate, topUpdate, minWidth, minHeight);
            }
        }


    }
}
