﻿namespace Avalon.Model
{
    public class ParameterFilter
    {
        const int maxPageSize = 50;

        public OrderByType OrderByType { get; set; } = OrderByType.CreatedOn;

        public string SortDirection { get; set; } = "desc";

        private int _pageIndex = 1;

        public int PageIndex
        {
            get 
            { 
                return _pageIndex; 
            }
            set 
            { 
                _pageIndex = (value < 1) ? 1 : value; 
            }
        }

        private int _pageSize = 10;

        public int PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                _pageSize = (value > maxPageSize) ? maxPageSize : value;
            }
        }

    }
}
