namespace SlurkExp.Services.Settings
{
    public interface ISettingsService
    {
        bool IsServerClosed { get; set; }
        bool IsRandomDispatch { get; set; }

        int TreatmentOverride { get; set; }
        int GroupOverride { get; set; }
        int ClientOverride { get; set; }

        int WaitingRoomLayoutId { get; set; }
        int ChatRoomLayoutId { get; set; }
    }
}
