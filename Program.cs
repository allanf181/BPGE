namespace BPGE
{
    using Terminal.Gui;
    using CommandLine;

    class BPGE
    {
        private class Options
        {
            [Option(Required = false, Default = false, HelpText = "Show debug output")]
            public bool Debug { get; set; }
            
            [Option('p', "port", Required = false, Default = 12345, HelpText = "Intiface Websocket Port")]
            public int IntifacePort { get; set; }
        }

        private static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(o =>
                {
                    Application.Init();
                    var bpge = new BPGEView(o.Debug, o.IntifacePort);

                    try
                    {
                        Application.Run(bpge);
                    }
                    finally
                    {
                        Application.Shutdown();
                    }
                });
        }
    }
}


