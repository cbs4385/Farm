#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.IO;
using System.Text;

namespace Farm.Platform
{
    // QA stand-in for Steam Cloud: a folder is the "cloud" (point two game folders at one shared folder to try two computers).
    // Development builds and the Editor only: `-farmFakeCloud <folder>` installs it (GameServices); release builds do not contain it.
    public sealed class FolderCloudStorage : ICloudStorage
    {
        readonly string _folder;

        public FolderCloudStorage(string folder) { _folder = folder; }

        public bool IsAvailable => true;

        string PathOf(string name) => Path.Combine(_folder, name.Replace('/', Path.DirectorySeparatorChar));

        public bool Exists(string name) => File.Exists(PathOf(name));

        public DateTime ModifiedUtc(string name) => File.Exists(PathOf(name)) ? File.GetLastWriteTimeUtc(PathOf(name)) : DateTime.MinValue;

        public bool TryRead(string name, out string text)
        {
            text = null;
            try
            {
                if (!File.Exists(PathOf(name))) return false;
                text = File.ReadAllText(PathOf(name), Encoding.UTF8);
                return true;
            }
            catch (Exception) { return false; }
        }

        public bool TryWrite(string name, string text)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(PathOf(name)) ?? ".");
                File.WriteAllText(PathOf(name), text, new UTF8Encoding(false));
                return true;
            }
            catch (Exception) { return false; }
        }

        public bool TryDelete(string name)
        {
            try
            {
                if (File.Exists(PathOf(name))) File.Delete(PathOf(name));
                return true;
            }
            catch (Exception) { return false; }
        }
    }

    // A platform that is only a folder cloud (names itself so logs say what is in use).
    public sealed class FolderCloudPlatform : IPlatformServices
    {
        public FolderCloudPlatform(string folder) { Cloud = new FolderCloudStorage(folder); }
        public string Name => "folder-cloud";
        public bool IsOnline => true;
        public void UnlockAchievement(string id) { }
        public ICloudStorage Cloud { get; }
    }
}
#endif
