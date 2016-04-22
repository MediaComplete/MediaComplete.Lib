using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MediaComplete.Lib.Metadata;
using TagLib;
using File = System.IO.File;
using TaglibFile = TagLib.File;

namespace MediaComplete.Lib.Library.DataSource
{
    /// <summary>
    /// The representation of the Data Source provided from the File System
    /// </summary>
    public class FileSystem : IFileSystem
    {
        /// <summary>
        /// Dictionary of id, <see cref="FileInfo"/> pairs.
        /// </summary>
        private readonly Dictionary<string, FileInfo> _cachedFiles = new Dictionary<string, FileInfo>();

        /// <summary>
        /// Returns the instance of the file system
        /// </summary>
        public static IFileSystem Instance { get { return _instance ?? (_instance = new FileSystem()); } }
        private static FileSystem _instance;

        private FileSystem()
        {
        }

        /// <summary>
        /// Initializes the locally stored data source based on a directory
        /// </summary>
        /// <param name="musicDir"></param>
        /// <returns></returns>
        public void Initialize(DirectoryPath musicDir)
        {
            _cachedFiles.Clear();

            if (!Directory.Exists(musicDir.FullPath))
            {
                CreateDirectory(musicDir);
            }

            var files = new DirectoryInfo(musicDir.FullPath).GetFiles("*", SearchOption.AllDirectories).GetMusicFiles();
            foreach (var fileInfo in files)
            {
                _cachedFiles.Add(fileInfo.FullName, fileInfo);
            }
        }

        /// <summary>
        /// Copies a file between two specified paths. 
        /// This is currently only used in the Importer
        /// </summary>
        /// <param name="filePath">Original location of the song, including filename</param>
        /// <param name="newFilePath">Destination location of the song, including filename</param>
        public void CopyFile(SongPath filePath, SongPath newFilePath)
        {
            File.Copy(filePath.FullPath, newFilePath.FullPath);
            var newFile = new FileInfo(newFilePath.FullPath);
            _cachedFiles.Add(newFile.FullName, newFile);
            SongAdded.Invoke(new[] { GetSongForFile(newFile) });
        }

        /// <summary>
        /// Create a folder at a specified location.
        /// Used by Sorter and to initialize music/playlist folders where necessary
        /// </summary>
        /// <param name="directory">Destination location to create the folder, including the folder name</param>
        public void CreateDirectory(DirectoryPath directory)
        {
            Directory.CreateDirectory(directory.FullPath);
        }

        /// <summary>
        /// Move a directory, and all its contents.
        /// </summary>
        /// <param name="source">The source directory</param>
        /// <param name="dest">The destination</param>
        public void MoveDirectory(DirectoryPath source, DirectoryPath dest)
        {
            Directory.Move(source.FullPath, dest.FullPath);
        }

        /// <summary>
        /// Verifies if the specified file exists.
        /// </summary>
        /// <param name="file">file location to check</param>
        /// <returns>true if the file exists</returns>
        /// <returns>false if the file does not exist</returns>
        public bool FileExists(SongPath file)
        {
            return File.Exists(file.FullPath);
        }

        /// <summary>
        /// Moves a file from a source song to a new location
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        public void MoveFile(LocalSong source, SongPath dest)
        {
            this.MoveFile(source.SongPath, dest);
        }

        /// <summary>
        /// Moves a file from a source path to a new location
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        public void MoveFile(SongPath source, SongPath dest)
        {
            bool alreadyInLibrary = false;
            if (_cachedFiles.Keys.Contains(source.FullPath))
            {
                alreadyInLibrary = true;
            }

            var targetDirectory = dest.Directory.FullPath;
            if (!Directory.Exists(targetDirectory))
                Directory.CreateDirectory(targetDirectory);
            if (FileExists(source))
                File.Move(source.FullPath, dest.FullPath);

            var song = GetSongForFile(new FileInfo(dest.FullPath));
            if (alreadyInLibrary)
            {
                SongMoved.Invoke(new[] { song });
            }
            else
            {
                SongAdded.Invoke(new[] { song });
            }
        }

        #region IDataSource methods

        /// <summary>
        /// writes a local song object to a file system object
        /// </summary>
        /// <param name="abstractSong"></param>
        public void SaveSong(AbstractSong abstractSong)
        {
            var song = abstractSong as LocalSong;
            if (song == null) return;

            var file = TagLib.File.Create(song.Path);

            foreach (var attribute in Enum.GetValues(typeof(MetaAttribute)).Cast<MetaAttribute>().ToList()
                .Where(x => file.GetAttribute(x) == null || !file.GetAttribute(x).Equals(song.GetAttribute(x))))
            {
                file.SetAttribute(attribute, song.GetAttribute(attribute));
            }
            try
            {
                file.Save(); //TODO: MC-4 add catch for save when editing a file while it is playing
            }
            catch (UnauthorizedAccessException)
            {
                StatusBarHandler.Instance.ChangeStatusBarMessage("Save-Error", StatusIcon.Error);
            }
        }

