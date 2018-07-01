﻿namespace Bindings
{
    public enum ServerPackets
    {
        SMessage = 1,
        SFullData = 2,
        SAckRegister = 3,
        SPlayerData = 4,
		SPulse = 5,
        SGalaxy = 6
    }

    public enum ClientPackets
    {
        CLogin = 1,
        CRegister = 2,
        CPlayerData = 3,
        CChat = 4
    }

    public enum MessageColors
    {
        Chat = 1,
        Warning = 2,
        Notification = 3
    }
}
