using Client;

namespace Server
{
    public static class ServerSend
    {
        public static void Welcome(int id)
        {
            NetworkManager.Instance.Server.SendPacketTo(ServerPacketId.Welcome, id, writer => {
                writer.Put(id);
            });
        }
    }
}
