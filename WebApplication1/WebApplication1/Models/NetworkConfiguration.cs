using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class NetworkConfiguration
    {
        /// <summary>
        /// порт приема сообщений
        /// </summary>
        public int LocalPort { get; set; }
        /// <summary>
        /// порт для отправки сообщений
        /// </summary>
        public int RemotePort { get; set; }
    }
}
