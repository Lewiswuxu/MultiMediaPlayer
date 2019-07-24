using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace videoplay
{
    //操作数据库类
    class SqlOperator:IPN
    {
        //连接语句
        //string ConnString = @"server=.;database=Record;integrated security=SSPI";
        string ConnString = @"server=.;database=Record;uid=sa;pwd=123456";
        string sql;

        public int delete(string path, string name)
        {
            //返回值
            int retValue = 0;
            //主键
            string pn = path +"\\"+ name;
            sql = "delete from HistoryRecord where fPN ='"+pn+"'";
            //创建SQL连接对象
            SqlConnection mycon = new SqlConnection(ConnString);
            //打开
            mycon.Open();

            SqlCommand sqlcom = new SqlCommand(sql, mycon);

            retValue = sqlcom.ExecuteNonQuery();

            sqlcom.Dispose();
            mycon.Close();

            return retValue;
        }
          
        //插入表中，若已存在，则更新
        public int insert(string path,string name,DateTime time)
        {
            int retValue = 0;
            string pn = path +"\\"+ name;

            //sql = "insert into HistoryRecord(fPN,fPath,fName,time) values('" + pn + "','" + path + "','" + name + "','" + time + "') ON DUPLICATE KEY UPDATE path=values('"+path+"') ";
            sql = "if exists (select * from HistoryRecord where fPN='" + pn + "') update HistoryRecord set time='" + time + "' where fPN='"+pn+"' else insert into HistoryRecord(fPN,fPath,fName,time) values('" + pn + "','" + path + "','" + name + "','" + time + "')";
            SqlConnection mycon = new SqlConnection(ConnString);
            mycon.Open();
            SqlCommand sqlcom = new SqlCommand(sql,mycon);
            retValue = sqlcom.ExecuteNonQuery();

            sqlcom.Dispose();
            mycon.Close();

            return retValue;
        }

        //update,已包含在insert中
        #region
        //public int update(string path,string name,DateTime time)
        //{
        //    int retValue = 0;
        //    string pn = path +"\\"+ name;

        //    sql = "update HistoryRecord set time='" + time + "'where fPN='"+pn+"'";
        //    SqlConnection mycon = new SqlConnection(ConnString);
        //    mycon.Open();
        //    SqlCommand sqlcom = new SqlCommand(sql,mycon);
        //    retValue = sqlcom.ExecuteNonQuery();

        //    sqlcom.Dispose();
        //    mycon.Close();

        //    return retValue;
        //}
        #endregion

        //获取十个历史记录
        public List<RecordMember> getTen()
        {
            List<RecordMember> list = new List<RecordMember>();

            sql = "select TOP 10 * from HistoryRecord ORDER BY time DESC";
            SqlConnection mycon = new SqlConnection(ConnString);
            mycon.Open();
            SqlCommand sqlcom = new SqlCommand(sql, mycon);
            SqlDataReader sReader = sqlcom.ExecuteReader();
            while(sReader.Read())
            {
                RecordMember rm = new RecordMember();
                rm.Name=(string)sReader["fName"];
                rm.Path = (string)sReader["fPath"];
                list.Add(rm);
            }
            sReader.Close();
            sqlcom.Dispose();
            mycon.Close();

            return list;
        }
    }
}
