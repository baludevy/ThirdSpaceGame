namespace Client
{
    public static class ClientSend
    {
        public static void Username(string username)
        {
            NetworkManager.Instance.Client.SendPacket(
                ClientPacketId.Username,
                writer => { writer.Put(username); }
            );
        }
    }
}
