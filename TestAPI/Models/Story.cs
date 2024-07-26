namespace TestAPI.Models
{
    public class Story:IComparable<Story>
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public long Id { get; set; }
        public string   PostedBy { get; set; }

        public int CompareTo(Story? other)
        {
            if (other.Id < this.Id) return 1;
            else if (other.Id > this.Id) return -1;
            else return 0;
        }
    }
}
