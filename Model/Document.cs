using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Model
{
    public class Document
    {   
        public int ID { get; set; }
        public string FileName { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Keywords { get; set; }
        public bool Public { get; set; }
        public List<Condition> Conditions { get; set; }
        
        public Document()
        {
            ID = 0;
            FileName = "";
            Name = "";
            Path = "";
            Keywords = "";
            Public = false;
            Conditions = new List<Condition>();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
