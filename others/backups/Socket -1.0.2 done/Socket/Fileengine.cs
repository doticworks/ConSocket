/*
 * 由SharpDevelop创建。
 * 用户： Administrator
 * 日期: 2020/2/27
 * 时间: 9:31
 * 
 * 要改变这种模板请点击 工具|选项|代码编写|编辑标准头文件
 */
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.Text;
using System.Diagnostics;

using System.Text.RegularExpressions;
using System.IO;
namespace Socket
{
	/// <summary>
	/// Socket.Fileengine
	/// Recvfile => filerecv()
	/// Sendfile => filesend()
	/// </summary>
	public class Fileengine
	{
		
		//---------------------show process freq
		int procm=250;
		
		//------------------------------------------------------package size
		int packagesize=4*1024;
		public Fileengine()
		{
		}
		public void filesend(string path,System.Net.Sockets.Socket socket)
		{
			long filelength=0;
			long sendseek=0;
			
			int paklen=0;
			int readlen=0;
			int buffersize=packagesize ;
			int done=0;
			int leng=0;
			
			byte[] buffer=new byte[buffersize];
			byte[] namebuf=new byte[1024];
			byte[] recvbuf=new byte[1024];
			
			string namestr=String.Empty;
			string filename =String.Empty;
			
			FileStream fs=new FileStream(path,FileMode.Open);
			
			filelength = fs.Length;
			filename =fs.Name.Substring(fs.Name.LastIndexOf("\\") + 1);  
			
			Console.WriteLine("File   => "+filename);
			Console.WriteLine("Length => "+filelength.ToString());	
			
			Console.WriteLine("");
			Writeyel("Starting");
			
			while(done == 0)
			{
				namestr=filename+@"@#@"+filelength.ToString()+@"@#@";//--------------------sndfmat
				
				namebuf = Encoding.UTF8.GetBytes(namestr);
				socket.Send(namebuf,namebuf.Length,SocketFlags.None);
				
				Console.WriteLine("");
				Console.Write("等待对方确认 ");
				Thread wat=new Thread(waitingth);
				wat.Start();
				try{
				leng= socket.Receive(recvbuf);
				}
				catch(Exception)
				{
					wat.Abort();
					Console.WriteLine("");
					Console.WriteLine("");
					Writered("Connection Failed");
					Console.WriteLine("");
					Console.WriteLine("");
					while(true){}
				}
				wat.Abort();
				Console.WriteLine("");
				Console.WriteLine("");
	//			if(leng==namebuf.Length&&recvbuf==namebuf)
	//			{
					done=1;
	//			}
			}
			
			namebuf=Encoding.UTF8.GetBytes("start");
			socket.Send(namebuf,namebuf.Length,SocketFlags.None);
			
			Console.Write("Process.send -> [");
			
			int procn=0;
			while(sendseek<filelength)
			{
				readlen=fs.Read(buffer,0,buffersize);
				try{
				socket.Send(buffer,readlen,SocketFlags.None);
				}
				catch(Exception){
					Console.WriteLine("");
					Console.WriteLine("");
					Writered("Connection Failed");
					Console.WriteLine("");
					Console.WriteLine("");
					while(true){}
				}
				sendseek+=readlen;
				procn++;
				if(procn>=procm)
				{
					procn=0;
					showproc((float)sendseek/filelength);
				}
			}
			
			
		
			Console.WriteLine("");
			Console.WriteLine("");
			Console.Write("\a");
			Writeyel("---Send done---");
			
			
			
		
			
		} 
		public void filerecv(string path,System.Net.Sockets.Socket socket)
		{
			long filelength=0;
			long recvseek=0;
			
			int paklen=0;
			int buffersize=packagesize ;
			int done=0;
			int n=0;
			
			byte[] buffer=new byte[buffersize];
			byte[] namebuf=new byte[1024];
			byte[] recvbuf=new byte[1024];
			
			string namestr=String.Empty;
			string filename =String.Empty;
			
			FileStream fs=null;
			
			
			Console.WriteLine("");
			Console.Write("等待对方发送");
			
			Thread wat=new Thread(waitingth);
			wat.Start();
				
			done=0;
			while(done==0)
			{
				try{
				n=socket.Receive(namebuf);
				}
				catch(Exception)
				{
					wat.Abort();
					Console.WriteLine("");
					Console.WriteLine("");
					Writered("Connection Failed");
					Console.WriteLine("");
					Console.WriteLine("");
					while(true){}
				}
				wat.Abort();
				namestr= Encoding.UTF8.GetString(namebuf, 0, n);
				string[] namestack=namestr.Split(@"@#@".ToCharArray());
				if(namestack[0]=="start")
				{
					done=1;
				}
				if(done==0)
				{
					filelength=int.Parse(namestack[3]);
					Console.WriteLine("");
					Console.WriteLine("File   => "+namestack[0]);
					Console.WriteLine("Length => "+filelength.ToString());
					Console.WriteLine("");
				
					//--------------------------------如果要选储存路径 写在这里
				
					if(File.Exists(@"C:\Users\Administrator\Desktop\"+namestack[0]))
					{
						
						
						//-------------------------------------------如果确认文件重复，写在这里
						File.Delete(@"C:\Users\Administrator\Desktop\"+namestack[0]);
				    }
					fs =File.OpenWrite(@"C:\Users\Administrator\Desktop\"+namestack[0]);
				
					namestr=filename+@"@#@"+filelength.ToString()+@"@#@";//--------------------sndfmat
				
					namebuf = Encoding.UTF8.GetBytes(namestr);
					socket.Send(namebuf,namebuf.Length,SocketFlags.None);
					
				//	socket.Send(namebuf,n,SocketFlags.None);
				}
				
			}
			done=0;
			int procn=0;
			Console.Write("Process.recv -> ");
			while(recvseek<filelength)
			{
				paklen=socket.Receive(buffer,buffersize,SocketFlags.None);
				fs.Position=fs.Length;
				fs.Write(buffer,0,paklen);
				recvseek+=paklen;
				procn++;
				if(procn>=procm)
				{
					procn=0;
					showproc((float)recvseek/filelength);
				}
				
			}
			Console.WriteLine("");
			Console.WriteLine("");
			Console.Write("\a");
			Writeyel("---recv done---");
		}
		
		void waitingth()
		{
			while(true)
			{
				Console.Write(".");
				Thread.Sleep(200);
				Console.Write(".");
				Thread.Sleep(200);
				Console.Write(".");
				Thread.Sleep(200);
				Console.Write(".");
				Thread.Sleep(200);
				Console.Write(".");
				Thread.Sleep(200);
				Console.Write(".");
				Thread.Sleep(200);
				
				Console.Write('\u0008');
				Console.Write('\u0008');
				Console.Write('\u0008');
				Console.Write('\u0008');
				Console.Write('\u0008');
				Console.Write('\u0008');
				Console.Write(" ");
				Console.Write(" ");
				Console.Write(" ");
				Console.Write(" ");
				Console.Write(" ");
				Console.Write(" ");
				Console.Write('\u0008');
				Console.Write('\u0008');
				Console.Write('\u0008');
				Console.Write('\u0008');
				Console.Write('\u0008');
				Console.Write('\u0008');
				
			}
		}
		void Writeyel(string str)
		{
			Console.ForegroundColor=ConsoleColor.Yellow;
			Console.WriteLine(str);
			Console.ForegroundColor=ConsoleColor.White;
		}
		void Writered(string str)
		{
			Console.ForegroundColor=ConsoleColor.Red;
			Console.WriteLine(str);
			Console.ForegroundColor=ConsoleColor.White;
		}
		void showproc(float proc)
		{
			for (int i=1;i<=(10-proc.ToString().Length);i++)
			{
				Console.Write(" ");
			}
			Console.Write(proc.ToString()+"%");
			Console.Write(" ]");
			
			Console.Write('\u0008');
			Console.Write('\u0008');
			Console.Write('\u0008');
			Console.Write('\u0008');
			Console.Write('\u0008');
			
			Console.Write('\u0008');
			Console.Write('\u0008');
			Console.Write('\u0008');
			Console.Write('\u0008');
			Console.Write('\u0008');
			
			Console.Write('\u0008');
			Console.Write('\u0008');
			Console.Write('\u0008');
			
		/*	Console.Write(" ");
			Console.Write(" ");
			Console.Write(" ");
			Console.Write(" ");
			Console.Write(" ");
			Console.Write(" ");
			Console.Write(" ");
			Console.Write(" ");
			Console.Write('\u0008');
			Console.Write('\u0008');
			Console.Write('\u0008');
			Console.Write('\u0008');
			Console.Write('\u0008');
			Console.Write('\u0008');
			Console.Write('\u0008');
			Console.Write('\u0008');*/
			
		}
	}
}
