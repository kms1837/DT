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

    private string readerSQLFile(string fileName) {
        string filePath = string.Format("schema/{0}", fileName);
        TextAsset readFileStr = Resources.Load(filePath, typeof(TextAsset)) as TextAsset;

        return readFileStr.text;
    }

    public void initDataBase() {
        string sql = "DROP DATABASE DT";
        query(sql);
        
        sql = readerSQLFile("users");
        query(sql);
        sql = readerSQLFile("skills");
        query(sql);
        sql = readerSQLFile("monsters");
        query(sql);
        sql = readerSQLFile("sample");
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