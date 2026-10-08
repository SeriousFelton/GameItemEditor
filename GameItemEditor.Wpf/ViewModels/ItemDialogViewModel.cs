using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameItemEditor.Core.Enums;
using GameItemEditor.Core.Models;
using GameItemEditor.Wpf.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;

namespace GameItemEditor.Wpf.ViewModels
{
    public enum DialogMode
    {
        Create,
        Edit,
        Clone
    }

    public partial class ItemDialogViewModel : ObservableObject, INotifyDataErrorInfo
    {
        private readonly DialogMode _mode;
        private readonly GameItem? _sourceItem;
        private readonly Dictionary<string, List<string>> _errors = new();
        private readonly IApiClient _apiClient;
        private readonly ILogger<ItemDialogViewModel> _logger;
        private CancellationTokenSource _cancellationTokenSource;

        public ItemDialogViewModel(DialogMode mode, IApiClient apiClient, ILogger<ItemDialogViewModel> logger, GameItem? sourceItem = null)
        {
            _mode = mode;
            _sourceItem = sourceItem;
            _apiClient = apiClient;
            _logger = logger;

            // Инициализация команд
            SaveCommand = new AsyncRelayCommand(SaveAsync, CanSave);
            CancelCommand = new RelayCommand(Cancel);

            // Заполняем поля в зависимости от режима
            InitializeFromMode();
        }

