using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SFTPIn_INW_Listener
{
    // Wraps sftpg3 for the "pull" operations this listener needs: list, download, rename/delete.
    // Each operation writes a small batch script and runs sftpg3 -B against it (non-interactive,
    // same shape as the -b batchfile option in standard OpenSSH sftp clients).
    internal sealed class SftpClient
    {
        private const int TimeoutMs = 60000;
        private readonly ConfigData _cfg;

        public SftpClient(ConfigData cfg)
        {
            _cfg = cfg;
        }

        // Lists file names in the remote directory. Empty list on failure (logged, not thrown)
        // so a transient listing error does not take down the whole loop.
        public List<string> ListRemoteFiles(string remoteDir)
        {
            string script = "cd " + remoteDir + "\nls -1\nbye\n";
            string output;
            int exit = RunBatch(script, out output);

            if (exit != 0)
            {
                Logger.Instance.Error("ListRemoteFiles failed (exit " + exit + "): " + output);
                return new List<string>();
            }

            // ls -1 output is one file name per line. Strip the sftp> prompt echo and blank lines.
            var files = new List<string>();
            foreach (string raw in output.Split('\n'))
            {
                string line = raw.Trim().TrimEnd('\r');
                if (line.Length == 0) continue;
                if (line.StartsWith("sftp>") || line.StartsWith("Connected") || line.StartsWith("Changing")) continue;
                files.Add(line);
            }
            return files;
        }

        // Downloads one remote file into localDir. Returns true on success.
        public bool Download(string remoteDir, string fileName, string localDir)
        {
            string localPath = Path.Combine(localDir, fileName);
            string script = "cd " + remoteDir + "\nget \"" + fileName + "\" \"" + localPath + "\"\nbye\n";
            string output;
            int exit = RunBatch(script, out output);

            if (exit != 0 || !File.Exists(localPath))
            {
                Logger.Instance.Error("Download failed (exit " + exit + "): " + fileName + " " + output);
                return false;
            }
            return true;
        }

        // Moves a remote file into the processed subfolder (used when DeleteAfterDownload is false).
        public bool MoveToProcessed(string remoteDir, string fileName, string processedDir)
        {
            string script = "cd " + remoteDir + "\nrename \"" + fileName + "\" \"" + processedDir + "/" + fileName + "\"\nbye\n";
            string output;
            int exit = RunBatch(script, out output);

            if (exit != 0)
            {
                Logger.Instance.Error("MoveToProcessed failed (exit " + exit + "): " + fileName + " " + output);
                return false;
            }
            return true;
        }

        // Deletes a remote file (used when DeleteAfterDownload is true).******
        public bool DeleteRemote(string remoteDir, string fileName)
        {
            string script = "cd " + remoteDir + "\nrm \"" + fileName + "\"\nbye\n";
            string output;
            int exit = RunBatch(script, out output);

            if (exit != 0)
            {
                Logger.Instance.Error("DeleteRemote failed (exit " + exit + "): " + fileName + " " + output);
                return false;
            }
            return true;
        }

        private int RunBatch(string scriptContent, out string output)
        {
            string scriptPath = Path.Combine(Path.GetTempPath(), "sftpin_" + Guid.NewGuid().ToString("N") + ".txt");
            try
            {
                File.WriteAllText(scriptPath, scriptContent);

                string args = "-P " + _cfg.SftpPort +
                              " -K \"" + _cfg.SftpKey + "\"" +
                              " -B \"" + scriptPath + "\"" +
                              " " + _cfg.SftpUser + "@" + _cfg.SftpHost;

                return ProcessRunner.Run(_cfg.SftpProgram, args, TimeoutMs, out output);
            }
            finally
            {
                try { File.Delete(scriptPath); } catch { }
            }
        }
    }
}