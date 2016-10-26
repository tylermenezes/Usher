using System.Collections.Generic;

namespace Usher.Config.Entities
{
    public class Node
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public List<string> Tags { get; set; }
    }
}