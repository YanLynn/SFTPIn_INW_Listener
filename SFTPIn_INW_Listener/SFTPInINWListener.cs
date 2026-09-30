using SFTPIn_INW_Listener;
using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceProcess;
using System.Threading;

namespace SFTPIn_INW_Listener
{
    public partial class SFTPInINWListener : ServiceBase
    {
        private ManualResetEvent _stopEvent;
        private Thread _worker;
        private ConfigData _cfg;
        private SftpClient _sftp;

        public SFTPInINWListener()
        {
            InitializeComponent();
        }

        public void DebugStart() { OnStart(null); }
        public void DebugStop() { OnStop(); }

        protected override void OnStart(string[] args)
        {
            try
            {
                _cfg = ConfigData.Load();
                _sftp = new SftpClient(_cfg);
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("Startup failed", ex);
                throw;
            }

            _stopEvent = new ManualResetEvent(false);
            _worker = new Thread(RunLoop) { IsBackground = true };
            _worker.Start();
            Logger.Instance.Info("SFTPIn_INW_Listener started");
        }

        protected override void OnStop()
        {
            _stopEvent.Set();
            _worker.Join(30000);
            Logger.Instance.Info("SFTPIn_INW_Listener stopped");
        }

        private void RunLoop()
        {
            do
            {
                try { ProcessOnce(); }
                catch (Exception ex) { Logger.Instance.Error("Loop error", ex); }
            }
            while (!_stopEvent.WaitOne(_cfg.SleepMs));
        }

        private void ProcessOnce()
        {
            string today = DateTime.Now.ToString(_cfg.DateFolderFormat);
            string localDir = Path.Combine(_cfg.LocalDestRoot, today);
            Directory.CreateDirectory(localDir);

            List<string> remoteFiles = _sftp.ListRemoteFiles(_cfg.RemoteDir);
            if (remoteFiles.Count == 0) return;

            var remoteSet = new HashSet<string>(remoteFiles, StringComparer.OrdinalIgnoreCase);

            foreach (string fileName in remoteFiles)
            {
                if (_stopEvent.WaitOne(0)) return;

                // Inward covers all transaction types (IWNM, IREM, IEVM, OEVM, INRM) as one
                // *.ZIP.PGP pattern — the SGL is what marks a file as ready, not a status suffix.
                if (!fileName.EndsWith(".ZIP.PGP", StringComparison.OrdinalIgnoreCase)) continue;

                string sglName = fileName + ".SGL";
                if (!remoteSet.Contains(sglName))
                {
                    Logger.Instance.Debug("Waiting for SGL: " + fileName);
                    continue;
                }

                if (File.Exists(Path.Combine(localDir, fileName)))
                {
                    Logger.Instance.Debug("Already downloaded, skipping: " + fileName);
                    continue;
                }

                ProcessFile(fileName, sglName, localDir);
            }
        }

        private void ProcessFile(string pgpFileName, string sglFileName, string localDir)
        {
<<<<<<< HEAD:SFTPIn_Resp_Listener/SFTPInRespListener.cs
            if (sglFileNameOrNull == null)
            {
                // SUCC has no SGL — wait so the bank has finished writing the .PGP before we pull it
                Thread.Sleep(_cfg.HoldBeforeSuccDownloadMs);
            }

=======
            // No hold delay here: unlike SUCC in the response flows, the SGL itself is the
            // "file is complete" signal, so there is nothing to wait for once it appears.
>>>>>>> 5d4a57e (update and bug fix):SFTPIn_INW_Listener/SFTPInINWListener.cs
            if (!_sftp.Download(_cfg.RemoteDir, pgpFileName, localDir))
            {
                Logger.Instance.Error("PGP download failed, will retry next loop: " + pgpFileName);
                return;
            }
            Logger.Instance.Info("Downloaded: " + pgpFileName);

<<<<<<< HEAD:SFTPIn_Resp_Listener/SFTPInRespListener.cs
            if (sglFileNameOrNull != null)
            {
                if (!_sftp.Download(_cfg.RemoteDir, sglFileNameOrNull, localDir))
                {
                    Logger.Instance.Error("SGL download failed: " + sglFileNameOrNull);
                    return;
                }
                Logger.Instance.Info("Downloaded: " + sglFileNameOrNull);
=======
            if (!_sftp.Download(_cfg.RemoteDir, sglFileName, localDir))
            {
                Logger.Instance.Error("SGL download failed: " + sglFileName);
                return;
>>>>>>> 5d4a57e (update and bug fix):SFTPIn_INW_Listener/SFTPInINWListener.cs
            }
            Logger.Instance.Info("Downloaded: " + sglFileName);

            CleanupRemote(pgpFileName);
            CleanupRemote(sglFileName);
        }

        private void CleanupRemote(string fileName)
        {
            bool ok = _cfg.DeleteAfterDownload
                ? _sftp.DeleteRemote(_cfg.RemoteDir, fileName)
                : _sftp.MoveToProcessed(_cfg.RemoteDir, fileName, _cfg.RemoteProcessedDir);

            if (!ok)
                Logger.Instance.Warn("Remote cleanup failed (file stays in place, may reprocess): " + fileName);
        }
    }
}