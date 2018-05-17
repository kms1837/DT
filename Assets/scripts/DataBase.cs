using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Data;
using Mono.Data.Sqlite;

public class DataBase {
    private IDbConnection database = null;
    private IDbCommand dbcmd;

    public DataBase(string dbName) {
        connectDB(dbName);
    }

    private void connectDB(string dbName) {
        string path = string.Format("URI=file:{0}/{1}.s3db", Application.streamingAssetsPath, dbName);

        database = (IDbConnection)new SqliteConnection(path);
        database.Open();

        dbcmd = database.CreateCommand();
    }

    public void initDataBase() {
        string sql = "DROP DATABASE DT";
        query(sql);

        sql = "create table monster (" +
                "id INTEGER NOT NULL Primary Key AUTOINCREMENT, " +
                "name VARCHAR, " +
                "description VARCHAR, " +
                "hp FLOAT, " +
                "dp FLOAT, " +
                "defore_delay FLOAT, " +
                "after_delay FLOAT, " +
                "range FLOAT, " +
                "power FLOAT, " +
                "skill1 INT, " +
                "skill2 INT, " +
                "skill3	INT, " +
                "skill4 INT" +
                ")";

        query(sql);

        query("INSERT INTO monster VALUES(1, '늑대', '늑대다', 180, 0, 0.5, 0.5, 10, 20, 0, 0, 0, 0)");
        query("INSERT INTO monster VALUES(2, '어린 늑대', '어린 늑대', 100, 0, 0.5, 0.5, 10, 10, 0, 0, 0, 0)");
        query("INSERT INTO monster VALUES(3, '어미 늑대', '어미 늑대', 300, 0, 0.5, 0.5, 10, 20, 0, 0, 0, 0)");
        query("INSERT INTO monster VALUES(4, '늑대 우두머리', '늑대 우두머리', 1000, 0, 0.5, 0.5, 10, 40, 0, 0, 0, 0)");
        query("INSERT INTO monster VALUES(5, '늑대왕', '늑대들의 왕', 1000, 0, 0.5, 0.5, 10, 60, 0, 0, 0, 0)");

        query(sql);

        sql = "create table skill (" +
            "id INTEGER NOT NULL Primary Key AUTOINCREMENT, " +
            "name VARCHAR, " +
            "description VARCHAR, " +
            "before_delay FLOAT, " +
            "after_delay FLOAT, " +
            "range FLOAT, " +
            "power FLOAT, " +
            "cool_time FLOAT, " +
            "effect VARCHAR, " +
            "script VARCHAR " +
            ")";

        query(sql);

        sql = "INSERT INTO skill VALUES(1, '회복', '대상 1초마다 10회 치유', 0.1, 0.9, 3, 10, 60, '', '')";

        query(sql);

        dbcmd.Dispose();
    }

    private void query(string queryStr) {
        try {
            dbcmd.CommandText = queryStr;
            dbcmd.ExecuteNonQuery();
        }
        catch (System.Exception err) {
            Debug.LogWarning(err);
        }
    }

    private IDataReader readQuery(string queryStr) {
        IDataReader reader = null;

        try {
            dbcmd.CommandText = queryStr;
            reader = dbcmd.ExecuteReader();
        }
        catch (System.Exception err) {
            Debug.LogWarning(err);
        }

        return reader;
    }

    public Dictionary<string, object> getTuple (string table, int id) {
        IDataReader reader = readQuery(string.Format("SELECT * FROM {0} WHERE id = {1}", table, id));
        Dictionary<string, object> tuple = new Dictionary<string, object>();

        while (reader.Read()) {
            for (int index=0; index<reader.FieldCount; index++) {
                string fieldName = reader.GetName(index);
                string dataType = reader.GetDataTypeName(index);
                object data = null;
                switch (dataType) {
                    case "INT":
                    case "INTEGER":
                        data = reader.GetInt32(index);
                        break;
                    case "VARCHAR":
                        data = reader.GetString(index);
                        break;
                    case "FLOAT":
                        data = reader.GetFloat(index);
                        break;
                }
                
                tuple.Add(fieldName, data);
            }
        }

        // issue - 빈 튜플이 올경우 처리
        return tuple;
    }

    public void closeDB() {
        dbcmd.Dispose();
        database.Close();
        dbcmd = null;
        database = null;
    }
}