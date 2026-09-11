using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using GameItemEditor.Core.Enums;
using GameItemEditor.Core.Models;

namespace GameItemEditor.Wpf.ViewModels
{
    public enum DialogMode
    {
        Create,
        Edit,
        Clone
    }

    public class ItemDialogViewModel : INotifyDataErrorInfo
    {
        private readonly DialogMode _mode;
        private readonly GameItem? _sourceItem;
        private readonly Dictionary<string, List<string>> _errors = new();

        public ItemDialogViewModel(DialogMode mode, GameItem? sourceItem = null)
        {
            _mode = mode;
            _sourceItem = sourceItem;

            // Инициализация команд
            SaveCommand = new RelayCommand(Save, CanSave);
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

        // Команды
        public IRelayCommand SaveCommand { get; }
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

        private bool CanSave() => !HasErrors && !string.IsNullOrWhiteSpace(Name)
                                  && BasePrice.HasValue && BasePrice.Value >= 0
                                  && Weight.HasValue && Weight.Value >= 0;

        private void Save()
        {
            Result = new GameItem
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

            CloseDialog?.Invoke(true);
        }

        private void Cancel()
        {
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
    }
}