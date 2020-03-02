/*
 * 由SharpDevelop创建。
 * 用户： Administrator
 * 日期: 2020/2/28
 * 时间: 6:55
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
using Socket;
using System.Text.RegularExpressions;
using System.IO;
namespace Socket
{
	/// <summary>
	/// Description of Chatengine.
	/// </summary>
	public class statesaver
	{
		///<summary>
		/// arg:9--is used to get value-----arg 0:can't socket-----arg 1:can socket
		/// 
		///</summary>
		private int state=1;
		public int set(int arg)
		{
			
			lock(this)
			{
				if(arg==9)
				{
					return state;
					
				}
				state=arg;
				return state;
			}
		}
	}
	
	
	
	public class stringclass
	{
		private  string inputstr=String.Empty;
		public string set(string str)
		{
			lock(this)
			{
				if(str==@"@#")
				{
					return inputstr;
				}
				inputstr=str;
				return inputstr;
				
			}
		}
	}
	
	
	
	public class Chatengine
	{
		Queue recvmsg=new Queue();
		Queue sentmsg=new Queue();
		
		const string filesendkeyword="%%sf";
		string inputstr=String.Empty;
		
		int socketfreq=200;
		
		
		
		
		
		public void chatlaunch(System.Net.Sockets.Socket socket)
		{
			statesaver stdsav=new statesaver();
			stringclass str=new stringclass();
			Thread showth =new  Thread(() => show(str,socket,stdsav));
			showth.Start();
			Thread inputth =new  Thread(() => input(str,socket,stdsav));
			inputth.Start();
			Thread sentth =new  Thread(() => sent(socket,stdsav));
			sentth.Start();
			Thread recvth =new  Thread(() => recv(socket,stdsav));
			recvth.Start();
		}
		
		
		
		void input(stringclass str,System.Net.Sockets.Socket socket,statesaver stdsav)
		{
			Console.Write("Edit   => ");
			while(true)
			{
				char inputchar=Console.ReadKey().KeyChar;
				if(inputchar!='\r')
				{
					str.set( str.set(@"@#")+inputchar);
				}
				if(inputchar=='\r')
				{
			//		if(inputstr!=String.Empty)
			//		{
					if(str.set(@"@#")==filesendkeyword)
					{
						stdsav.set(0);
						Console.WriteLine("Sent   => "+"FILE");
						filesendlaunch(socket);
	//					Console.WriteLine("-------------------------------------------------set state to 1");
						stdsav.set(1);
					}
					if(str.set(@"@#")!=filesendkeyword)
					{
						
						Console.WriteLine("Sent   => "+inputstr);
					}
					sentmsg.Enqueue((object)str.set(@"@#"));
					
					
					
			//		}
			//		if(inputstr==String.Empty)
			//		{
			//			Console.WriteLine("--空内容--\n");
			//		}
					
					str.set(String.Empty);
					Console.Write("Edit   => ");
				}
				
			}
			
		}
		void show(stringclass str,System.Net.Sockets.Socket socket,statesaver stdsav)
		{
			string recvstr=String.Empty;
			while(true)
			{
				Thread.Sleep(200);
			
				if(recvmsg.Count>=1)
				{
					recvstr= recvmsg.Dequeue() as string;
					
					
					int forn;
					for(forn=0;forn<=(str.set(@"@#").Length+11);forn++)
					{
						Console.Write('\u0008');
					}
					for(forn=0;forn<=(str.set(@"@#").Length+11);forn++)
					{
						Console.Write(" ");
					}
					for(forn=0;forn<=(str.set(@"@#").Length+11);forn++)
					{
						Console.Write('\u0008');
					}
		//			Console.WriteLine("");
					if(recvstr==@"$%$filereq")
					{
						stdsav.set(0);
						Console.WriteLine("Recv   => "+"FILE");
						filerecvlaunch(socket);
	//					Console.WriteLine("-------------------------------------------------set state to 1");
						stdsav.set(1);
					}
					if(recvstr!=@"$%$filereq")
					{
						Console.WriteLine("Recv   => "+recvstr);
					}
					Console.Write("Edit   => "+str.set(@"@#"));
				}
			}
			
		}
		void sent(System.Net.Sockets.Socket socket,statesaver stdsav)
		{
			string sentstr=String.Empty;
			byte[] buffer=new byte[1024*1024];
			while(true)
			{
				Thread.Sleep(socketfreq);
				if(sentmsg.Count >=1 && stdsav.set(9)==1)
				{
					sentstr=sentmsg.Dequeue() as string;
					buffer = Encoding.UTF8.GetBytes(sentstr);
					try{
					socket.Send(buffer);
					}
					catch(Exception)
					{
						Writered("\n\nConnection Failed");
						while(true){}
					}
				}
			}
		}
		void recv(System.Net.Sockets.Socket socket,statesaver stdsav)
		{
			string recvstr=String.Empty;
			byte[] buffer=new byte[1024*1024];
			int n=1;
			int working=1;
			while(working==1)
			{
				
				Thread.Sleep(socketfreq);
				while(stdsav.set(9)==0){Thread.Sleep(200);}
				try{
					if(stdsav.set(9)==1)
					{
						//---------------------------------------------------------------------------------------问题等待解决：此receive抢包问题：进去时还是1，抢包时是0
						//---------------------------------------------------------------------------------------目前代替方案：$%$recv/accpet包将会发送两次,
						//----------------------------------------------------------------------------------------------------下18行左右有内码包过滤代码，如解决，需删除
						n = socket.Receive(buffer);
			//			Console.WriteLine("-----Received    with code "+stdsav.set(9).ToString());
					}
				
				}
				catch(Exception)
				{
					Writered("\n\nConnection Failed");
					while(true){}
	
					
					working=0;
				}
                recvstr = Encoding.UTF8.GetString(buffer, 0, n);
                if(recvstr!=@"$%$accepted"&&recvstr!=@"$%$refused")
                {
                	recvmsg.Enqueue((object)recvstr);
                }
                
				
				
			}
		}
		
		
		
		void filesendlaunch(System.Net.Sockets.Socket socket)
		{
			
			
			string reqstr=String.Empty;
			string accstr=String.Empty;
			string sendpath=String.Empty;
			
			byte[] reqbuf=new byte[1024];
			byte[] accbuf=new byte[1024];
			
			int done=0;
			
			reqstr=@"$%$filereq";
			reqbuf=Encoding.UTF8.GetBytes(reqstr);
			socket.Send(reqbuf,reqbuf.Length,SocketFlags.None);
			Console.Write("\n等待对方接受请求");
			Thread wat=new Thread(waitingth);
			wat.Start();
			int n=socket.Receive(accbuf,1024,SocketFlags.None);
			wat.Abort();
			accstr=Encoding.UTF8.GetString(accbuf,0,n);
			if(accstr==@"$%$accepted")
			{
				Console.Write("\n--对方已接受  输入文件路径--\n" +
				       		  "    （输入相对PATH转义为程序所在文件夹）\n" +
				       		  "    （输入 a.txt 将会在程序所在文件夹下寻找该文件）\n");
				while(true)
				{
					Console.Write("FILEPATH => ");
					sendpath= Console.ReadLine();
					if(!File.Exists(sendpath))
					{
						Writered("\n未找到文件\n");
					}
					if(File.Exists(sendpath))
					{
						break;
					}
				}
				Writeyel("\nPrepareing...\n");
				FileTrans ft=new FileTrans();
				ft.filesend(sendpath,socket);
				return;
					
			}
			if(accstr!=@"$%$refused")
			{
				Writered("\n--对方拒绝了传输请求--\n");
				return;
			}
		}
		
		
		void filerecvlaunch(System.Net.Sockets.Socket socket)
		{
			Console.WriteLine("对方请求发送文件  是否接受 (y/n)");
			
			char inputchar;
			
			string accstr=String.Empty;
			
			byte[] accbuf=new byte[1024];
			
			while(true)
			{
				inputchar=Console.ReadKey().KeyChar;
				Console.Write('\u0008');
				if(inputchar=='y'||inputchar=='n')
				{
					break;
				}
				
			}
			if(inputchar=='n')
			{
				Writeyel("\n--已拒绝--\n");
				accstr=@"$%$refused";
				accbuf=Encoding.UTF8.GetBytes(accstr);
				socket.Send(accbuf);
				socket.Send(accbuf);
				return;
			}
			if(inputchar=='y')
			{
				Writeyel("\n--已接受--\n");
				accstr=@"$%$accepted";
				accbuf=Encoding.UTF8.GetBytes(accstr);
				socket.Send(accbuf);
				socket.Send(accbuf);
				Writeyel("\nPrepareing...\n");
				FileTrans ft=new FileTrans();
				ft.filerecv("",socket);
				return;
			}
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
		void showproc(string headstr,float proc)
		{
			
			Console.Write("\r");
			Console.Write(headstr);
			for (int i=1;i<=(10-proc.ToString().Length);i++)
			{
				Console.Write(" ");
			}
			Console.Write(proc.ToString()+"%");
			Console.Write(" ]");
			
	/*		for (int i=1;i<=(10-proc.ToString().Length);i++)
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
			
			
			
			
			Console.Write(" ");
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
