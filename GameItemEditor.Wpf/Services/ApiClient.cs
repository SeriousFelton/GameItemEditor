#nullable enable
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using GameItemEditor.Core.Enums;
using GameItemEditor.Core.Models;
using GameItemEditor.Wpf.Models.Dto;
using Microsoft.Extensions.Logging;

namespace GameItemEditor.Wpf.Services
{
    public class ApiClient : IApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiClient> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiClient(HttpClient httpClient, ILogger<ApiClient> logger, JsonSerializerOptions jsonOptions)
        {
            _httpClient = httpClient;
            _logger = logger;
            _jsonOptions = jsonOptions;
        }

        public async Task<List<GameItem>> GetItemsAsync(
            string? search = null,
            ItemType? type = null,
            ItemRarity? rarity = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = new List<string>();
                if (!string.IsNullOrWhiteSpace(search))
                    query.Add($"search={Uri.EscapeDataString(search)}");
                if (type.HasValue)
                    query.Add($"type={type.Value}");
                if (rarity.HasValue)
                    query.Add($"rarity={rarity.Value}");

                var url = "api/items";
                if (query.Count > 0)
                    url += "?" + string.Join("&", query);

                var response = await _httpClient.GetAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<GameItem>>(_jsonOptions, cancellationToken) ?? new List<GameItem>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Ошибка при запросе к API");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка предметов");
                throw;
            }
        }

        public async Task<GameItem> GetItemAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/items/{id}", cancellationToken);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<GameItem>(_jsonOptions, cancellationToken) 
                    ?? throw new InvalidOperationException("Предмет не найден");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при получении предмета {id}");
                throw;
            }
        }

        public async Task<GameItem> CreateItemAsync(GameItem item, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/items", item, _jsonOptions, cancellationToken);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<GameItem>(_jsonOptions, cancellationToken)
                    ?? throw new InvalidOperationException("Не удалось создать предмет");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при создании предмета");
                throw;
            }
        }

        public async Task UpdateItemAsync(Guid id, GameItem item, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/items/{id}", item, _jsonOptions, cancellationToken);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка обновления предмета {id}");
                throw;
            }
        }

        public async Task PatchItemAsync(Guid id, object patchDto, CancellationToken cancellationToken = default)
        {
            try
            {
                var json = JsonSerializer.Serialize(patchDto, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PatchAsync($"api/items/{id}", content, cancellationToken);

                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при частичном обновлении предмета {id}");
                throw;
            }
        }

        public async Task DeleteItemAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/items/{id}", cancellationToken);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при удалении предмета {id}");
                throw;
            }
        }

    }
}
