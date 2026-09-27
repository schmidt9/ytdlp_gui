namespace ytdlp_gui;

using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Storage;
using Microsoft.Maui.ApplicationModel.DataTransfer;

public partial class MainPage : ContentPage
{

	private readonly ObservableCollection<string> _logItems = new();

	private bool _isDownloading = false;

	private CancellationTokenSource? _cancellationTokenSource;

	public MainPage()
	{
		InitializeComponent();
		SetupUI();
	}

	private void SetupUI()
	{
		UrlEntry.Text = AppSettings.URL;
		SavePathEntry.Text = AppSettings.SavePath;
		LogListView.ItemsSource = _logItems;
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
			var messageWithTimestamp = $"[{DateTime.Now:HH:mm:ss}] {message}";

			_logItems.Add(messageWithTimestamp);

			LogListView.ScrollTo(messageWithTimestamp, ScrollToPosition.End, animated: true);
		});
	}

	private async void StartDownload(string url, string savePath)
	{
		var arguments = new List<string>
		{
			"-t", "mp4",
			url,
			"--paths", savePath
		};

		_cancellationTokenSource = new CancellationTokenSource();
		var cancellationToken = _cancellationTokenSource.Token;

		var exeDirectory = AppContext.BaseDirectory;
		AppendLog($"Executable directory: {exeDirectory}");

		var ytdlpPath = Path.Combine([exeDirectory, "ytdlp", "yt-dlp.exe"]);

		AppendLog($"Starting download for URL: {url}");

		AppendLog($"Running yt-dlp executable at: '{ytdlpPath}' with arguments: '{string.Join(" ", arguments)}'");

		try
		{
			await Task.Run(async () =>
		{
			await foreach (var line in YtdlpRunner.RunYtdlpAsync(ytdlpPath, arguments, cancellationToken))
			{
				AppendLog(line);
			}

			AppendLog("Download process completed.\n\n");
		}, cancellationToken);
		}
		catch (OperationCanceledException)
		{
			AppendLog("Download canceled by user");
			HandleDownloadStop();
		}
		catch (Exception ex)
		{
			var message = $"An error occurred: {ex.Message}";
			AppendLog(message);
			await DisplayAlert("Error", message, "OK");
		}
		finally
		{
			HandleDownloadStop();
		}
	}

	private void CancelDownload()
	{
		_cancellationTokenSource?.Cancel();
	}

	private void HandleDownloadStop()
	{
		_isDownloading = false;

		ToggleActivityIndicatorVisible(false);
		UpdateDownloadButton(false);
	}

	private void ToggleActivityIndicatorVisible(bool isActive)
	{
		MainThread.BeginInvokeOnMainThread(() =>
		{
			DownloadActivityIndicator.IsRunning = isActive;
			DownloadActivityIndicator.IsVisible = isActive;
		});
	}

	private void UpdateDownloadButton(bool isDownloading)
	{
		MainThread.BeginInvokeOnMainThread(() =>
		{
			DownloadBtn.Text = isDownloading ? "Cancel" : "Download";
		});
	}

	private void OnStartDownloadClicked(object sender, EventArgs e)
	{
		if (!_isDownloading && (!ValidateUrlEntry() || !ValidateSavePathEntry()))
		{
			return;
		}

		SaveSettings();

		_isDownloading = !_isDownloading;

		ToggleActivityIndicatorVisible(_isDownloading);

		UpdateDownloadButton(_isDownloading);

		if (_isDownloading)
		{
			StartDownload(UrlEntry.Text, SavePathEntry.Text);
		}
		else
		{
			CancelDownload();
		}
	}

	private void OnSelectSavePathClicked(object sender, EventArgs e)
	{
		SelectSavePath();
	}

	private bool LogListViewHasItems(object sender, EventArgs e)
	{
		return _logItems.Count > 0;
	}

	private void OnLogListViewCopyAllClicked(object sender, EventArgs e)
	{
		var allLogText = string.Join(Environment.NewLine, _logItems);

		MainThread.BeginInvokeOnMainThread(async () =>
		{
			await Clipboard.Default.SetTextAsync(allLogText);
		});
	}

	private void OnLogListViewClearAllClicked(object sender, EventArgs e)
	{
		_logItems.Clear();
	}

	private bool ValidateUrlEntry()
	{
		if (string.IsNullOrWhiteSpace(UrlEntry.Text))
		{
			DisplayAlert("Error", "Please enter a valid URL", "OK");
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

