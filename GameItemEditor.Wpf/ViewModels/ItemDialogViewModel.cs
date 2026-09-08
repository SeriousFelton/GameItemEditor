using System;
using System.Collections.Generic;
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

    public class ItemDialogViewModel
    {
        private readonly DialogMode _mode;
        private readonly GameItem? _sourceItem;

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
        public string Name { get; set; } = string.Empty;
        public ItemType Type { get; set; }
        public ItemRarity Rarity { get; set; }
        public decimal BasePrice { get; set; }
        public double Weight { get; set; }
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

        // Команды
        public IRelayCommand SaveCommand { get; }
        public IRelayCommand CancelCommand { get; }

        // Событие для закрытия окна
        public Action<bool>? CloseDialog { get; set; }

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

        private bool CanSave() => !string.IsNullOrWhiteSpace(Name) && BasePrice >= 0 && Weight >= 0;
        
        private void Save()
        {
            Result = new GameItem
            {
                Id = Id,
                Name = this.Name,
                Type = this.Type,
                Rarity = this.Rarity,
                BasePrice = this.BasePrice,
                Weight = this.Weight,
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
    }
}