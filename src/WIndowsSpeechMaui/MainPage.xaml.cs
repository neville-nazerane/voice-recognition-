using System.Globalization;
using System.Speech.Recognition;
using Windows.Media.SpeechRecognition;

namespace WIndowsSpeechMaui
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        static readonly HttpClient client = new()
        {
            BaseAddress = new Uri("http://192.168.1.155:5010")
        };

        async Task ListenToMeNowAsync()
        {
            var prem = await Permissions.RequestAsync<Permissions.Speech>();

            SpeechRecognitionEngine recognizer = new(new CultureInfo("en-US"));

            Choices commands = new();
            commands.Add(new string[] {
                "Smarty",
                "Turn on Lights",
                "Turn off Lights",
                "Turn on bedroom",
                "Turn off bedroom",
                "Turn on TV",
                "Turn on movie mode"
            });
            GrammarBuilder gBuilder = new();
            gBuilder.Append(commands);
            Grammar grammar = new(gBuilder);
            recognizer.LoadGrammar(grammar);

            //recognizer.LoadGrammar(new DictationGrammar());


            recognizer.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(Recognizer_SpeechRecognized);

            recognizer.SetInputToDefaultAudioDevice();
            recognizer.RecognizeAsync(RecognizeMode.Multiple);



            return;

            //var speechRecognizer = new SpeechRecognizer();
            //await speechRecognizer.CompileConstraintsAsync();
            //try
            //{
            //    var res = await speechRecognizer.RecognizeAsync();

            //    await Task.Delay(3000);

            //    CounterBtn.Text = res.Text;
            //}
            //catch (Exception ex)
            //{
            //}
            //SpeechRecognitionResult speechRecognitionResult = await speechRecognizer.RecognizeWithUIAsync();

        }

        async void Recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Text == "Turn off Lights")
                await client.PutAsync("philipsHue/switchLight/ff9e4968-20f7-41f4-8bf3-3e045564896c/False", null);
            else if (e.Result.Text == "Turn on Lights")
                await client.PutAsync("philipsHue/switchLight/ff9e4968-20f7-41f4-8bf3-3e045564896c/True", null);

            Title.Text = e.Result.Text;
            Console.WriteLine("Recognized text: " + e.Result.Text);
        }


        private async void OnCounterClicked(object sender, EventArgs e)
        {
            await ListenToMeNowAsync();
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }
    }

}
