using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameItemEditor.Core.Enums;
using GameItemEditor.Core.Models;
using GameItemEditor.Wpf.Services;
using Microsoft.Extensions.Logging;

namespace GameItemEditor.Wpf.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IApiClient _apiClient;
        private readonly ILogger<MainViewModel> _logger;
        private CancellationTokenSource _cancellationTokenSource;

        // Коллекция предметов для DataGrid
        [ObservableProperty]
        private ObservableCollection<GameItem> _items = new();

        // Состояние загрузки
        [ObservableProperty]
        private bool _isBusy;

        // Текст поиска
        [ObservableProperty]
        private string _searchText = string.Empty;

        // Выбранный тип
        [ObservableProperty]
        private FilterItem<ItemType>? _selectedType;

        // Выбранная редкость
        [ObservableProperty]
        private FilterItem<ItemRarity>? _selectedRarity;

        // Выбранный предмет в DataGrid
        [ObservableProperty]
        private GameItem? _selectedItem;

        // Список типов для ComboBox
        public List<FilterItem<ItemType>> ItemTypesWithAll { get; } = new()
        {
            new FilterItem<ItemType> { Name = "Все", Value = null },
            new FilterItem<ItemType> { Name = "Weapon", Value = ItemType.Weapon },
            new FilterItem<ItemType> { Name = "Armor", Value = ItemType.Armor },
            new FilterItem<ItemType> { Name = "Potion", Value = ItemType.Potion },
            new FilterItem<ItemType> { Name = "QuestItem", Value = ItemType.QuestItem },
            new FilterItem<ItemType> { Name = "Accessory", Value = ItemType.Accessory }
        };

        // Список редкостей для ComboBox
        public List<FilterItem<ItemRarity>> ItemRaritiesWithAll { get; } = new()
        {
            new FilterItem<ItemRarity> { Name = "Все", Value = null },
            new FilterItem<ItemRarity> { Name = "Common", Value = ItemRarity.Common },
            new FilterItem<ItemRarity> { Name = "Uncommon", Value = ItemRarity.Uncommon },
            new FilterItem<ItemRarity> { Name = "Rare", Value = ItemRarity.Rare },
            new FilterItem<ItemRarity> { Name = "Epic", Value = ItemRarity.Epic },
            new FilterItem<ItemRarity> { Name = "Legendary", Value = ItemRarity.Legendary }
        };

        // Конструктор
        public MainViewModel(IApiClient apiClient, ILogger<MainViewModel> logger)
        {
            _apiClient = apiClient;
            _logger = logger;

            // Инициализация команд
            LoadItemsCommand = new AsyncRelayCommand(LoadItemsAsync, CanLoadItem);
            DeleteItemCommand = new AsyncRelayCommand<GameItem>(DeleteItemAsync, CanDeleteItem);
            OpenEditorCommand = new RelayCommand(OpenEditor, CanOpenEditor);
            SearchCommand = new AsyncRelayCommand(SearchAsync, CanSearch);
            CancelCommand = new RelayCommand(Cancel, CanCancel);

            // Устанавливаем значения по умолчанию для фильтров (Все)
            _selectedType = ItemTypesWithAll[0];
            _selectedRarity = ItemRaritiesWithAll[0];
        }

        // Команды
        public IAsyncRelayCommand LoadItemsCommand { get; }
        public IAsyncRelayCommand<GameItem> DeleteItemCommand { get; }
        public IRelayCommand OpenEditorCommand { get; }
        public IAsyncRelayCommand SearchCommand { get; }
        public IRelayCommand CancelCommand { get; }

        // Загрузка списка предметов
        private async Task LoadItemsAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource = new CancellationTokenSource();

                var items = await _apiClient.GetItemsAsync(
                    search: string.IsNullOrWhiteSpace(SearchText) ? null : SearchText,
                    type: SelectedType?.Value as ItemType?,
                    rarity: SelectedRarity?.Value as ItemRarity?,
                    cancellationToken: _cancellationTokenSource.Token
                );

                Items.Clear();
                foreach (var item in items)
                {
                    Items.Add(item);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Загрузка предметов была отменена");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке предметов");
                MessageBox.Show($"Не удалось загрузить предметы: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanLoadItem() => !IsBusy;

        // Удаление предмета
        private async Task DeleteItemAsync(GameItem? item)
        {
            if (item == null || IsBusy) return;

            var result = MessageBox.Show(
                $"Удалить предмет '{item.Name}'?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                IsBusy = true;
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource = new CancellationTokenSource();

                await _apiClient.DeleteItemAsync(item.Id, _cancellationTokenSource.Token);
                Items.Remove(item);

                if (SelectedItem == item)
                    SelectedItem = null;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Удаление предмета было отменено");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при удалении предмета {item.Id}");
                MessageBox.Show($"Не удалось удалить предмет: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanDeleteItem(GameItem? item) => !IsBusy && item != null;

        // Открытие редактора (заглушка, будет заменено в Шаге 8)
        private void OpenEditor()
        {
            MessageBox.Show("Открытие редактора предметов...", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private bool CanOpenEditor() => !IsBusy;

        // Поиск
        private async Task SearchAsync()
        {
            await LoadItemsAsync();
        }

        private bool CanSearch() => !IsBusy;

        // Отмена текущей операции
        private void Cancel()
        {
            _cancellationTokenSource?.Cancel();
            IsBusy = false;
        }

        private bool CanCancel() => IsBusy;

        // Обновление списка после закрытия диалога
        public void RefreshItems()
        {
            _ = LoadItemsAsync();
        }

        // При изменении текста поиска
        partial void OnSearchTextChanged(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _ = SearchAsync();
            }
            else
            {
                // Если текст пустой — загружаем все предметы
                _ = LoadItemsAsync();
            }
        }

        // При изменении типа
        partial void OnSelectedTypeChanged(FilterItem<ItemType>? value)
        {
            _ = SearchAsync();
        }

        // При изменении редкости
        partial void OnSelectedRarityChanged(FilterItem<ItemRarity>? value)
        {
            _ = SearchAsync();
        }
    }
    public class FilterItem<T>
    {
        public string Name { get; set; } = "Все";
        public object? Value { get; set; }

        public override string ToString() => Name;
    }
}