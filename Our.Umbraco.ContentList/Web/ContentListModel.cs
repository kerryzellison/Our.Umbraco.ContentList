using System;
using System.Collections.Generic;
using Our.Umbraco.ContentList.DataSources;

namespace Our.Umbraco.ContentList.Web
{
    public class ContentListModel
    {
        public ContentListConfiguration Configuration { get; set; }
        public IEnumerable<IListableContent> Items { get; set; }
        public ContentListColumnStyling ColumnStyling { get; set; }
        public ContentListPaging Paging { get; set; }
        public string Hash { get; set; }
        public ContentListQuery Query { get; set; }
    }

    public class ContentListPaging
    {
        public long Page { get; set; }
        public long PageSize { get; set; }
        public long From { get; set; }
        public long To { get; set; }
        public long Total { get; set; }
        public bool ShowPaging { get; set; }

        public long Pages
        {
            get
            {
                var pages = Total / PageSize;
                pages += Total % PageSize > 0 ? 1 : 0;
                return pages;
            }
        }
    }

    public class ContentListColumnStyling
    {
        private readonly ContentListColumns columns;

        public ContentListColumnStyling(ContentListColumns columns, int max = 12)
        {
            this.columns = columns;
            if (columns != null)
            {
                SmallSize = (max / Math.Max(columns.Small, 1));
                MediumSize = (max / Math.Max(columns.Medium, 1));
                LargeSize = (max / Math.Max(columns.Large, 1));
            }
        }

        public int SmallSize { get; set; }
        public int MediumSize { get; set; }
        public int LargeSize { get; set; }

        public string ColumnClasses(string smallPrefix, string mediumPrefix, string largePrefix, string suffix = "")
        {
            return SmallClass(smallPrefix, suffix) + " " +
                   MediumClass(mediumPrefix, suffix) + " " +
                   LargeClass(largePrefix, suffix);
        }

        public string SmallClass(string prefix, string suffix = "")
        {
            return Class(prefix, suffix, SmallSize);
        }

        public string MediumClass(string prefix, string suffix = "")
        {
            return Class(prefix, suffix, MediumSize);
        }

        public string LargeClass(string prefix, string suffix = "")
        {
            return Class(prefix, suffix, LargeSize);
        }

        private string Class(string prefix, string suffix, int size)
        {
            return prefix + size + suffix;
        }

        public bool IsSmallBreak(int index)
        {
            return IsFactor(index, columns.Small);
        }

        public bool IsMediumBreak(int index)
        {
            return IsFactor(index, columns.Medium);
        }

        public bool IsLargeBreak(int index)
        {
            return IsFactor(index, columns.Large);
        }

        private static bool IsFactor(int index, int divisor)
        {
            return (index + 1) % divisor == 0;
        }
    }
}
