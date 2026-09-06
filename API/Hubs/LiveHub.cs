using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace ABrechozeiraApp.Hubs
{
    public class LiveHub : Hub
    {
        /// <summary>
        /// O cliente (operador ou visualizador) entra na sala de uma live específica
        /// </summary>
        public async Task EntrarNaLive(string liveId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Live_{liveId}");
        }

        /// <summary>
        /// O cliente sai da sala de uma live específica
        /// </summary>
        public async Task SairDaLive(string liveId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Live_{liveId}");
        }
    }
}
