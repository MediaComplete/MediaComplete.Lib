﻿using System;

namespace MediaComplete.Lib.Library
{
    /// <summary>
    /// Represents a song in the error state. The original song (which maybe null) is available via the Source property.
    /// </summary>
    public class ErrorSong : AbstractSong
    {
        /// <summary>
        /// Contains the original song that has a problem. It may be null, if the problem was that the song didn't exist.
        /// </summary>
        public AbstractSong Source { get; private set; }

        /// <summary>
        /// Create an error-state wrapper around a song. 
        /// </summary>
        /// <param name="source">The original song that had the problem. This may be null.</param>
        public ErrorSong(AbstractSong source)
        {
            Source = source;

            _id = Source != null ? Source.Id : Guid.NewGuid().ToString();
        }

        #region AbstractSong overrides

        /// <summary>
        /// Unique key value used to look up the song in the FileManager
        /// </summary>
        public override string Id { get { return _id; } }
        private readonly string _id;

        /// <summary>
        /// The path describing where we tried to get this song from
        /// </summary>
        public override string Path { get { return Source.Path; } }

        /// <summary>
        /// Compares songs by Id
        /// </summary>
        /// <param name="other">The <see cref="Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object other)
        {
            return other is ErrorSong && ((ErrorSong)other).Id.Equals(_id);
        }

        #endregion
    }
}
