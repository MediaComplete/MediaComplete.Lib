using MediaComplete.Lib;
using NUnit.Framework;

namespace MediaComplete.Test
{
    [TestFixture]
    public class StatusBarTest
    {
        private string _message;
        private StatusIcon _icon = StatusIcon.None;
        private int _count;

        [Test, Timeout(30000)]
        public void RaiseStatusBarEvent_StatusMessage_ValidStatusMessage()
        {
            const string testMessage = "Test is successful";
            const StatusIcon testIcon = StatusIcon.Success;
            const string clearMessage = null;
            const StatusIcon clearIcon = StatusIcon.None;
            StatusBarHandler.Instance.Interval = .1;

            StatusBarHandler.Instance.RaiseStatusBarEvent += HandleStatusBarChangeEvent;
            StatusBarHandler.Instance.ChangeStatusBarMessage(testMessage, testIcon);

            Assert.AreEqual(testMessage, _message);
            Assert.AreEqual(testIcon, _icon);

            while (_count < 2)
            {
            }

            Assert.AreEqual(clearMessage, _message);
            Assert.AreEqual(clearIcon, _icon);
        }

        private void HandleStatusBarChangeEvent(string format, string messageKey, StatusIcon icon, params object[] extraArgs)
        {
            _message = messageKey;
            _icon = icon;
            _count++;
        }
    }
}
