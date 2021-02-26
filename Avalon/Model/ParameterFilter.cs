namespace Avalon.Model
{
    public class ParameterFilter
    {
        const int maxPageSize = 50;

        public OrderByType OrderByType { get; set; } = OrderByType.CreatedOn;

        public int PageNumber { get; set; } = 1;

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
