using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace U2.SharpTracker.Core;

public abstract class FileCache
{
    private static string IdToPath(string folder, int id, int start = -1)
    {
        var currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var id1 = id / 1000000;
        var id2 = (id % 1000000) / 1000;
        var id3 = (id % 1000);
        var id4 = $"{Path.DirectorySeparatorChar}{start.ToString().PadLeft(5, '0')}";
        if (start < 0)
        {
            id4 = string.Empty;
        }
        var path = Path.Combine(currentPath,"Cache", folder, 
            id1.ToString().PadLeft(3, '0'), 
            id2.ToString().PadLeft(3, '0'), 
            $"{id3.ToString().PadLeft(3, '0')}{id4}.html");
        return path;
    }

    public static string TryGetCache(string url)
    {
        if (url.Contains("viewtopic"))
        {
            var id = RutrackerParser.GetIdFromUrl(url);
            return TryGetTopicCache(id);
        }
        else if (url.Contains("viewforum"))
        {
            if (RegularExpressionHelper.Match("f=(\\d+)&start=(\\d+)", url, out var m))
            {
                var id = int.Parse(m[1]);
                var start = int.Parse(m[2]);
                return TryGetBranchCache(id, start);
            }
            else if (RegularExpressionHelper.Match("f=(\\d+)", url, out var m2))
            {
                var id = int.Parse(m[1]);
                return TryGetBranchCache(id, 0);
            }
            return null;// TryGetBranchCache(id);
        }

        return null;
    }

    public static string TryGetCache(int id, int start, string folder)
    {
        var path = IdToPath(folder, id, start);
        if (!File.Exists(path))
        {
            return null;
        }

        return File.ReadAllText(path);
    }

    public static string TryGetTopicCache(int id)
    {
        return TryGetCache(id, 0, "topics");
    }

    public static string TryGetBranchCache(int id, int start)
    {
        return TryGetCache(id, start, "branches");
    }

    public static void PutCache(string url, string content)
    {
        if (url.Contains("viewtopic"))
        {
            var id = RutrackerParser.GetIdFromUrl(url);
            PutTopicCache(id, content);
        }
        else if (url.Contains("viewforum"))
        {
            if (RegularExpressionHelper.Match("f=(\\d+)&start=(\\d+)", url, out var m))
            {
                var id = int.Parse(m[1]);
                var start = int.Parse(m[2]);
                PutBranchCache(id, start, content);
            }
            else if (RegularExpressionHelper.Match("f=(\\d+)", url, out var m2))
            {
                var id = int.Parse(m[1]);
                PutBranchCache(id, 0, content);
            }
        }
    }

    public static void PutCache(int id, int start, string folder, string content)
    {
        var path = IdToPath(folder, id, start);
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, content);
    }

    public static void PutTopicCache(int id, string content)
    {
        PutCache(id, 0, "topics", content);
    }

    public static void PutBranchCache(int id, int start, string content)
    {
        PutCache(id, start, "branches", content);
    }
}
