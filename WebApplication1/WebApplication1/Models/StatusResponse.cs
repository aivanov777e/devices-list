using System;

namespace WebApplication1.Models
{
    public enum StatusResponse : UInt16
    {
        Successes = 0x0000,
        WrongData = 0x000F,
        UnknownError = 0x00FF,
        UnknownStatus
    }
}
