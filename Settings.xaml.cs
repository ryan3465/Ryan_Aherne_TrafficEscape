
using System;
using System.Xml.Linq;

namespace TrafficEscape2;

public partial class Settings : ContentPage
{
    private string difficulty;
    public event Action<string> DifficultySelected;



    public Settings()
	{
		InitializeComponent();
        string savedDifficulty = Preferences.Get("Difficulty", "normal");
        ChangetSelectedDifficulty(savedDifficulty);

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


    private async void BackButton_ClickedAsync(object sender, EventArgs e)
    {
        await Navigation.PopAsync();

    }
}