namespace IdentityMapping
{
    using System.Collections.Generic;

    /// <summary>
    /// ルート情報
    /// </summary>
    class RouteInfo
    {
        public string Input { get; }

        public string Output { get; }

        public Dictionary<string, string> Properties { get; }

        public RouteInfo(string input, string output, Dictionary<string, string> properties)
        {
            Input = input;
            Output = output;
            Properties = properties;
        }
    }
}
