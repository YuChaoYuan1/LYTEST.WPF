namespace RMT
{
    interface IConnection
    {
        string ConnectName { get; set; }
        
        int MaxWaitSeconds { set; get; }

        int WaitSecondsPerByte { set; get; }

        bool Open();

        bool Close();

        bool UpdateSetting(string szSetting);

        bool SendData(ref byte[] vData, bool IsReturn, int WaiteTime);
    }
}
