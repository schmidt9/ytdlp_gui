namespace ytdlp_gui;

public partial class MainPage : ContentPage
{

	public MainPage()
	{
		InitializeComponent();
	}

	private void OnStartDownloadClicked(object sender, EventArgs e)
	{
		if (!ValidateUrlEntry() || !ValidateSavePathEntry())
		{
			return;
		}


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
		try
		{
			var result = await FilePicker.Default.PickAsync();
			if (result != null)
			{
				string fileName = result.FileName;
				string fullPath = result.FullPath; // Путь к файлу (на некоторых ОС может быть виртуальным URI)

				SavePathEntry.Text = fullPath; // Устанавливаем путь в Entry
			}
		}
		catch (Exception ex)
		{
			await DisplayAlert("Error", $"An error occurred while picking a file: {ex.Message}", "OK");
		}
	}
}

