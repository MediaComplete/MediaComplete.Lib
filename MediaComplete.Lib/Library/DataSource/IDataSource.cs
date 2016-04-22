using System;
using System.Collections.Generic;

namespace MediaComplete.Lib.Library.DataSource
{
    /// <summary>
    /// Interface for any datasource
    /// </summary>
    public interface IDataSource
    {
        /// <summary>
        /// writes a song object to the data source
        /// </summary>
        /// <param name="file"></param>
        void SaveSong(AbstractSong file);
        /// <summary>
        /// Delete a song from the Data source
        /// </summary>
        /// <param name="deletedSong"></param>
        void DeleteSong(AbstractSong deletedSong);
        /// <summary>
        /// Returns a local song object based on a song's path
        /// </summary>
        /// <param name="path">The path to the resource</param>
        /// <returns>The data</returns>
        AbstractSong GetSong(string path);
        /// <summary>
        /// Return all song files
        /// </summary>
        /// <returns></returns>
        IEnumerable<AbstractSong> GetAllSongs();

        /// <summary>
        /// Occurs when a song is moved
        /// </summary>
        event SongUpdatedHandler SongMoved;
        /// <summary>
        /// Occurs when a song is changed
        /// </summary>
        event SongUpdatedHandler SongChanged;
        /// <summary>
        /// Occurs when a song is added
        /// </summary>
        event SongUpdatedHandler SongAdded;
        /// <summary>
        /// Occurs whenever a song is deleted
        /// </summary>
        event SongUpdatedHandler SongDeleted;
    }

    /// <summary>
    /// Delegate definition for handling changes to song files
    /// </summary>
    /// <param name="songs">The updated songs.</param>
    public delegate void SongUpdatedHandler(IEnumerable<AbstractSong> songs);
}
