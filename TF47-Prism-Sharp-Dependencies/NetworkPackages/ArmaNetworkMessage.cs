using System;

namespace TF47_Prism_Sharp_Dependencies.NetworkPackages
{
    public class ArmaNetworkMessage
    {
        public Guid ServerId { get; set; }
        
        public string Namespace { get; set; }
        public string VariableName { get; set; }
        public string Data { get; set; }
        public bool IsJip { get; set; }
        public string TargetId { get; set; }
        public string SenderId { get; set; }
    }
}