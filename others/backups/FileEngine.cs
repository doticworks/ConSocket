#region
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
#endregion
namespace OtherTest {
    /// <summary>
    ///     Socket.FileEngine
    ///     RecvFile => FileRecv()
    ///     SendFile => FileSend()
    /// </summary>
    public static class FileEngine {
        public const int PackageSize = 1024;
        public const int ProcWhen = 1000;
        public static void FileSend(string iPath, Socket iSocket) {
            long sendSeek = 0;
            var buffer = new byte[PackageSize];
            byte[] nameBuf;
            var recvBuf = new byte[1024];
            var fileStream = new FileStream(iPath, FileMode.Open);
            var fileLength = fileStream.Length; //var fileLength = new FileInfo(iPath).Length;
            var fileName = Path.GetFileName(iPath);
            Console.WriteLine($"\nFile   => {fileName}\nLength => {fileLength}\n");
            WriteColor("Starting", ConsoleColor.Yellow);
            while (true) {
                var nameStr = $@"{fileName}@#@{fileLength}@#@";
                nameBuf = Encoding.UTF8.GetBytes(nameStr);
                iSocket.Send(nameBuf, nameBuf.Length, SocketFlags.None);
                Console.Write("\n等待对方确认 ");
                var wait = new Thread(Waiting);
                wait.Start();
                try {
                    var len = iSocket.Receive(recvBuf);
                    wait.Abort();
                    Console.Write("\n\n");
                    if (len == nameBuf.Length && nameBuf.SequenceEqual(recvBuf)) break;
                }
                catch {
                    wait.Abort();
                    WriteColor("\n\nConnection Failed\n\n", ConsoleColor.Red);
                    Console.ReadKey(false);
                }
            }
            nameBuf = Encoding.UTF8.GetBytes("Start");
            iSocket.Send(nameBuf, nameBuf.Length, SocketFlags.None);
            Console.Write("Process.send -> ");
            try {
                var procN = 0;
                while (sendSeek < fileLength) {
                    var readLen = fileStream.Read(buffer, 0, PackageSize);
                    iSocket.Send(buffer, readLen, SocketFlags.None);
                    sendSeek += readLen;
                    procN++;
                    if (procN < ProcWhen) continue;
                    procN = 0;
                    ShowProc((float) sendSeek / fileLength);
                }
            }
            catch {
                WriteColor("\n\nConnection Failed\n\n", ConsoleColor.Red);
                Console.ReadKey(false);
            }
            WriteColor("---Send done---", ConsoleColor.Yellow);
        }
        public static void FileRecv(string iPath, Socket iSocket) {
            long recvSeek = 0;
            var buffer = new byte[PackageSize];
            var nameBuf = new byte[1024];
            var filename = string.Empty;
            Console.Write("\n等待对方发送");
            var wat = new Thread(Waiting);
            wat.Start();
            while (true)
                try {
                    var len = iSocket.Receive(nameBuf);
                    wat.Abort();
                    var nameStr = Encoding.UTF8.GetString(nameBuf, 0, len);
                    var nameStack = nameStr.Split(@"@#@".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (nameStack[0] == "Start") break;
                    long fileLength = int.Parse(nameStack[1]);
                    Console.WriteLine($"\nFile   => {nameStack[0]}\nLength => {fileLength}\n");
                    var fileStream = new FileStream($@"{Environment.CurrentDirectory}\{nameStack[0]}", FileMode.Create, FileAccess.Write);
                    nameStr = $@"{filename}@#@{fileLength}@#@";
                    nameBuf = Encoding.UTF8.GetBytes(nameStr);
                    iSocket.Send(nameBuf, nameBuf.Length, SocketFlags.None);
                    Console.Write("Process.recv -> ");
                    var procN = 0;
                    while (recvSeek < fileLength) {
                        var pakLen = iSocket.Receive(buffer, PackageSize, SocketFlags.None);
                        fileStream.Position = fileStream.Length;
                        fileStream.Write(buffer, 0, pakLen);
                        recvSeek += pakLen;
                        procN++;
                        if (procN < ProcWhen) continue;
                        procN = 0;
                        ShowProc((float) recvSeek / fileLength);
                    }
                    WriteColor("---recv done---", ConsoleColor.Yellow);
                }
                catch {
                    wat.Abort();
                    WriteColor("\n\nConnection Failed\n\n", ConsoleColor.Red);
                    Console.ReadKey(false);
                }
        }
        public static void Waiting() {
            while (true) {
                for (var i = 6; i-- > 0;) {
                    Console.Write(".");
                    Thread.Sleep(200);
                }
                foreach (var c in "\b \b") Console.Write(new string(c, 6));
            }
        }
        public static void WriteColor(string iStr, ConsoleColor iConsoleColor) {
            Console.ForegroundColor = iConsoleColor;
            Console.WriteLine(iStr);
            Console.ForegroundColor = ConsoleColor.White;
        }
        public static void ShowProc(float iProc) {
            var str = iProc.ToString(CultureInfo.CurrentCulture);
            Console.Write(iProc);
            Console.SetCursorPosition(Console.CursorLeft - str.Length, Console.CursorTop);
        }
    }
}