        // Свойства для привязки
        public Guid Id { get; private set; }
        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    Validate(nameof(Name));
                    SaveCommand.NotifyCanExecuteChanged();
                }
            }
        }
        public ItemType Type { get; set; }
        public ItemRarity Rarity { get; set; }
        private decimal? _basePrice;
        public decimal? BasePrice
        {
            get => _basePrice;
            set
            {
                if (_basePrice != value)
                {
                    _basePrice = value;
                    Validate(nameof(BasePrice));
                    SaveCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private double? _weight;
        public double? Weight
        {
            get => _weight;
            set
            {
                if (_weight != value)
                {
                    _weight = value;
                    Validate(nameof(Weight));
                    SaveCommand.NotifyCanExecuteChanged();
                }
            }
        }
        public object Properties { get; set; } = new();

        // Списки для комбо-боксов в диалоге
        public List<ItemType> ItemTypes { get; } = Enum.GetValues<ItemType>().ToList();
        public List<ItemRarity> ItemRarities { get; } = Enum.GetValues<ItemRarity>().ToList();

        // Режим для результата в основном окне
        public DialogMode Mode => _mode;

        // Заголовок и текст кнопки
        public string Title { get; private set; } = "Редактор предмета";
        public string SaveButtonText { get; private set; } = "Сохранить";

        // Результат диалога
        public GameItem? Result { get; private set; }

        // Свойство для проверки есть ли ошибки
        public bool HasErrors => _errors.Any();

        // Состояние загрузки
        [ObservableProperty]
        private bool _isBusy;

        // Команды
        public IAsyncRelayCommand SaveCommand { get; }
        public IRelayCommand CancelCommand { get; }

        // Событие для закрытия окна
        public Action<bool>? CloseDialog { get; set; }
        // Событие для уведомления UI об изменении статуса ошибок
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        private void InitializeFromMode()
        {
            switch (_mode)
            {
                case DialogMode.Edit when _sourceItem != null:
                    Id = _sourceItem.Id;
                    Name = _sourceItem.Name;
                    Type = _sourceItem.Type;
                    Rarity = _sourceItem.Rarity;
                    BasePrice = _sourceItem.BasePrice;
                    Weight = _sourceItem.Weight;
                    Properties = DeserializeProperties(_sourceItem.PropertiesJson);
                    Title = "Редактирование предмета";
                    SaveButtonText = "Сохранить";
                    break;

                case DialogMode.Clone when _sourceItem != null:
                    Id = Guid.NewGuid();
                    Name = _sourceItem.Name + " (копия)";
                    Type = _sourceItem.Type;
                    Rarity = _sourceItem.Rarity;
                    BasePrice = _sourceItem.BasePrice;
                    Weight = _sourceItem.Weight;
                    Properties = DeserializeProperties(_sourceItem.PropertiesJson);
                    Title = "Клонирование предмета";
                    SaveButtonText = "Создать копию";
                    break;

                default: // Create
                    Id = Guid.NewGuid();
                    Name = "Новый предмет";
                    Validate(nameof(Name));
                    Type = ItemType.Weapon;
                    Rarity = ItemRarity.Common;
                    BasePrice = 0;
                    Weight = 0;
                    Properties = new object();
                    Title = "Создание предмета";
                    SaveButtonText = "Создать";
                    break;
            }
        }

        private object DeserializeProperties(string json)
        {
            if (string.IsNullOrEmpty(json)) 
                return new object();

            try
            {
                return JsonSerializer.Deserialize<object>(json) ?? new object();
            }
            catch
            {
                return new object();
            }
        }

        private bool CanSave() => !IsBusy
                                  && !HasErrors 
                                  && !string.IsNullOrWhiteSpace(Name)
                                  && BasePrice.HasValue && BasePrice.Value >= 0
                                  && Weight.HasValue && Weight.Value >= 0;

        private async Task SaveAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                _cancellationTokenSource = new CancellationTokenSource();
                var token = _cancellationTokenSource.Token;

                var item = new GameItem
                {
                    Id = Id,
                    Name = this.Name,
                    Type = this.Type,
                    Rarity = this.Rarity,
                    BasePrice = this.BasePrice ?? 0,
                    Weight = this.Weight ?? 0,
                    PropertiesJson = JsonSerializer.Serialize(this.Properties),
                    CreatedAt = _sourceItem?.CreatedAt ?? DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                switch (_mode)
                {
                    case DialogMode.Edit:
                        await _apiClient.UpdateItemAsync(item.Id, item, token);
                        break;

                    case DialogMode.Create:
                    case DialogMode.Clone:
                        await _apiClient.CreateItemAsync(item, token);
                        break;
                }

                Result = item;
                CloseDialog?.Invoke(true);
            }
            catch (OperationCanceledException ex)
            {
                // Проверка на отмену юзверем
                if (_cancellationTokenSource?.IsCancellationRequested == true)
                {
                    _logger.LogInformation("Сохранение отменено пользователем");
                }
                else
                {
                    // Это таймаут
                    _logger.LogError(ex, "Таймаут запроса к серверу");
                    MessageBox.Show(
                        "Сервер не отвечает.\nПопробуйте позже или проверьте подключение.",
                        "Таймаут",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Не удалось подключиться к серверу API");
                MessageBox.Show(
                    "Не удалось подключиться к серверу.\nПроверьте, что сервер запущен и доступен.",
                    "Ошибка подключения",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Непредвиденная ошибка при сохранении предмета");
                MessageBox.Show(
                    "Произошла непредвиденная ошибка при сохранении.\nПопробуйте ещё раз или обратитесь к администратору.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void Cancel()
        {
            _cancellationTokenSource?.Cancel();
            Result = null;
            CloseDialog?.Invoke(false);
        }

        public IEnumerable GetErrors(string? propertyName)
        {
            return _errors.GetValueOrDefault(propertyName) ?? Enumerable.Empty<string>();
        }

        private void Validate(string propertyName)
        {
            _errors.Remove(propertyName);

            switch(propertyName)
            {
                case nameof(Name):
                    if (string.IsNullOrWhiteSpace(Name))
                        AddError(propertyName, "Название не может быть пустым");
                    break;

                case nameof(BasePrice):
                    if (!BasePrice.HasValue)
                        AddError(propertyName, "Цена не может быть пустой");
                    else if (BasePrice.Value < 0)
                        AddError(propertyName, "Цена не может быть ниже 0");
                    break;

                case nameof(Weight):
                    if (!Weight.HasValue)
                        AddError(propertyName, "Вес не может быть пустым");
                    else if (Weight.Value < 0)
                        AddError(propertyName, "Вес не может быть ниже 0");
                    break;
            }

            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        private void AddError(string propertyName, string error)
        {
            if (!_errors.ContainsKey(propertyName))
                _errors[propertyName] = new List<string>();
            _errors[propertyName].Add(error);
        }

        partial void OnIsBusyChanged(bool value)
        {
            SaveCommand.NotifyCanExecuteChanged();
        }
    }
}