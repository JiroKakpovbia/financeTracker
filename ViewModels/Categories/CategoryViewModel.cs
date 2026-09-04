using CommunityToolkit.Mvvm.ComponentModel;
using trackr.Models;

namespace trackr.ViewModels
{
    public partial class CategoryViewModel(Category model) : ObservableObject
    {
        // Default colour resource for each category
        private string DefaultColourResource => Name switch
        {
            "Income" => "CategoryGreenSurface",
            "Savings" => "CategoryTealSurface",
            "Housing" => "CategoryOrangeSurface",
            "Communications" => "CategoryBlueSurface",
            "Food" => "CategoryYellowSurface",
            "Insurance" => "CategoryPurpleSurface",
            "Transportation" => "CategorySkySurface",
            "Education" => "CategoryIndigoSurface",
            "Recreation" => "CategoryPinkSurface",
            "Personal Care" => "CategoryRoseSurface",
            "Fees" => "CategoryRedSurface",
            "Transfers" => "CategoryMintSurface",
            _ => "CategoryGreySurface"
        };

        private Color DefaultColour
        {
            get
            {
                bool isDarkMode = Application.Current?.RequestedTheme == AppTheme.Dark;
                string resourceKey = $"{DefaultColourResource}{(isDarkMode ? "Dark" : "Light")}";
                
                if (Application.Current?.Resources.TryGetValue(resourceKey, out object? resource) == true && resource is Color colour)
                    return colour;
                return isDarkMode ? Color.FromArgb("#2C2C2C") : Color.FromArgb("#EFEFEF"); // Fallback if the resource cannot be found
            }
        }

        public Category Model { get; } = model;

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