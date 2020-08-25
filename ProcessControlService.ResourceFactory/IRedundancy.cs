namespace ProcessControlService.ResourceFactory
{
    
    public interface IRedundancy
    {
        
        #region Rendundancy

        bool NeedDataSync
        {
            get;
        }

        RedundancyMode CurrentRedundancyMode
        {
            get;
        }

        void RedundancyModeChange(RedundancyMode mode);

        string BuildSyncData();

        void ExtractSyncData(string data);

        #endregion
        
    }
}
