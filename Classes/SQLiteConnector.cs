using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MpFree4k.Classes
{
    public class SQLiteConnector
    {
        SQLiteConnection sqlcon = null;
        public void init()
        {
            string progpath = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
            string dbpath = Path.Combine(progpath, "play.db");

            if (!File.Exists(dbpath))
            {
                SQLiteConnection.CreateFile(dbpath);
            }

            sqlcon = new SQLiteConnection("Data Source='" + dbpath + "'; Version=3;");
            sqlcon.Open();

            create();

            GetTables();
        }

        void create()
        {
            string sql = @"CREATE TABLE IF NOT EXISTS `AlbumTracks` (
	'id'	    INTEGER PRIMARY KEY AUTOINCREMENT UNIQUE,
	'albumid'	INTEGER NOT NULL,
	'path'	TEXT NOT NULL
);";
            SQLiteCommand command = new SQLiteCommand(sql, sqlcon);
            command.ExecuteNonQuery();


            sql = @"CREATE TABLE IF NOT EXISTS `RecentAlbums` (
	'id' 	INTEGER PRIMARY KEY AUTOINCREMENT UNIQUE,
	'name'	TEXT NOT NULL,
	'year'	TEXT NOT NULL,
	'playcount'	INTEGER NOT NULL,
	'playdate'	DATE NOT NULL
);";
            command = new SQLiteCommand(sql, sqlcon);
            command.ExecuteNonQuery();

            sql = @"CREATE TABLE IF NOT EXISTS `RecentTracks` (
	'id'	    INTEGER PRIMARY KEY AUTOINCREMENT UNIQUE,
	'path'	TEXT NOT NULL,
	'playcount'	INTEGER NOT NULL,
	'playdate'	DATE NOT NULL
);";
            command = new SQLiteCommand(sql, sqlcon);
            command.ExecuteNonQuery();
        }

        private const string sql_get_track = "SELECT COUNT(id) as count, id FROM RecentTracks WHERE path = '{0}'";
        private const string sql_insert_track = "INSERT INTO RecentTracks (path, playcount, playdate) VALUES ('{0}',1,'{1}')";
        private const string sql_update_track = "UPDATE RecentTracks SET playdate = '{1}', playcount = playcount + 1 WHERE id = {0}";

        private const string sql_get_album = "SELECT COUNT(id) as count, id FROM RecentAlbums where name = '{0}' and year = '{1}'";
        private const string sql_insert_album = "INSERT INTO RecentAlbums (name, year, playcount, playdate) VALUES ('{0}', '{1}', 1, '{2}')";
        private const string sql_update_album = "Update RecentAlbums SET playdate = '{1}', playcount = playcount + 1 WHERE id = {0}";

        private const string sql_insert_album_track = "INSERT INTO AlbumTracks (albumid, path) VALUES ({0}, '{1}')";
        private const string sql_get_album_track = "SELECT id FROM AlbumTracks WHERE albumid = '{0}' AND path = '{1}' LIMIT 1";
        private const string sql_get_album_trackcount = "SELECT COUNT(id) as count FROM AlbumTracks WHERE Albumid = '{0}'";

        private const string sql_get_tables = "SELECT name FROM sqlite_master WHERE type = 'table'";

        private const string sql_get_recent_albums = "SELECT * FROM (SELECT id, name, year, playcount FROM  RecentAlbums WHERE name <> '' ORDER BY playdate DESC LIMIT 400) ORDER BY playcount DESC LIMIT {0}";
        private const string sql_get_recent_tracks = "SELECT * FROM (SELECT path, playcount from RecentTracks ORDER BY playdate DESC LIMIT 10000) ORDER BY playcount DESC LIMIT {0}";
        private const string sql_get_recent_tracks_detail = "SELECT * FROM (SELECT path, playcount, playdate from RecentTracks ORDER BY playdate DESC LIMIT 10000) ORDER BY playcount DESC LIMIT {0}";
        private const string sql_get_album_tracks = "SELECT path from AlbumTracks WHERE albumid = '{0}'";

        private const string sql_remove_album = "DELETE FROM RecentALbums WHERE id = '{0}'";
        private const string sql_remove_albumtracks = "DELETE FROM AlbumTracks WHERE albumid = '{0}'";

        private string get_sql_date(DateTime dt)
        {
            return dt.ToString("yyyy-MM-dd HH:mm:ss");
        }

        private string sanitize(string str)
        {
            str = str.Replace("'", "{!x%99}");
            return str;
        }

        private string desanitize(string str)
        {
            str = str.Replace("{!x%99}", "'");
            return str;
        }

        private void GetTables()
        {
            string sql = String.Format(sql_get_tables);
            SQLiteCommand command = new SQLiteCommand(sql, sqlcon);

            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    reader.ToString();
                }
            }
        }

        private int GetAlbum(string name, string year)
        {
            string sql = String.Format(sql_get_album, name, year);
            SQLiteCommand command = new SQLiteCommand(sql, sqlcon);

            using (SQLiteDataReader reader = command.ExecuteReader())
            {

                if (reader.StepCount < 1 || !reader.HasRows)
                    return -1;

                reader.Read();
                int count = 0;
                int.TryParse(reader["count"].ToString(), out count);

                if (count == 0)
                    return -1;
                else
                {
                    int id = 0;
                    int.TryParse(reader["id"].ToString(), out id);
                    return id;
                }
            }
        }

        public int GetAlbumTrackCount(int id)
        {
            string sql = String.Format(sql_get_album_trackcount, id);
            SQLiteCommand command = new SQLiteCommand(sql, sqlcon);

            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                int count = 0;
                if (int.TryParse(reader["count"].ToString(), out count))
                    return count;
                else
                    return 0;
            }
        }

        private int GetTrack(string path)
        {
            string sql = String.Format(sql_get_track, sanitize(path));
            SQLiteCommand command = new SQLiteCommand(sql, sqlcon);

            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                reader.Read();

                if (reader.StepCount < 1 || !reader.HasRows)
                    return -1;

                int count = 0;
                int.TryParse(reader["count"].ToString(), out count);

                if (count == 0)
                    return -1;
                else
                {
                    int id = 0;
                    int.TryParse(reader["id"].ToString(), out id);
                    return id;
                }
            }
        }

        private string getDate()
        {
            return get_sql_date(DateTime.Now);
        }

        private void InsertTrack(string path)
        {
            string sql = String.Format(sql_insert_track, sanitize(path), getDate());
            SQLiteCommand command = new SQLiteCommand(sql, sqlcon);
            command.ExecuteNonQuery();
        }

        private void InsertAlbum(string name, string year)
        {
            string sql = String.Format(sql_insert_album, name, year, getDate());
            SQLiteCommand command = new SQLiteCommand(sql, sqlcon);
            command.ExecuteNonQuery();
        }

        private void UpdateTrack(int id, string path)
        {
            string sql = String.Format(sql_update_track, id.ToString(), getDate());
            SQLiteCommand command = new SQLiteCommand(sql, sqlcon);
            command.ExecuteNonQuery();
        }

        private void UpdateAlbum(int id)
        {
            string sql = String.Format(sql_update_album, id.ToString(), getDate());
            SQLiteCommand command = new SQLiteCommand(sql, sqlcon);
            command.ExecuteNonQuery();
        }

        private bool GetAlbumTrack(int albumid, string track)
        {
            string sql = String.Format(sql_get_album_track, albumid, sanitize(track));
            SQLiteCommand command = new SQLiteCommand(sql, sqlcon);

            SQLiteDataReader reader = command.ExecuteReader();
            reader.Read();

            if (reader.StepCount == 0 || !reader.HasRows)
                return false;

            int id = 0;
            int.TryParse(reader["id"].ToString(), out id);
            return id >= 0;
        }

        private void InsertAlbumTrack(int id, string track)
        {
            string sql = String.Format(sql_insert_album_track, id.ToString(), sanitize(track));
            SQLiteCommand command = new SQLiteCommand(sql, sqlcon);
            command.ExecuteNonQuery();
        }

        public void SetTrack(string path)
        {
            int id = GetTrack(path);
            if (id < 0)
                InsertTrack(path);
            else
                UpdateTrack(id, path);
        }

        public List<Tuple<string, int, DateTime>> GetRecentTracksDetails(int num)
        {
            string sql = String.Format(sql_get_recent_tracks_detail, num);
            List<Tuple<string, int, DateTime>> tracks = new List<Tuple<string, int, DateTime>>();

            SQLiteCommand command = new SQLiteCommand(sql, sqlcon);
            int playcount = 0;
            string path = "";
            string str_date = "";
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {

                    if (reader.StepCount < 1 || !reader.HasRows)
                        return tracks;

                    path = desanitize(reader["path"].ToString());
                    if (!int.TryParse(reader["playcount"].ToString(), out playcount))
                        playcount = 1;
                    str_date = reader["playdate"].ToString();

                    DateTime myDate = DateTime.ParseExact(str_date, "dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

                    Tuple<string, int, DateTime> t = new Tuple<string, int, DateTime>(path, playcount, myDate);

                    tracks.Add(t);
                }
            }
            return tracks;
        }

        public List<string> GetRecentTracks(int num)
        {
            string sql = String.Format(sql_get_recent_tracks, num);
            List<string> tracks = new List<string>();

            SQLiteCommand command = new SQLiteCommand(sql, sqlcon);

            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {

                    if (reader.StepCount < 1 || !reader.HasRows)
                        return tracks;

                    tracks.Add(desanitize(reader["path"].ToString()));
                }
            }
            return tracks;
        }

        public List<Tuple<string, string, int>> GetRecentAlbums(int num)
        {

            string sql = String.Format(sql_get_recent_albums, num);
            List<Tuple<string, string, int>> albums = new List<Tuple<string, string, int>>();
            SQLiteCommand command = new SQLiteCommand(sql, sqlcon);

            //MainWindow.SetProgress(0);
            //int step = 100 / UserConfig.NumberRecentAlbums;
            //int _count = 0;

            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    //_count++;
                    //MainWindow.SetProgress(step * _count);
                    if (reader.StepCount < 1 || !reader.HasRows)
                        return albums;

                    int id = 0;
                    if (!int.TryParse(reader["id"].ToString(), out id) || reader["name"].ToString() == "")
                        continue;

                    albums.Add(new Tuple<string, string, int>(reader["name"].ToString(), reader["year"].ToString(), id));

                }
            }

            //MainWindow.SetProgress(0);
            return albums;
        }

        public string[] GetAlbumTracks(int id)
        {
            List<string> t = new List<string>();
            string sql = String.Format(sql_get_album_tracks, id);

            SQLiteCommand command = new SQLiteCommand(sql, sqlcon);

            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (reader.StepCount < 1 || !reader.HasRows)
                        return t.ToArray();

                    t.Add(desanitize(reader["path"].ToString()));
                }
            }

            return t.Distinct().ToArray();
        }


        public void SetAlbum(string _name, string year, string[] tracks)
        {
            string name = sanitize(_name);

            int id = GetAlbum(name, year);
            if (id < 0)
            {
                InsertAlbum(name, year);
                id = GetAlbum(name, year);
                if (id < 0)
                    return;
            }
            else
            {
                UpdateAlbum(id);
            }

            foreach (string track in tracks)
            {
                string t = sanitize(track);
                if (!GetAlbumTrack(id, t))
                    InsertAlbumTrack(id, t);
            }
        }

        public void RemoveAlbum(int id)
        {
            string sql_albums = String.Format(sql_remove_album, id);
            string sql_tracks = String.Format(sql_remove_albumtracks, id);
            SQLiteCommand command = new SQLiteCommand(sql_albums, sqlcon);
            command.ExecuteNonQuery();
            command = new SQLiteCommand(sql_tracks, sqlcon);
            command.ExecuteNonQuery();
        }
    }
}
