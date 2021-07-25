using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using TF47_Prism_Sharp.Services;

namespace TF47_Prism_Sharp
{
    public class Extension
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
            string parameter = Marshal.PtrToStringAnsi((IntPtr)function);
            
            byte[] byteFinalString = Encoding.ASCII.GetBytes("finalString");
            Marshal.Copy(byteFinalString,0,(IntPtr)output,byteFinalString.Length);
        }

        [UnmanagedCallersOnly(EntryPoint = "RVExtensionArgs")]
        public static unsafe void RVExtensionArgs(char* output, int outputSize, char* function, char** argv, int argc)
        {
            //Let's grab the string from the pointer passed from the Arma call to our extension
            //Note the explicit cast
            string method = Marshal.PtrToStringAnsi((IntPtr)function);

            var parameters = new List<string>();
            for (int i=0; i<argc; i++) {
                var tmp = Marshal.PtrToStringAnsi((IntPtr)argv[i]) ?? "";
                parameters.Add(tmp);
            }

            var result = string.Empty;

            switch (method)
            {
                case "createSession":
                {
                    break;   
                }
                case "endSession":
                {
                    break;
                }
                case "updateTicketCount":
                {
                    break;
                }
                case "getPlayerPermissions":
                {
                    break;
                }
                default:
                {
                    result = "Method not implemented";
                }
            }
            
            byte[] byteFinalString = Encoding.ASCII.GetBytes(result);
            Marshal.Copy(byteFinalString,0,(IntPtr)output,byteFinalString.Length);
        }

        [UnmanagedCallersOnly(EntryPoint = "RVExtensionVersion")]
        public static unsafe void RVExtensionVersion(char* output, int outputSize)
        {
            Application.ReadConfiguration();    
            Application.BuildApplication();

            var scope = Application.ServiceProvider.CreateScope();
            _mediatorService = scope.ServiceProvider.GetRequiredService<MediatorService>();
            
            var assembly = Assembly.GetExecutingAssembly();
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            var versionString = fileVersionInfo.ProductVersion ?? "Unknown";
            
            byte[] byteFinalString = Encoding.ASCII.GetBytes(versionString);
            Marshal.Copy(byteFinalString,0,(IntPtr)output,byteFinalString.Length);
        }
    }
}