using System.Configuration;

namespace SFTPIn_INW_Listener
{
    internal sealed class ConfigData
    {
        public int SleepMs { get; private set; }

        public string SftpProgram { get; private set; }
        public string SftpHost { get; private set; }
        public string SftpPort { get; private set; }
        public string SftpUser { get; private set; }
        public string SftpKey { get; private set; }

        public string RemoteDir { get; private set; }
        public string RemoteProcessedDir { get; private set; }
        public bool DeleteAfterDownload { get; private set; }

        public string LocalDestRoot { get; private set; }
        public string DateFolderFormat { get; private set; }
        public int HoldBeforeSuccDownloadMs { get; private set; }

        public static ConfigData Load()
        {
            return new ConfigData
            {
                SleepMs = GetInt("SLEEP_MS", 3000),
                SftpProgram = Get("SFTP_PROGRAM"),
                SftpHost = Get("SFTP_HOST"),
                SftpPort = Get("SFTP_PORT"),
                SftpUser = Get("SFTP_USER"),
                SftpKey = Get("SFTP_KEY"),
                RemoteDir = Get("REMOTE_DIR"),
                RemoteProcessedDir = Get("REMOTE_PROCESSED_DIR"),
                DeleteAfterDownload = GetBool("DELETE_AFTER_DOWNLOAD", false),
                LocalDestRoot = Get("LOCAL_DEST_ROOT"),
                DateFolderFormat = Get("DATE_FOLDER_FORMAT"),
                HoldBeforeSuccDownloadMs = GetInt("HOLD_BEFORE_SUCC_DOWNLOAD_MS", 3000),
            };
        }

        private static string Get(string key)
        {
            string v = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrWhiteSpace(v))
                throw new ConfigurationErrorsException("Missing appSetting: " + key);
            return v.Trim();
        }

        private static int GetInt(string key, int def)
        {
            int n;
            return int.TryParse(ConfigurationManager.AppSettings[key], out n) ? n : def;
        }

        private static bool GetBool(string key, bool def)
        {
            bool b;
            return bool.TryParse(ConfigurationManager.AppSettings[key], out b) ? b : def;
        }
    }
}