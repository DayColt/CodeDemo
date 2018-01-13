
namespace LinkHandler.Core
{
    class LinksTableModel
    {
        public int ID { get; set; }
        public string Link { get; set; }
        public bool IsHandled { get; set; }

        public override string ToString()
        {
            return ID.ToString() + ": " + Link + " Handled: " + IsHandled.ToString();
        }
    }
}
