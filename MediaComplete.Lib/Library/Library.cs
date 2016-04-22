using System.Collections.Generic;
using MediaComplete.Lib.Library.DataSource;
using System.Linq;

namespace MediaComplete.Lib.Library
{
    /// <summary>
    /// The collection of abstractsongs, from every datasource
    /// </summary>
    public class Library : ILibrary
    {
        /// <summary>
        /// Dictionary of Id, song pairs. songs carry a reference to the ID identifying them
        /// </summary>
        private readonly Dictionary<string, LocalSong> _cachedSongFiles;

        /// <summary>
        /// singleton instance of the Filemanager
        /// </summary>
        private static Library _instance;
        /// <summary>
        /// The publicly acessible variable for the Library instance
        /// </summary>
        public static ILibrary Instance { get { return _instance ?? (_instance = new Library(FileSystem.Instance)); } }
        private readonly IFileSystem _fileSystem;

        private Library(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            _cachedSongFiles = new Dictionary<string, LocalSong>();
            Initialize(SettingWrapper.MusicDir);
        }

        /// <summary>
        /// Rebuilds the dictionaries using the parameter as the source. 
        /// </summary>
        /// <param name="musicDir">Source Directory for populating the dictionarires</param>
        public void Initialize(DirectoryPath musicDir)
        {
            _cachedSongFiles.Clear();
            _fileSystem.Initialize(musicDir);
        }

        #region Data Operations
        /// <summary>
        /// Writes the attributes of the song parameter to the TagLib File and updates the stored FileInfo and song
        /// </summary>
        /// <param name="song">file with updated metadata</param>
        public void SaveSong(AbstractSong song)
        {
            var file = song as LocalSong;
            if (file != null)
                _fileSystem.SaveSong(file);

            SongChanged.Invoke(song);
        }

        /// <summary>
        /// Changes the location of the song's audio data.
        /// </summary>
        /// <param name="song">Existing song</param>
        /// <param name="newLocation">The new location</param>
        public void MoveSong(AbstractSong song, SongPath newLocation)
        {
            if (song is LocalSong)
            {
                var localSong = (song as LocalSong);
                _fileSystem.MoveFile(localSong, newLocation);
                localSong.SongPath = newLocation;
            }
        }

        /// <summary>
        /// Removes a song from the caches, AND THE FILESYSTEM AS WELL. THAT FILE IS GONE NOW. DON'T JUST CALL THERE FOR THE HECK OF IT
        /// </summary>
        /// <param name="deletedSong">the song that needs to be deleted</param>
        public void DeleteSong(AbstractSong deletedSong)
        {
            var song = deletedSong as LocalSong;
            if(song != null)
                _fileSystem.DeleteSong(song);

            SongRemoved.Invoke(deletedSong);
        }

        /// <summary>
        /// Adds a song into the library. Also adds it into the appropriate data store, if necessary.
        /// </summary>
        /// <param name="newSong">The new song</param>
        public void AddSong(AbstractSong newSong)
        {
            if (newSong is LocalSong)
            {
                _cachedSongFiles.Add(newSong.Id, newSong as LocalSong);
            }

            SongAdded.Invoke(newSong);
        }

        /// <summary>
        /// Return all song files
        /// </summary>
        /// <returns></returns>
        public IEnumerable<AbstractSong> GetAllSongs()
        {
            return _cachedSongFiles.Values;
        }

        /// <summary>
        /// Get a song object for a matching source path
        /// </summary>
        /// <param name="path">string path to compare</param>
        /// <returns>LocalSong if it exists, null if it doesn't</returns>
        public AbstractSong GetSong(string path)
        {
            return _cachedSongFiles.FirstOrDefault(s => s.Value.SongPath.FullPath == path).Value;
        }
        #endregion

        #region Events

        /// <summary>
        /// Triggered when a song is removed from the library
        /// </summary>
        public event SongEventHandler SongRemoved = delegate { };

        /// <summary>
        /// Triggered when a song's properties is changed
        /// </summary>
        public event SongEventHandler SongChanged = delegate { };

        /// <summary>
        /// Triggered when a new song is added to the library
        /// </summary>
        public event SongEventHandler SongAdded = delegate { };

        #endregion
    }

    /// <summary>
    /// interface for the collection of abstractsongs, from every datasource
    /// </summary>
    public interface ILibrary
    {
        /// <summary>
        /// Rebuilds the dictionaries using the parameter as the source. 
        /// </summary>
        /// <param name="directory">Source Directory for populating the dictionarires</param>
        void Initialize(DirectoryPath directory);
        /// <summary>
        /// Writes the attributes of the song parameter to the TagLib File and updates the stored FileInfo and song
        /// </summary>
        /// <param name="song">file with updated metadata</param>
        void SaveSong(AbstractSong song);
        /// <summary>
        /// Changes the location of the song's audio data.
        /// </summary>
        /// <param name="song">Existing song</param>
        /// <param name="newLocation">The new location</param>
        void MoveSong(AbstractSong song, SongPath newLocation);
        /// <summary>
        /// Get every song object that exists in the cache
        /// </summary>
        /// <returns>IEnumerable containing every song within the cache</returns>
        IEnumerable<AbstractSong> GetAllSongs();
        /// <summary>
        /// Removes a song from the caches, AND THE FILESYSTEM AS WELL. THAT FILE IS GONE NOW. DON'T JUST CALL THERE FOR THE HECK OF IT
        /// </summary>
        /// <param name="deletedSong">the song that needs to be deleted</param>
        void DeleteSong(AbstractSong deletedSong);
        /// <summary>
        /// Adds a song into the library. Also adds it into the appropriate data store, depending on the type.
        /// </summary>
        /// <param name="newSong">The new song</param>
        void AddSong(AbstractSong newSong);
        /// <summary>
        /// Get a LocalSong object with a matching SongPath object
        /// </summary>
        /// <param name="path">string path to compare</param>
        /// <returns>LocalSong if it exists, null if it doesn't</returns>
        AbstractSong GetSong(string path);

        /// <summary>
        /// Triggered when a song is removed from the library
        /// </summary>
        event SongEventHandler SongRemoved;

        /// <summary>
        /// Triggered when a song's properties is changed
        /// </summary>
        event SongEventHandler SongChanged;

        /// <summary>
        /// Triggered when a new song is added to the library
        /// </summary>
        event SongEventHandler SongAdded;
    }

    /// <summary>
    /// The signature for an event handler that accepts information about song changes within the library.
    /// </summary>
    /// <param name="song">The affected song</param>
    public delegate void SongEventHandler(AbstractSong song);
}
