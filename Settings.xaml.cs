
using System;
using System.Xml.Linq;

namespace TrafficEscape2;

public partial class Settings : ContentPage
{
    private string difficulty;
    public event Action<string> DifficultySelected;
    private bool isDarkMode;



    public Settings()
	{
		InitializeComponent();
        string savedDifficulty = Preferences.Get("Difficulty", "normal");
        ChangetSelectedDifficulty(savedDifficulty);
        isDarkMode = Preferences.Get("IsDarkMode", false);
        ApplyTheme();

        Theme.Text = isDarkMode ? "Light Mode" : "Dark Mode";

    }
    private void ApplyTheme()
    {
        Application.Current.UserAppTheme =
            isDarkMode ? AppTheme.Dark : AppTheme.Light;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        int highScore = Preferences.Get("HighScore", 0);
        HighScoreLabel.Text = $"High Score: {highScore}";
    }

    private void ChangetSelectedDifficulty(string diff)
    {

        if (diff == "easy")
            DifficultyButton.Text = "Easy";
        else if (diff == "normal")
            DifficultyButton.Text = "Normal";
        else if (diff == "hard")
            DifficultyButton.Text = "Hard";
    }

    private void Theme_Clicked(object sender, EventArgs e)
    {
        //if (Theme.Text.Equals("Light Mode"))
            //Theme.Text = "Dark Mode";
       // else if (Theme.Text.Equals("Dark Mode"))
          //  Theme.Text = "Light Mode";

        isDarkMode = !isDarkMode;
        Preferences.Set("IsDarkMode", isDarkMode);
        ApplyTheme();
    }

    private void DifficultyButton_Clicked(object sender, EventArgs e)
    {
        if (DifficultyButton.Text.Equals("Normal"))
        {
            DifficultyButton.Text = "Hard";
            difficulty = "Hard";
            DifficultySelected?.Invoke("hard");
            Preferences.Set("Difficulty", "hard");
        }
        else if (DifficultyButton.Text.Equals("Hard"))
        {
            DifficultyButton.Text = "Easy";
            difficulty = "Easy";
            DifficultySelected?.Invoke("easy");
            Preferences.Set("Difficulty", "easy");
        }
        else
        {
            DifficultyButton.Text = "Normal";
            difficulty = "Normal";
            DifficultySelected?.Invoke("normal");
            Preferences.Set("Difficulty", "normal");
        }
    }

    private void ClearHighScoreButton_Clicked(object sender, EventArgs e)
    {
        Preferences.Remove("HighScore"); // Erase high score
        HighScoreLabel.Text = "High Score: 0";
    }
    private async void BackButton_ClickedAsync(object sender, EventArgs e)
    {
        await Navigation.PopAsync();

    }
}