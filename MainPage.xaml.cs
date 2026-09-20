namespace ytdlp_gui;

using CommunityToolkit.Maui.Storage;

public partial class MainPage : ContentPage
{

	public MainPage()
	{
		InitializeComponent();
		SetupUI();
	}

	private void SetupUI()
	{
		UrlEntry.Text = AppSettings.URL;
		SavePathEntry.Text = AppSettings.SavePath;
	}

	private void SaveSettings()
	{
		AppSettings.URL = UrlEntry.Text;
		AppSettings.SavePath = SavePathEntry.Text;
	}

	private void OnStartDownloadClicked(object sender, EventArgs e)
	{
		if (!ValidateUrlEntry() || !ValidateSavePathEntry())
		{
			return;
		}

		SaveSettings();
	}

	private void OnSelectSavePathClicked(object sender, EventArgs e)
	{
		SelectSavePath();
	}

	private bool ValidateUrlEntry()
	{
		if (string.IsNullOrWhiteSpace(UrlEntry.Text))
		{
			DisplayAlert("Error", "Please enter a valid YouTube URL.", "OK");
			return false;
		}

		return true;
	}

	private bool ValidateSavePathEntry()
	{
		if (string.IsNullOrWhiteSpace(SavePathEntry.Text))
		{
			DisplayAlert("Error", "Please enter a valid save path.", "OK");
			return false;
		}

		return true;
	}

	async void SelectSavePath()
	{
		var source = new CancellationTokenSource();

		var folderPicker = Handler?.MauiContext?.Services.GetService<IFolderPicker>();
		if (folderPicker == null) return;

        var result = await folderPicker.PickAsync(source.Token);

        if (result.IsSuccessful)
        {
            var folderPath = result.Folder.Path;

			SavePathEntry.Text = folderPath;

			SaveSettings();
		}
	}
}

