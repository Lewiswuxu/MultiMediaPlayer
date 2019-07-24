using System;
using System.Collections.Generic;
using System.Text;

namespace videoplay
{
    interface IPN
    {
        int insert(string path,string name,DateTime time);
        //int update(string path,string name,DateTime time);
        int delete(string path,string name);
        List<RecordMember> getTen();
    }
}
