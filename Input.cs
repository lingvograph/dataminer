namespace LingvoGraph
{
    struct Input
    {
        public string Text;
        public string Lang;

        public Input(string text, string lang)
        {
            Text = text;
            Lang = lang;
        }
    }
}