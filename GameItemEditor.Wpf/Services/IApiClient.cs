using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GameItemEditor.Core.Enums;
using GameItemEditor.Core.Models;

namespace GameItemEditor.Wpf.Services
{
    public interface IApiClient
    {
        Task<List<GameItem>> GetItemsAsync(
            string? search = null,
            ItemType? type = null,
            ItemRarity? rarity = null,
            CancellationToken cancellationToken = default);

        Task<GameItem> GetItemAsync(Guid id, CancellationToken cancellationToken = default);
        Task<GameItem> CreateItemAsync(GameItem item, CancellationToken cancellationToken = default);
        Task UpdateItemAsync(Guid id, GameItem item, CancellationToken cancellationToken = default);
        Task PatchItemAsync(Guid id, object patchDto, CancellationToken cancellationToken = default);
        Task DeleteItemAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
