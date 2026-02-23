namespace DeskQuotes.Misc.Win32Helpers;

public static class User32Dll
{
    private const string DllName = "user32.dll";

    #region methods

    [DllImport(DllName, SetLastError = true)]
    public static extern bool GetLastInputInfo(ref LASTINPUTINFO pLastInputInfo);

    #endregion

    #region structs

    [StructLayout(LayoutKind.Sequential)]
    // ReSharper disable once InconsistentNaming
    public struct LASTINPUTINFO
    {
        public uint cbSize;

        public uint dwTime;
    }

    #endregion

    #region enums

    // @TODO

    #endregion
}