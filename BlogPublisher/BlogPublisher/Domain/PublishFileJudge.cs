using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace BlogPublisher.Domain;

public class PublishFileJudge
{
    private readonly Dictionary<string, string> _dict;

    public PublishFileJudge()
    {
        _dict = LoadPublishFileHashDictionary();
    }

    public PublishFileCollection GetPublishFiles()
    {
        var setting = Setting.GetInstance();
        ResetPublishFolder(setting);
        var files = new PublishFileCollection(setting.PublishDirectory);
        foreach (var file in Directory.GetFiles(setting.LocalBlogDirectory, "*.*", SearchOption.AllDirectories))
        {
            if (_dict.ContainsKey(file))
            {
                var fileHash = GetHash(file);
                if (string.CompareOrdinal(_dict[file], fileHash) != 0 ||
                    !setting.PublishChangedFileOnly)
                {
                    _dict[file] = fileHash;
                    var publishFile = MoveToPublishDirectory(file);
                    files.Add(publishFile);
                }
            }
            else
            {
                _dict.Add(file, GetHash(file));
                var publishFile = MoveToPublishDirectory(file);
                files.Add(publishFile);
            }
        }

        if (files.Count > 0)
        {
            SavePublishFileHashDictionary(_dict);
        }

        return files;
    }

    private Dictionary<string, string> LoadPublishFileHashDictionary()
    {
        var dict = new Dictionary<string, string>();
        var setting = Setting.GetInstance();
        if (File.Exists(setting.PublishFileHashDictionary))
        {
            var lines = File.ReadAllLines(setting.PublishFileHashDictionary);
            foreach (var line in lines)
            {
                var pair = line.Split(',');
                dict[pair[0]] = pair[1];
            }
        }
            
        return dict;
    }

    private void ResetPublishFolder(Setting setting)
    {
        if (Directory.Exists(setting.PublishDirectory))
        {
            Directory.Delete(setting.PublishDirectory, true);
        }

        Directory.CreateDirectory(setting.PublishDirectory);
    }

    private string GetHash(string file)
    {
        var md5 = MD5.Create();
        using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read))
        {
            var hash = new StringBuilder();
            foreach (var data in md5.ComputeHash(stream))
            {
                hash.Append(data.ToString("X2"));
            }
                
            return hash.ToString();
        }
            
    }

    private string MoveToPublishDirectory(string file)
    {
        var setting = Setting.GetInstance();
        var publishFile = Path.Combine(setting.PublishDirectory, file.Substring(setting.LocalBlogDirectory.Length + 1));
        var publishFolder = Path.GetDirectoryName(publishFile);
        if (publishFolder != null && !Directory.Exists(publishFolder))
        {
            Directory.CreateDirectory(publishFolder);
        }
            
        File.Copy(file, publishFile, true);
        return publishFile;
    }

    private void SavePublishFileHashDictionary(Dictionary<string, string> dict)
    {
        var setting = Setting.GetInstance();
        File.WriteAllLines(setting.PublishFileHashDictionary,
            dict.Select(kv => $"{kv.Key},{kv.Value}"),
            Encoding.UTF8);
    }
}