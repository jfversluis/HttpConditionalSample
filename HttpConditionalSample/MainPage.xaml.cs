using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace HttpConditionalSample
{
	public partial class MainPage : ContentPage, INotifyPropertyChanged
	{
		private readonly HttpClient _customHttpClient;
		private readonly EtagHttpClientHandler _customHttpClientHandler;

		private const string ApiBaseUrl = "http://airplanemodeproof-api.azurewebsites.net/api/";
		bool isLoading;

		public bool IsLoading
		{
			get
			{
				return isLoading;
			}

			set
			{
				isLoading = value;
				OnPropertyChanged(nameof(IsLoading));
			}
		}

		public ObservableCollection<Superhero> Superheroes { get; } = new ObservableCollection<Superhero>();
		public Command RefreshDataCommand { get; }

		public MainPage()
		{
			InitializeComponent();

			_customHttpClientHandler = new EtagHttpClientHandler();
			_customHttpClient = new HttpClient(_customHttpClientHandler)
			{
				BaseAddress = new Uri(ApiBaseUrl)
			};

			RefreshDataCommand = new Command(async () => await GetSuperheroes());


			BindingContext = this;

			RefreshDataCommand.Execute(null);
		}

		public async Task GetSuperheroes()
		{
			IsLoading = true;

			try
			{
				var superheroesJson = await _customHttpClient.GetStringAsync("superhero");
				var superheroes = JsonConvert.DeserializeObject<Superhero[]>(superheroesJson);

				Superheroes.Clear();

				foreach (var hero in superheroes)
					Superheroes.Add(hero);
			}
			catch (HttpRequestException ex) when (ex.Message.Contains("304"))
			{
				// Intentionally left blank, 304 response is fine
			}
			finally
			{
				IsLoading = false;
			}
		}
	}
}