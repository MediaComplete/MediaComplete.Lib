﻿using System;
using Moq;
using MediaComplete.Lib.Playing;
using NAudio.Wave;
using Ploeh.AutoFixture;
using TagLib;
using MediaComplete.Lib.Library;
using MediaComplete.Lib.Library.DataSource;
using NUnit.Framework;

namespace MediaComplete.Test.Playing
{
    [TestFixture]
    public class PlayerTest
    {
        [SetUp]
        public void Setup()
        {
            NowPlaying.Inst.Clear();
        }

        [Test]
        public void StateChanges_HappyPath()
        {
            var mockNAudioWrapper = new Mock<INAudioWrapper>();
            var mockFileManager = new Mock<ILibrary>();
            var stuff = new Fixture().Create<string>();
            var mockLocalSong = new LocalSong("id", new SongPath(stuff));

            mockNAudioWrapper.Setup(m => m.Setup(mockLocalSong, It.IsAny<EventHandler<StoppedEventArgs>>(), 1.0));

            var player = new Player(mockNAudioWrapper.Object, mockFileManager.Object);
            mockNAudioWrapper.Setup(m => m.Play()).Returns(PlaybackState.Playing);

            NowPlaying.Inst.Add(mockLocalSong);
            player.Play();
            Assert.AreEqual(PlaybackState.Playing, player.PlaybackState);

            mockNAudioWrapper.Setup(m => m.Pause()).Returns(PlaybackState.Paused);

            player.Pause();
            Assert.AreEqual(PlaybackState.Paused, player.PlaybackState);

            mockNAudioWrapper.Setup(m => m.Play()).Returns(PlaybackState.Playing);

            player.Resume();
            Assert.AreEqual(PlaybackState.Playing, player.PlaybackState);

            mockNAudioWrapper.Setup(m => m.Stop()).Returns(PlaybackState.Stopped);

            player.Stop();
            Assert.AreEqual(PlaybackState.Stopped, player.PlaybackState);
        }

        [Test]
        public void StateChanges_WeirdPath()
        {
            var mockNAudioWrapper = new Mock<INAudioWrapper>();
            var mockFileManager = new Mock<ILibrary>();
            var stuff = new Fixture().Create<string>();
            var mockLocalSong = new LocalSong("id", new SongPath(stuff));

            mockNAudioWrapper.Setup(m => m.Setup(mockLocalSong, It.IsAny<EventHandler<StoppedEventArgs>>(), 1.0));

            var player = new Player(mockNAudioWrapper.Object, mockFileManager.Object);
            mockNAudioWrapper.Setup(m => m.Play()).Returns(PlaybackState.Playing);

            NowPlaying.Inst.Add(mockLocalSong);
            player.Play();
            Assert.AreEqual(PlaybackState.Playing, player.PlaybackState);

            mockNAudioWrapper.Setup(m => m.Stop()).Returns(PlaybackState.Stopped);

            player.Stop();
            Assert.AreEqual(PlaybackState.Stopped, player.PlaybackState);

            mockNAudioWrapper.Setup(m => m.Stop()).Returns(PlaybackState.Stopped);

            player.Stop();
            Assert.AreEqual(PlaybackState.Stopped, player.PlaybackState);

            mockNAudioWrapper.Setup(m => m.Pause()).Returns(PlaybackState.Stopped);

            player.Pause();
            Assert.AreEqual(PlaybackState.Stopped, player.PlaybackState);

            mockNAudioWrapper.Setup(m => m.Play()).Returns(PlaybackState.Stopped);

            player.Resume();
            Assert.AreEqual(PlaybackState.Stopped, player.PlaybackState);

            mockNAudioWrapper.Setup(m => m.Play()).Returns(PlaybackState.Playing);

            player.Play();
            Assert.AreEqual(PlaybackState.Playing, player.PlaybackState);

            mockNAudioWrapper.Setup(m => m.Pause()).Returns(PlaybackState.Paused);

            player.Pause();
            Assert.AreEqual(PlaybackState.Paused, player.PlaybackState);

            mockNAudioWrapper.Setup(m => m.Pause()).Returns(PlaybackState.Paused);

            player.Pause();
            Assert.AreEqual(PlaybackState.Paused, player.PlaybackState);
        }

        [Test]
        [ExpectedException(typeof(CorruptFileException))]
        public void Play_InvalidFileInfo_ThrowsCorruptFileException()
        {
            var mockNAudioWrapper = new Mock<INAudioWrapper>();
            var mockFileManager = new Mock<ILibrary>();
            var stuff = new Fixture().Create<string>();
            var mockLocalSong = new LocalSong("id", new SongPath(stuff));

            mockNAudioWrapper.Setup(m => m.Setup(mockLocalSong, It.IsAny<EventHandler<StoppedEventArgs>>(), 1.0)).Throws(new CorruptFileException());

            var player = new Player(mockNAudioWrapper.Object, mockFileManager.Object);
            NowPlaying.Inst.Add(mockLocalSong);
            player.Play();

            Assert.Fail("Play should not have handled the CorruptFileException");
        }
    }
}
