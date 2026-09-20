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

	private void AppendLog(string message)
	{
		MainThread.BeginInvokeOnMainThread(() =>
		{
			LogListView.ItemsSource ??= new List<string>();
			var logList = (List<string>)LogListView.ItemsSource;

			var messageWithTimestamp = $"[{DateTime.Now:HH:mm:ss}] {message}";
			logList.Add(messageWithTimestamp);

			LogListView.ItemsSource = null; // Reset the ItemsSource to refresh the ListView
			LogListView.ItemsSource = logList; // Reassign the updated list
		});
	}

	private async void StartDownload(string url, string savePath)
	{
		var arguments = $"-t mp4 {url} --paths {savePath}";

		var cancellationTokenSource = new CancellationTokenSource();
		var cancellationToken = cancellationTokenSource.Token;

		var exeDirectory = AppContext.BaseDirectory;
		AppendLog($"Executable directory: {exeDirectory}");

		var ytdlpPath = Path.Combine([exeDirectory, "ytdlp_bin", "yt-dlp.exe"]);

		AppendLog($"Using yt-dlp executable at: {ytdlpPath}");

		try
		{
			await Task.Run(async () =>
		{
			await foreach (var line in YtdlpRunner.RunYtdlpAsync(ytdlpPath, arguments, cancellationToken))
			{
				AppendLog(line);
			}
		}, cancellationToken);
		}
		catch (Exception ex)
		{
			AppendLog($"Error: {ex.Message}");
			await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
		}
	}

	private void OnStartDownloadClicked(object sender, EventArgs e)
	{
		if (!ValidateUrlEntry() || !ValidateSavePathEntry())
		{
			return;
		}

		SaveSettings();

		AppendLog($"Starting download for URL: {UrlEntry.Text}");

		StartDownload(UrlEntry.Text, SavePathEntry.Text);
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

			AppendLog($"Selected folder: {folderPath}");

			SaveSettings();
		}
	}
}

