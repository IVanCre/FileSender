
using Xamarin.Essentials;


namespace FileSender
{
    class UniversalIO
    {

        public static void PrintAll(string message)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                MainActivity.responseText.Text += message + "\n";
            });
        }


        public static void Print(string message)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                MainActivity.responseText.Text = message + "\n";
            });
        }
    }
}
