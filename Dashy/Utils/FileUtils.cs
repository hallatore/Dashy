using System.IO;

namespace Dashy.Utils
{
    public static class FileUtils
    {
        public static string ResolvePath(string path, string profilePath = null)
        {
            if (profilePath != null)
            {
                var combinedPath = Path.Combine(Path.GetDirectoryName(profilePath), path);
                var combinedPathResult = _ResolvePath(combinedPath);

                if (combinedPathResult != null)
                {
                    return combinedPathResult;
                }
            }

            return _ResolvePath(path);
        }

        private static string _ResolvePath(string path)
        {
            if (File.Exists(path))
            {
                return path;
            }

            var configPath = Path.Combine("configs", path);

            if (File.Exists(configPath))
            {
                return configPath;
            }

            return null;
        }
    }
}
