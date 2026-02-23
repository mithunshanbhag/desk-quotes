namespace DeskQuotes.Misc.Win32Helpers;

public static class Wtsapi32Dll
{
    private const string DllName = "wtsapi32.dll";

    #region enums

    // ReSharper disable once InconsistentNaming
    public enum WTS_CONNECTSTATE_CLASS
    {
        WTSActive,
        WTSConnected,
        WTSConnectQuery,
        WTSShadow,
        WTSDisconnected,
        WTSIdle,
        WTSListen,
        WTSReset,
        WTSDown,
        WTSInit
    }

    // ReSharper disable once InconsistentNaming
    public enum WTS_INFO_CLASS
    {
        WTSInitialProgram,
        WTSApplicationName,
        WTSWorkingDirectory,
        WTSOEMId,
        WTSSessionId,
        WTSUserName,
        WTSWinStationName,
        WTSDomainName,
        WTSConnectState,
        WTSClientBuildNumber,
        WTSClientName,
        WTSClientDirectory,
        WTSClientProductId,
        WTSClientHardwareId,
        WTSClientAddress,
        WTSClientDisplay,
        WTSClientProtocolType,
        WTSIdleTime,
        WTSLogonTime,
        WTSIncomingBytes,
        WTSOutgoingBytes,
        WTSIncomingFrames,
        WTSOutgoingFrames,
        WTSClientInfo,
        WTSSessionInfo,
        WTSSessionInfoEx,
        WTSConfigInfo,
        WTSValidationInfo, // Info Class value used to fetch Validation Information through the WTSQuerySessionInformation
        WTSSessionAddressV4,
        WTSIsRemoteSession
    }

    // ReSharper disable once InconsistentNaming
    public enum WTS_TYPE_CLASS
    {
        WTSTypeProcessInfoLevel0,
        WTSTypeProcessInfoLevel1,
        WTSTypeSessionInfoLevel1
    }

    #endregion

    #region structs

    [StructLayout(LayoutKind.Sequential)]
    public struct WTSINFO
        // ReSharper disable once InconsistentNaming
    {
        public readonly WTS_CONNECTSTATE_CLASS State;

        public readonly uint SessionId;

        public readonly uint IncomingBytes;

        public readonly uint OutgoingBytes;

        public readonly uint IncomingFrames;

        public readonly uint OutgoingFrames;

        public readonly uint IncomingCompressedBytes;

        public readonly uint OutgoingCompressedBytes;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly byte[] WinStationName;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
        public readonly byte[] Domain;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 21)]
        public readonly byte[] UserName;

        public readonly long ConnectTime;

        public readonly long DisconnectTime;

        public readonly long LastInputTime;

        public readonly long LogonTime;

        public readonly long CurrentTime;
    }


    [StructLayout(LayoutKind.Sequential)]
    // ReSharper disable once InconsistentNaming
    public struct WTS_SESSION_INFO_1
    {
        public readonly uint ExecEnvId;

        public readonly WTS_CONNECTSTATE_CLASS State;

        public readonly uint SessionID;

        public readonly string pSessionName;

        public readonly string pHostName;

        public readonly string pUserName;

        public readonly string pDomainName;

        public readonly string pFarmName;
    }

    #endregion

    #region methods

    [DllImport(DllName, SetLastError = true)]
    public static extern bool WTSEnumerateSessionsEx(IntPtr hServer, ref uint pLevel, uint Filter, ref IntPtr ppSessionInfo, ref uint pCount);

    [DllImport(DllName, SetLastError = true)]
    public static extern bool WTSFreeMemory(IntPtr pMemory);

    [DllImport(DllName, SetLastError = true)]
    public static extern bool WTSFreeMemoryEx(WTS_TYPE_CLASS WTSTypeClass, IntPtr pMemory, uint NumberOfEntries);

    [DllImport(DllName, SetLastError = true)]
    public static extern bool WTSQuerySessionInformation(IntPtr hServer, uint SessionId, WTS_INFO_CLASS WTSInfoClass, ref IntPtr ppBuffer, ref uint pBytesReturned);

    #endregion
}