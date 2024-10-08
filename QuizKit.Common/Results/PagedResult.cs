namespace QuizKit.Common.Results;

public record PagedResult<T> : Result<List<T>>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)Math.Max(1, PageSize));

    public static implicit operator PagedResult<T>(Failure failure)
            => new()
            {
                Errors = failure.Errors,
                Message = failure.Message,
                Status = failure.Status,
            };
}
