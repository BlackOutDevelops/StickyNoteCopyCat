using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteTaker
{
    internal class SQLiteDatabaseAccess
    {
        public static List<NoteCardModel> LoadNotes()
        {
            using (IDbConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = conn.Query<NoteCardModel>("SELECT * from NoteCards", new DynamicParameters());
                return output.ToList();
            }
        }

        public static int LoadRecentNoteDatabaseID()
        {
            using (IDbConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = conn.QuerySingle("SELECT seq FROM sqlite_sequence", new DynamicParameters());
                return (int)output.seq;
            }
        }

        public static void SaveCurrentNote(NoteCardModel note)
        {
            using (IDbConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Execute("UPDATE NoteCards SET Note = @Note, UpdatedTime = @UpdatedTime WHERE Id=@Id;", note);
            }
        }

        public static void SaveNewNote(NoteCardModel note)
        {
            using (IDbConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Execute("INSERT INTO NoteCards (Note, UpdatedTime) VALUES (@Note, @UpdatedTime)", note);
            }
        }

        public static void SaveImageToNewNote(NoteCardModel note)
        {
            using (IDbConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Execute("INSERT INTO NoteCards (ImagePaths, UpdatedTime) VALUES (@ImagePaths, @UpdatedTime)", note);
            }
        }

        public static void SaveImageToCurrentNote(NoteCardModel note)
        {
            using (IDbConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Execute("UPDATE NoteCards SET ImagePaths = @ImagePaths, UpdatedTime = @UpdatedTime WHERE Id=@Id;", note);
            }
        }

        public static void DeleteNote(NoteCardModel note)
        {
            using (IDbConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Execute("DELETE FROM NoteCards WHERE ID=@ID", note);
            }
        }

        public static string LoadConnectionString(string id = "Default")
        {
            return ConfigurationManager.ConnectionStrings[id].ConnectionString;
        }
    }
}
