using System;

namespace Xamarin.Contacts
{
    public class ContactGroup
    {
        public ContactGroup()
        {
        }

        public string Title{ get; set; }

        public string Id{ get; set; }

        public string Notes{ get; set; }

        public int SummaryCount{ get; set; }

        public int SummaryWithPhones{ get; set; }

        public int Deleted{ get; set; }

        public int GroupVisible{ get; set; }

        public int ShoudSync{ get; set; }

        public string SystemId{ get; set; }

        public string DataSet{ get; set; }
    }
}