        /// <summary>
        /// Delete a file from the local File System
        /// </summary>
        /// <param name="abstractSong">The song to delete</param>
        public void DeleteSong(AbstractSong abstractSong)
        {
            var song = abstractSong as LocalSong;
            if (song == null) return;

            var sourceDir = song.SongPath.Directory;
            if (File.Exists(song.Path))
                File.Delete(song.Path);
            _cachedFiles.Remove(song.Path);
            ScrubEmptyDirectories(new DirectoryInfo(sourceDir.FullPath));
        }

        /// <summary>
        /// Returns a local song object based on a song's path
        /// </summary>
        /// <param name="path">The path of the new song</param>
        /// <returns>The song object</returns>
        public AbstractSong GetSong(string path)
        {
            var file = _cachedFiles.Values.FirstOrDefault(f => f.FullName == path);
            if (file == null)
            {
                // File not cached - lets try the OS
                file = new FileInfo(path);
                if (file == null)
                    return null;

                _cachedFiles.Add(path, file);
            }

            return GetSongForFile(file);
        }

        /// <summary>
        /// Returns all the songs available to this datasource
        /// </summary>
        /// <returns></returns>
        public IEnumerable<AbstractSong> GetAllSongs()
        {
            return _cachedFiles.Values.Select(GetSongForFile);
        }

        #endregion

        #region IDataSource Events

        /// <summary>
        /// Occurs when a song is renamed
        /// </summary>
        public event SongUpdatedHandler SongMoved = delegate { };
        /// <summary>
        /// Occurs when a song is modified
        /// </summary>
        public event SongUpdatedHandler SongChanged = delegate { };
        /// <summary>
        /// Occurs when a song is created
        /// </summary>
        public event SongUpdatedHandler SongAdded = delegate { };
        /// <summary>
        /// Occurs whenever a song is deleted
        /// </summary>
        public event SongUpdatedHandler SongDeleted = delegate { };

        #endregion

        #region Private Helpers

        /// <summary>
        /// Initializes a new LocalSong object, using the path and unique ID provided.
        /// A TagLib file is created, and the necessary fields are read in, in order to populate
        /// the new LocalSong object.
        /// </summary>
        /// <param name="file">location of the new song</param>
        /// <returns>newly initialized and populated LocalSong object</returns>
        private static AbstractSong GetSongForFile(FileInfo file)
        {
            try
            {
                var tagFile = TaglibFile.Create(file.FullName);
                var tag = tagFile.Tag;
                return new LocalSong(Guid.NewGuid().ToString(), new SongPath(file.FullName))
                {
                    Title = tag.Title,
                    Artists = tag.AlbumArtists,
                    Album = tag.Album,
                    Genres = tag.Genres,
                    Year = tag.Year,
                    TrackNumber = tag.Track,
                    SupportingArtists = tag.Performers,
                    Duration = (int?)tagFile.Properties.Duration.TotalSeconds
                };
            }
            catch (CorruptFileException)
            {
                StatusBarHandler.Instance.ChangeStatusBarMessage("CorruptFile-Error", StatusIcon.Error);
                return new ErrorSong(new LocalSong(Guid.NewGuid().ToString(), new SongPath(file.FullName)));
            }
        }

        /// <summary>
        /// Recursively clean up directories that we may have emptied.
        /// </summary>
        /// <param name="directory">The directory to purge from.</param>
        private static void ScrubEmptyDirectories(DirectoryInfo directory)
        {
            if (!directory.Exists) return;
            foreach (var child in directory.EnumerateDirectories("*", SearchOption.AllDirectories))
            {
                ScrubEmptyDirectories(child);
                if (!child.EnumerateFileSystemInfos().Any())
                {
                    child.Delete();
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// Interface for the data source represented by the local file system
    /// </summary>
    public interface IFileSystem : IDataSource
    {
        /// <summary>
        /// Copies a file between two specified paths. 
        /// This is currently only used in the Importer
        /// </summary>
        /// <param name="file">Original location of the song, including filename</param>
        /// <param name="newFile">Destination location of the song, including filename</param>
        void CopyFile(SongPath file, SongPath newFile);
        /// <summary>
        /// Create a folder at a specified location.
        /// Used by Sorter and to initialize music/playlist folders where necessary
        /// </summary>
        /// <param name="directory">Destination location to create the folder, including folder name</param>
        void CreateDirectory(DirectoryPath directory);
        /// <summary>
        /// Move a directory, and all its contents.
        /// </summary>
        /// <param name="source">The source directory</param>
        /// <param name="dest">The destination</param>
        void MoveDirectory(DirectoryPath source, DirectoryPath dest);
        /// <summary>
        /// Verifies if the specified file exists.
        /// </summary>
        /// <param name="file">file location to check</param>
        /// <returns>true if the file exists</returns>
        /// <returns>false if the file does not exist</returns>
        bool FileExists(SongPath file);
        /// <summary>
        /// Moves a file from a source song to a new location
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        void MoveFile(LocalSong source, SongPath dest);
        /// <summary>
        /// Moves a file from a source path to a new location
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        void MoveFile(SongPath source, SongPath dest);

        /// <summary>
        /// Initializes the locally stored data source based on a directory
        /// </summary>
        /// <param name="musicDir"></param>
        void Initialize(DirectoryPath musicDir);
    }
}
