using System.Runtime.InteropServices;

namespace AutoUpdate.modules
{
    class WinAPI
    {
        /// <summary>
        /// MoveFileFlags
        /// </summary>
        public enum MoveFileFlags
        {
            MOVEFILE_0x01_REPLACE_EXISTING = 0x01,
            MOVEFILE_0x02_COPY_ALLOWED = 0x02,
            MOVEFILE_0x04_DELAY_UNTIL_REBOOT = 0x04,
            MOVEFILE_0x08_WRITE_THROUGH = 0x08,
            MOVEFILE_0x10_CREATE_HARDLINK = 0x10,
            MOVEFILE_0x20_FAIL_IF_NOT_TRACKABLE = 0x20,
        }
        /// <summary>
        /// MoveFileEx
        /// </summary>
        /// <param name="lpExistingFileName"></param>
        /// <param name="lpNewFileName"></param>
        /// <param name="dwFlags"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        public static extern bool MoveFileEx(string lpExistingFileName, string lpNewFileName, MoveFileFlags dwFlags);
    }
}
