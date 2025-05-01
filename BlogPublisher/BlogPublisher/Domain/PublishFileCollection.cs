using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BlogPublisher.Domain;

public class PublishFileCollection : IEnumerable<string>
{
    private readonly List<string> _files = new();

    public PublishFileCollection(string publishFolder)
    {
        PublishFolder = publishFolder.TrimEnd('\\');
    }

    public int Count => _files.Count;

    public string PublishFolder { get; }

    public void Add(string file)
    {
        _files.Add(file);
    }

    public IReadOnlyList<string> GetInvalidationPath()
    {
        var setting = Setting.GetInstance();
        if (setting.PublishChangedFileOnly)
        {
            return _files
                .Select(GetInvalidationPath)
                .OrderBy(f => f)
                .ToList();
        }
        
        // HACK 考虑经济性的话，固定返回 /* 或许更好 https://docs.aws.amazon.com/zh_cn/AmazonCloudFront/latest/DeveloperGuide/PayingForInvalidation.html
        return new List<string> { "/*" };
    }

    private string GetInvalidationPath(string path)
    {
        return path.Substring(PublishFolder.Length).Replace('\\', '/');
    }

    public IEnumerator<string> GetEnumerator()
    {
        return _files.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}