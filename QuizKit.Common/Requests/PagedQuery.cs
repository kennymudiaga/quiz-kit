using QuizKit.Common.Constants;

namespace QuizKit.Common.Requests;

public abstract record PagedQuery
{
    private int _page;
    private int _pageSize;

    public int Page
    {
        get => _page;
        init => _page = value > 0 ? value : PaginationConstants.DefaultPage;
    }

    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = value > 0 ? value : PaginationConstants.DefaultPageSize;
    }
}
