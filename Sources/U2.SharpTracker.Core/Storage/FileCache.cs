using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace U2.SharpTracker.Core;

public abstract class FileCache
{
    private static string IdToPath(string folder, int id)
    {
        var currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var id1 = id / 1000000;
        var id2 = (id % 1000000) / 1000;
        var id3 = (id % 1000);
        var path = Path.Combine(currentPath,"Cache", folder, 
            id1.ToString().PadLeft(3, '0'), 
            id2.ToString().PadLeft(3, '0'), 
            $"{id3.ToString().PadLeft(3, '0')}.html");
        return path;
    }

    public static string TryGetCache(string url)
    {
        var id = RutrackerParser.GetIdFromUrl(url);
        if (url.Contains("viewtopic"))
        {
            return TryGetTopicCache(id);
        }
        else
        {
            return TryGetBranchCache(id);
        }
    }

    public static string TryGetCache(int id, string folder)
    {
        var path = IdToPath(folder, id);
        if (!File.Exists(path))
        {
            return null;
        }

        return File.ReadAllText(path);
    }

    public static string TryGetTopicCache(int id)
    {
        return TryGetCache(id, "topics");
    }

    public static string TryGetBranchCache(int id)
    {
        return TryGetCache(id, "branches");
    }

    public static void PutCache(string url, string content)
    {
        var id = RutrackerParser.GetIdFromUrl(url);
        if (url.Contains("viewtopic"))
        {
            PutTopicCache(id, content);
        }
        else 
        {
            PutBranchCache(id, content);
        }
    }

    public static void PutCache(int id, string folder, string content)
    {
        var path = IdToPath(folder, id);
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, content);
    }

    public static void PutTopicCache(int id, string content)
    {
        PutCache(id, "topics", content);
    }

    public static void PutBranchCache(int id, string content)
    {
        PutCache(id, "branches", content);
    }
}
