using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Model
{
    public class xxxTask
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Complete { get; set; }
        public DateTime CreateDate {get; set;}
        public DateTime CompletionDate {get; set;}
        public int CertificateID { get; set; }
        public string CertificateName { get; set; }
        public int WarningPeriod { get; set; }
        public string Recurrance { get; set; }
        public bool Enabled { get; set; }
        public DateTime DueDate { get; set; }
        public bool Mandatory { get; set; }
        public List<int> Roles { get; set; }
        public List<Document> Documents { get; set; }

        public xxxTask()
        {
            ID = 0;
            Name = "";
            Description = "";
            Complete = false;
            CreateDate = DateTime.Today;
            CompletionDate = DateTime.MaxValue;
            CertificateID = 0;
            CertificateName = "";
            WarningPeriod = 0;
            Recurrance = "";
            Enabled = false;
            DueDate = DateTime.MaxValue;
            Mandatory = false;
            Roles = new List<int>();
            Documents = new List<Document>();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
