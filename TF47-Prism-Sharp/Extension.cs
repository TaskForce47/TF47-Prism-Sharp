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
            var method = Marshal.PtrToStringAnsi((IntPtr)function) ?? "";

            var result = "";
            switch (method)
            {
                case "endSession":
                    result = _mediatorService.EndSession().ToString();
                    break;
                default:
                    result = "Method not implemented";
                    break;
            }
            
            byte[] byteFinalString = Encoding.ASCII.GetBytes(result);
            Marshal.Copy(byteFinalString,0,(IntPtr)output,byteFinalString.Length);
        }

        [UnmanagedCallersOnly(EntryPoint = "RVExtensionArgs")]
        public static unsafe void RVExtensionArgs(char* output, int outputSize, char* function, char** argv, int argc)
        {
            var method = Marshal.PtrToStringAnsi((IntPtr)function) ?? "";

            var parameters = new List<string>();
            for (int i=0; i<argc; i++) {
                var tmp = Marshal.PtrToStringAnsi((IntPtr)argv[i]) ?? "";
                tmp = tmp.Replace("\"", "");
                parameters.Add(tmp);
            }

            var result = string.Empty;

            switch (method)
            {
                case "createSession":
                    result = _mediatorService.CreateSession(parameters[0], parameters[1], int.Parse(parameters[2])).ToString();
                    break;
                case "endSession":
                    result = _mediatorService.EndSession().ToString();
                    break;
                case "updateTicketCount":
                    _mediatorService.UpdateTicketCount(Convert.ToInt32(parameters[0]), Convert.ToInt32(parameters[1]), parameters[2],
                        parameters[3]);
                    result = "200";
                    break;
                case "getPlayerPermissions":
                    _mediatorService.UpdatePlayerPermissions(parameters[0]);
                    result = "200";
                    break;
                case "createPlayer":
                    _mediatorService.CreateUser(parameters[0], parameters[1]);
                    result = "200";
                    break;
                default:
                    result = "Method not implemented";
                    break;
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

            var versionString = "0.2"; 
            
            byte[] byteFinalString = Encoding.ASCII.GetBytes(versionString);
            Marshal.Copy(byteFinalString,0,(IntPtr)output,byteFinalString.Length);
        }
    }
}