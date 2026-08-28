using CommunityToolkit.Mvvm.ComponentModel;
using trackr.Models;

namespace trackr.ViewModels
{
    public partial class CategoryViewModel(Category model) : ObservableObject
    {
        // Default colours for categories if no colour is specified
        private Color DefaultColour => Name switch
        {
            "Income" => Color.FromArgb("#f50202"),
            "Savings" => Color.FromArgb("#f98704"),
            "Housing" => Color.FromArgb("#edfe00"),
            "Communications" => Color.FromArgb("#5eff00"),
            "Food" => Color.FromArgb("#00ff6a"),
            "Insurance" => Color.FromArgb("#00ffa2"),
            "Transportation" => Color.FromArgb("#0080ff"),
            "Education" => Color.FromArgb("#0000ff"),
            "Recreation" => Color.FromArgb("#9000ff"),
            "Personal Care" => Color.FromArgb("#b700ff"),
            "Fees" => Color.FromArgb("#ff0080"),
            "Transfers" => Color.FromArgb("#ffb9d4"),
            _ => Color.FromArgb("#ABA9A9")
        };

        public Category Model { get; } = model;

        public int Id => Model.Id;

        public string Name
        {
            get => Model.Name;
            set
            {
                if (SetProperty(Model.Name, value, Model, (m, v) => m.Name = v))
                    OnPropertyChanged(nameof(Icon));
            }
        }

        public Color Colour
        {
            get => !string.IsNullOrWhiteSpace(Model.Colour)
                ? Color.FromArgb(Model.Colour)
                : DefaultColour;

            set
            {
                if (SetProperty(
                    Model.Colour,
                    value?.ToArgbHex(),
                    Model,
                    (m, v) => m.Colour = v))
                {
                    OnPropertyChanged();
                }
            }
        }

        public string Icon => Name switch
        {
            "Income" => "\uf058",
            "Savings" => "\uf4d3",
            "Housing" => "\uf015",
            "Communications" => "\uf095",
            "Food" => "\uf0f5",
            "Insurance" => "\uf0c2",
            "Transportation" => "\uf0b1",
            "Education" => "\uf0d6",
            "Recreation" => "\uf059",
            "Personal Care" => "\uf059",
            "Fees" => "\uf059",
            "Transfers" => "\uf058",
            _ => "\uf059"
        };
    }
}