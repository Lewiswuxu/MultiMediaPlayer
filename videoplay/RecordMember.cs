using System;
using System.Collections.Generic;
using System.Text;

namespace videoplay
{
    class RecordMember
    {

        private string path;
        private string name;

        public string Path
        {
            get { return path;}
            set { path = value; }
        }
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
    }
}
