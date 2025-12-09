
namespace TrafficEscape2;

public partial class Settings : ContentPage
{
	public Settings()
	{
		InitializeComponent();
	}

    private void Theme_Clicked(object sender, EventArgs e)
    {

    }

    private void DifficultyButton_Clicked(object sender, EventArgs e)
    {
        if (DifficultyButton.Text.Equals("Normal"))
            DifficultyButton.Text = "Hard";
        else
            DifficultyButton.Text = "Normal";
    }


    private async void BackButton_ClickedAsync(object sender, EventArgs e)
    {
        await Navigation.PopAsync();

    }
}