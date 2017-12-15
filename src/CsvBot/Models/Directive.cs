namespace CsvBot.Models
{
    internal class Directive
    {
        public string Name { get; set; }
        public string Rename { get; set; }
        public int Position { get; set; }
        public int To { get; set; }
        public string Action { get; set; }
    }
}
