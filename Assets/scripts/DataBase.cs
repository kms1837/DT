using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;

using tupleType = System.Collections.Generic.Dictionary<string, object>;

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
        string sql;

        sql = readerSQLFile("users");
        query(sql);
        sql = readerSQLFile("skills");
        query(sql);
        sql = readerSQLFile("monsters");
        query(sql);
        sql = readerSQLFile("jobclass");
        query(sql);
        sql = readerSQLFile("places");
        query(sql);
        sql = readerSQLFile("threats");
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

    public Dictionary<int, tupleType> getList (string table) {
        Dictionary<int, Dictionary<string, object>> list =  getList(table, "");
        return list;
    }

    public Dictionary<int, tupleType> getList(string table, string parm) {
        Dictionary<int, Dictionary<string, object>> list = new Dictionary<int, tupleType>();

        try {
            IDataReader reader = readQuery(string.Format("SELECT * FROM {0} {1}", table, parm));
            Dictionary<string, object> tuple;

            while (reader.Read()) {
                tuple = getTupleNode(reader);
                list.Add((int)tuple["id"], tuple);
            }
        }
        catch (System.Exception error) {
            Debug.LogWarning(string.Format("[query error from {0}]", table));
            Debug.LogWarning(error);
            dbcmd.Dispose();
            return null;
        }

        dbcmd.Dispose();
        return list;
    }

    public tupleType getTuple (string table, int id) {
        tupleType tuple = null;

        try {
            IDataReader reader = readQuery(string.Format("SELECT * FROM {0} WHERE id = {1}", table, id));
            while (reader.Read()) {
                tuple = getTupleNode(reader);
            }
        }
        catch (System.Exception error) {
            Debug.LogWarning(string.Format("[query error from {0} {1}]", table, id));
            Debug.LogWarning(error);
            dbcmd.Dispose();
            return null;
        }

        dbcmd.Dispose();
        return tuple;
    }

    private tupleType getTupleNode(IDataReader reader) {
        tupleType tuple = new tupleType();

        for (int index = 0; index < reader.FieldCount; index++) {
            string fieldName = reader.GetName(index);
            string dataType = reader.GetDataTypeName(index);
            object data = null;

            try {
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
            catch(System.Exception error) {
                Debug.LogWarning("[" + dataType + "]");
                Debug.LogWarning(error);
            }
            
        }

        return tuple;
    }

    public void closeDB() {
        dbcmd.Dispose();
        database.Close();
        dbcmd = null;
        database = null;
    }
}