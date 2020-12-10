using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace KEngine
{
    public class LogFileRecorder
    {
        public enum UIState
        {
            LoadAB = 0,
            LoadAsset,
            OnInit,
            OnOpen
        }
        private StreamWriter writer;
        /// <summary>
        /// 初始化记录器，在游戏退出时调用Close
        /// </summary>
        /// <param name="filePath">文件名中不能包含特殊字符比如:</param>
        /// <param name="mode"></param>
        public LogFileRecorder(string filePath, FileMode mode = FileMode.Create)
        {
            int index = 0;
            try
            {
                var fs = new FileStream(filePath, mode);
                writer = new StreamWriter(fs);
            }
            catch (IOException e)
            {
                filePath = Path.GetDirectoryName(filePath) + "/" + Path.GetFileNameWithoutExtension(filePath) + "_" + (index++) + Path.GetExtension(filePath);
				Debug.LogError(e.Message);
            }
        }

        public void WriteLine(string line)
        {
            writer.WriteLine(line);
            writer.Flush();
        }

        public void Close()
        {
            writer.Flush();
            writer.Close();
        }


        #region 记录函数执行耗时

        private static Dictionary<string, LogFileRecorder> loggers = new Dictionary<string, LogFileRecorder>();

        public static void CloseStream()
        {
            foreach (var kv in loggers)
            {
                kv.Value.Close();
            }
        }
        
        /// <summary>
        /// 保存UI的一些数据到文件中
        /// </summary>
        public static void WriteUILog(string uiName,UIState state, float time)
        {
            LogFileRecorder logger;
            var logType = "ui";
            if (!loggers.TryGetValue(logType, out logger))
            {
                logger = new LogFileRecorder(Application.persistentDataPath + $"/profiler_ui_{DateTime.Now.ToString("yyyy-M-d HH.mm.ss")}.csv");
                loggers.Add(logType, logger);
                logger.WriteLine("UI名字,操作(函数),耗时(ms)");
            }

            logger.WriteLine(string.Format("{0},{1},{2:0.###}",uiName,state,time));
        }
        
        public static void WriteLoadAbLog(string abName,float time)
        {
            LogFileRecorder logger;
            var logType = "loadab";
            if (!loggers.TryGetValue(logType, out logger))
            {
                logger = new LogFileRecorder(Application.persistentDataPath + $"/profiler_loadab_{DateTime.Now.ToString("yyyy-M-d HH.mm.ss")}.csv");
                loggers.Add(logType, logger);
                logger.WriteLine("AB资源,耗时");
            }

            logger.WriteLine(string.Format("{0},{1:0.###}",abName,time));
        }

        public static void WriteProfileLog(string logType, string line)
        {
            LogFileRecorder logger;
            if (!loggers.TryGetValue(logType, out logger))
            {
                logger = new LogFileRecorder(Application.persistentDataPath + $"/profiler_{logType}_{DateTime.Now.ToString("yyyy-M-d HH.mm.ss")}.csv");
                loggers.Add(logType, logger);
            }

            logger.WriteLine(line);
        }

        #endregion
    }
}