
namespace SlurkExp.Services.SlurkSetup
{
    public interface ISlurkSetup
    {
        SlurkSetupRequest DefaultRequest();
        Task<SlurkSetupResponse> RoomSetup(SlurkSetupRequest request);
        Task<SlurkSetupResponse> RoomSetup(int userCount, int minUserCount, List<int> botIds, List<string> botNames);
        Task Assignment(int groupId, int clientId, int treatmentId, SlurkSetupResponse response, List<SlurkLink> slurkLinks, SlurkSetupRequest request);
        SlurkSetupOptions GetSetupOptions();
        SlurkSetupRequest GetSetupRequest();
    }
}
