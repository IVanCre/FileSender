using Android.App;
using Android.OS;
using Android.Runtime;
using AndroidX.AppCompat.App;
using Android.Widget;//содержит определение контролов ?
using Android.Content.PM;
using System.Threading.Tasks;
using Xamarin.Essentials;
using System.Collections.Generic;




namespace FileSender
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true, Icon ="@drawable/icon_pic")]
    public class MainActivity : AppCompatActivity
    {
        public static  EditText loginText;
        public static  EditText paswText;

        public static  TextView responseText;
        public static string UserSetsFilename = "/storage/emulated/0/Android/data/com.Nosferatu.FileSender/userSets.bin";//настройки для подключения клиента
        public static Button acceptSettingsButton;
        public static Button selectFilesButton;
        public static Button sendDataButton;
        public List<string> selectedFileNames = new List<string>();



        protected override void OnCreate(Bundle savedInstanceState)
        {
            Platform.Init(this, savedInstanceState);

            base.OnCreate(savedInstanceState);
            TryToGetPermissions();//запрашиваем подтверждение разрешения на доступ к файлам

            SetContentView(Resource.Layout.activity_main);// Set our view from the "main" layout resource
            loginText = FindViewById<EditText>(Resource.Id.editText1);

            paswText = FindViewById<EditText>(Resource.Id.editText2);


            responseText = FindViewById<TextView>(Resource.Id.textView3);



            sendDataButton = FindViewById<Button>(Resource.Id.button2);
            sendDataButton.Visibility = Android.Views.ViewStates.Invisible;
            sendDataButton.Click += (sender, e) =>
            {
                ButtonFuncs.ConnectToServer(selectedFileNames);

                //после отправки данных возвращаем ресурсы к исходному состоянию(даже если сервак не ответил)
                selectedFileNames.Clear();
                selectFilesButton.Text = "Выбрать файлы";
                sendDataButton.Visibility = Android.Views.ViewStates.Invisible;
            };

            selectFilesButton = FindViewById<Button>(Resource.Id.button3);
            selectFilesButton.Click += (sender, e) =>
            {
                ButtonFuncs.PickFiles(selectedFileNames, sendDataButton, selectFilesButton);
            };





            acceptSettingsButton = FindViewById<Button>(Resource.Id.button1);//кнопка появляется, если файл юзера пуст(еще нет логина+пароль)
            acceptSettingsButton.Click += (sender, e) =>
            {
                paswText.Visibility = Android.Views.ViewStates.Visible;//для ввода данных, поля становятся видимыми
                ButtonFuncs.SaveUserInfo();
                selectFilesButton.Visibility = Android.Views.ViewStates.Visible;
            };

            if (ButtonFuncs.CheckPresentUserSets())//разрешение уже было получено
            {
                paswText.Visibility = Android.Views.ViewStates.Invisible;//скрываем ненужные поля(данные уже есть)
                selectFilesButton.Visibility = Android.Views.ViewStates.Visible;
            }
        }



        async Task TryToGetPermissions()
        {
            if ((int)Build.VERSION.SdkInt >= 23)
            {
                await GetPermissionsAsync();
                return;
            }
        }
        const int RequestId = 0;

        readonly string[] PermissionsGroupLocation =
        {
          Android.Manifest.Permission.WriteExternalStorage,
          Android.Manifest.Permission.ReadExternalStorage,
          Android.Manifest.Permission.AccessMediaLocation
        };
        async Task GetPermissionsAsync()//проверяет, есть ли требуемые разрешения 
        {
            const string permission = Android.Manifest.Permission.AccessFineLocation;

            if (CheckSelfPermission(permission) == (int)Android.Content.PM.Permission.Granted)//разрешение уже было получено
            {
                Toast.MakeText(this, "Разрешение успешно получено", ToastLength.Short).Show();
                return;
            }

            if (ShouldShowRequestPermissionRationale(permission))//разрешение еще не было получено, запрос на подтверждение(новая форма)
            {
                //set alert for executing the task
                Android.App.AlertDialog.Builder alert = new Android.App.AlertDialog.Builder(this);
                alert.SetTitle("Permissions Needed");
                alert.SetMessage("Приложению требуются разрешения для дальнейшей работы");


                alert.SetPositiveButton("Request Permissions", (senderAlert, args) =>
                {
                    RequestPermissions(PermissionsGroupLocation, RequestId);
                });
                alert.SetNegativeButton("Cancel", (senderAlert, args) =>
                {
                    Toast.MakeText(this, "Cancelled!", ToastLength.Short).Show();
                });

                Dialog dialog = alert.Create();//показ миниформы для выбора
                dialog.Show();
                return;
            }

            RequestPermissions(PermissionsGroupLocation, RequestId);
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)//загружается, когда требуется показать результат запроса
        {
            switch (requestCode)
            {
                case RequestId:
                    {
                        if (grantResults[0] == (int)Permission.Granted)
                        {
                            Toast.MakeText(this, "Доступ к файлам разрешен", ToastLength.Short).Show();
                        }
                        else
                        {
                            Toast.MakeText(this, "Доступ к файлам запрещен!", ToastLength.Short).Show();
                        }
                    }
                    break;
            }
        }
    }
}