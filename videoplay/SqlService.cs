using System;
using System.Collections.Generic;
using System.Text;

namespace videoplay
{
    class SqlService
    {
        IPN sqlTool = null;

        public void chooseSqlService()
        {
            sqlTool = new SqlOperator();
        }

        public string delete(string path, string name)
        {
            int a = 0;
            string retValue = "";
            a = sqlTool.delete(path,name);
            if (a != 0)
            {
                retValue = "删除成功！";
            }
            else
            {
                retValue = "删除失败！";
            }
            return retValue;
        }
        public string insert(string path, string name,DateTime time)
        {
            int a = 0;
            string retValue = "";
            a = sqlTool.insert(path, name,time);
            if (a != 0)
            {
                retValue = "删除成功！";
            }
            else
            {
                retValue = "删除失败！";
            }
            return retValue;
        }

        //public string update(string path, string name, DateTime time)
        //{
        //    int a = 0;
        //    string retValue = "";
        //    a = sqlTool.update(path, name,time);
        //    if (a != 0)
        //    {
        //        retValue = "删除成功！";
        //    }
        //    else
        //    {
        //        retValue = "删除失败！";
        //    }
        //    return retValue;
        //}

        public List<RecordMember> getTen()
        {
            List<RecordMember> list = sqlTool.getTen();
            return list;
        }

        
    }
}
