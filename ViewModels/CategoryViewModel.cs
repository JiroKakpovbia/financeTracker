using CommunityToolkit.Mvvm.ComponentModel;
using trackr.Models;

namespace trackr.ViewModels
{
    public partial class CategoryViewModel(Category model) : ObservableObject
    {
        public Category Model { get; } = model;

        public int Id => Model.Id;

        public string Name
        {
            get => Model.Name;
            set => SetProperty(Model.Name, value, Model, (m, v) => m.Name = v);
        }

        public Color? Colour
        {
            get => string.IsNullOrEmpty(Model.Colour)
                ? null
                : Color.FromArgb(Model.Colour);

            set => SetProperty(
                Model.Colour,
                value?.ToArgbHex(),
                Model,
                (m, v) => m.Colour = v);
        }

        public string? Icon
        {
            get => Model.Icon;
            set => SetProperty(Model.Icon, value, Model, (m, v) => m.Icon = v);
        }
    }
}