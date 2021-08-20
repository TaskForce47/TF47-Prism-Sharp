using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using TF47_Prism_Sharp_Client.Services;

namespace TF47_Prism_Sharp_Client
{
    public static class Extension
    {
        private static MediatorService _mediatorService;

        public static unsafe delegate* unmanaged<string, string, string, int> Callback;

        [UnmanagedCallersOnly(EntryPoint = "RVExtensionRegisterCallback")]
        public static unsafe void RvExtensionRegisterCallback(delegate* unmanaged<string, string, string, int> callback)
        {
            Callback = callback;
            Console.WriteLine("Loaded Callback");
        }

        [UnmanagedCallersOnly(EntryPoint = "RVExtension")]
        public static unsafe void RVExtension(char* output, int outputSize, char* function)
        {
            var method = Marshal.PtrToStringAnsi((IntPtr) function) ?? "";

            var result = "";
            switch (method)
            {
                default:
                    result = "Method not implemented";
                    break;
            }

            byte[] byteFinalString = Encoding.ASCII.GetBytes(result);
            Marshal.Copy(byteFinalString, 0, (IntPtr) output, byteFinalString.Length);
        }

        [UnmanagedCallersOnly(EntryPoint = "RVExtensionArgs")]
        public static unsafe int RVExtensionArgs(char* output, int outputSize, char* function, char** argv, int argc)
        {
            var method = Marshal.PtrToStringAnsi((IntPtr) function) ?? "";

            var parameters = new List<string>();
            for (int i = 0; i < argc; i++)
            {
                var tmp = Marshal.PtrToStringAnsi((IntPtr) argv[i]) ?? "";
                tmp = tmp.Replace("\"", "");
                parameters.Add(tmp);
            }

            var result = string.Empty;

            switch (method)
            {

                default:
                    result = "Method not implemented";
                    break;
            }

            byte[] byteFinalString = Encoding.ASCII.GetBytes(result);
            Marshal.Copy(byteFinalString, 0, (IntPtr) output, byteFinalString.Length);
            return 100;
        }

        [UnmanagedCallersOnly(EntryPoint = "RVExtensionVersion")]
        public static unsafe void RVExtensionVersion(char* output, int outputSize)
        {
            //Application.ReadConfiguration();    
            Application.BuildApplication();

            var scope = Application.ServiceProvider.CreateScope();
            _mediatorService = scope.ServiceProvider.GetRequiredService<MediatorService>();

            var versionString = "0.2";

            byte[] byteFinalString = Encoding.ASCII.GetBytes(versionString);
            Marshal.Copy(byteFinalString, 0, (IntPtr) output, byteFinalString.Length);
        }
    }
}