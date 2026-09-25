using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace QuantForge.App;

public sealed class MainPage : ContentPage
{
    public MainPage()
    {
        Title = "QuantForge";

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = 24,
                Spacing = 12,
                Children =
                {
                    new Label
                    {
                        Text = "QuantForge",
                        FontSize = 30,
                        FontAttributes = FontAttributes.Bold
                    },
                    new Label
                    {
                        Text = "Research workspace shell"
                    },
                    new Label
                    {
                        Text = "Live trading: disabled"
                    },
                    new Label
                    {
                        Text = "This shell displays validated read-only research state and does not hold broker, order-submission, or application-setting authority."
                    }
                }
            }
        };
    }
}
