﻿using System;
using Windows.Foundation;
using static Friendica_Mobile.UWP.PrintHelper;

namespace Friendica_Mobile.UWP
{
    public class PageDescription : IEquatable<PageDescription>
    {
        public Size Margin;
        public Size PageSize;
        public Size ViewablePageSize;
        public Size PictureViewSize;
        public bool IsContentCropped;
        public Placement PhotoPlacement;

        public bool Equals(PageDescription other)
        {
            // Detect if PageSize changed
            bool equal = (Math.Abs(PageSize.Width - other.PageSize.Width) < double.Epsilon) &&
                (Math.Abs(PageSize.Height - other.PageSize.Height) < double.Epsilon);

            // Detect if ViewablePageSize changed
            if (equal)
            {
                equal = (Math.Abs(ViewablePageSize.Width - other.ViewablePageSize.Width) < double.Epsilon) &&
                    (Math.Abs(ViewablePageSize.Height - other.ViewablePageSize.Height) < double.Epsilon);
            }

            // Detect if PictureViewSize changed
            if (equal)
            {
                equal = (Math.Abs(PictureViewSize.Width - other.PictureViewSize.Width) < double.Epsilon) &&
                    (Math.Abs(PictureViewSize.Height - other.PictureViewSize.Height) < double.Epsilon);
            }

            // Detect if cropping changed
            if (equal)
            {
                equal = IsContentCropped == other.IsContentCropped;
            }

            // detect if placement changed
            if (equal)
            {
                equal = PhotoPlacement == other.PhotoPlacement;
            }

            return equal;
        }
    }
}
