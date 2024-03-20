namespace FacebookPosting.Abstractions;

public interface IPostEntity
{
    FacebookPostData PostData { get; }

    FacebookGroupData FacebookGroupData { get; }
}