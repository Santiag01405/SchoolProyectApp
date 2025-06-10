using Android.App;
using Android.Runtime;
using Android.Content.Res;
using System;
using Android.OS;
using Android.Widget;
using Microsoft.Maui.Handlers;

namespace SchoolProyectApp
{
    [Application]
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
            EntryHandler.Mapper.AppendToMapping(nameof(Entry), (handler, view) =>
            {
                if (handler.PlatformView is EditText editText)
                {
                    // Color del texto (escrito)
                    editText.SetTextColor(Android.Graphics.Color.ParseColor("#6bbdda"));

                    // Quitar subrayado por defecto
                    editText.BackgroundTintList = ColorStateList.ValueOf(Android.Graphics.Color.Transparent);

                    // Cambiar borde al enfocar
                    editText.FocusChange += (sender, args) =>
                    {
                        if (handler.PlatformView is EditText editText)
                        {
                            // ✅ Color del texto escrito
                            editText.SetTextColor(Android.Graphics.Color.ParseColor("#6bbdda"));

                            // ✅ Eliminar subrayado
                            editText.BackgroundTintList = ColorStateList.ValueOf(Android.Graphics.Color.Transparent);

                            // ❌ No toques más el BackgroundTintList para no afectar el fondo visual
                        }

                    };

                }
            });
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}
