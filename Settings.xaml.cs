using System;
using System.Xml.Linq;

namespace TrafficEscape2;

// Settings page for the game
public partial class Settings : ContentPage
{
    // -------------------- FIELDS & EVENTS --------------------

    // Stores the current difficulty selection
    private string difficulty;

    // Event that notifies subscribers when a new difficulty is selected
    public event Action<string> DifficultySelected;

    // Tracks whether dark mode is active
    private bool isDarkMode;

    // -------------------- CONSTRUCTOR --------------------
    public Settings()
    {
        InitializeComponent();

        // Load saved difficulty from preferences (default to "normal")
        string savedDifficulty = Preferences.Get("Difficulty", "normal");
        ChangetSelectedDifficulty(savedDifficulty);

        // Load saved theme (dark mode or light mode)
        isDarkMode = Preferences.Get("IsDarkMode", false);
        ApplyTheme();

        // Set the theme button text to show the opposite of current theme
        Theme.Text = isDarkMode ? "Light Mode" : "Dark Mode";
    }

    // -------------------- METHODS --------------------

    // Applies the theme based on isDarkMode
    private void ApplyTheme()
    {
        Application.Current.UserAppTheme =
            isDarkMode ? AppTheme.Dark : AppTheme.Light;
    }

    // Called when the page appears on screen
    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Display the current high score
        int highScore = Preferences.Get("HighScore", 0);
        HighScoreLabel.Text = $"High Score: {highScore}";
    }

    // Updates the difficulty button text based on the current difficulty
    private void ChangetSelectedDifficulty(string diff)
    {
        if (diff == "easy")
            DifficultyButton.Text = "Easy";
        else if (diff == "normal")
            DifficultyButton.Text = "Normal";
        else if (diff == "hard")
            DifficultyButton.Text = "Hard";
    }

    // Toggle theme between dark and light mode
    private void Theme_Clicked(object sender, EventArgs e)
    {
        // Toggle the isDarkMode boolean
        isDarkMode = !isDarkMode;

        // Save new theme preference
        Preferences.Set("IsDarkMode", isDarkMode);

        // Apply the updated theme
        ApplyTheme();
    }

    // Handles clicks on the difficulty button
    private void DifficultyButton_Clicked(object sender, EventArgs e)
    {
        // Cycle through Normal ? Hard ? Easy ? Normal
        if (DifficultyButton.Text.Equals("Normal"))
        {
            DifficultyButton.Text = "Hard";
            difficulty = "Hard";

            // Notify subscribers that difficulty changed
            DifficultySelected?.Invoke("hard");

            // Save to preferences
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

    // Clears the saved high score
    private void ClearHighScoreButton_Clicked(object sender, EventArgs e)
    {
        Preferences.Remove("HighScore"); // Delete saved high score
        HighScoreLabel.Text = "High Score: 0"; // Update UI
    }

    // Navigate back to the previous page
    private async void BackButton_ClickedAsync(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}