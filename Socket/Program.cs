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
	
	
	class Program
	{
		Queue recvmsg;
		Queue sentmsg;
		string inputstr=String.Empty;
		
		int sentslp=100;
		int recvslp=100;
		//------------------------------------------------------package size
		int packagesize=1024;
		int hidecodesize=0;
//		byte[] hidecode=new byte[hidecodesize];
/*		byte[] hidecode  = new byte[64]{2,5,3,5,3,2,4,5,6,7,
										4,6,4,3,2,6,3,8,5,4,
										7,3,6,8,5,3,5,7,7,8,
										9,5,8,5,4,3,2,1,4,5,
										9,8,9,4,2,3,6,5,5,4,
										9,6,3,5,5,5,5,5,6,5
										3,5,2,5};*/
		//------------------------------------------------------package size
		public static void Main(string[] args)
		{
			Console.Title="ConSocket";
			Console.WindowHeight=12;
			Console.WindowWidth=80;
			
			Console.WriteLine("Hello, ConSocket!     by Tclauncher");
			
			updatelog ul = new updatelog();
			ul.showlog();
			
			Console.WriteLine("Con  => Prepareing...");
			
			
			var chat = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			var client = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			int done=0;
			int mode=0;
			
			string filepath=String.Empty;
			
			
			Chatengine ceng=new Chatengine();
			FileTrans ft=new FileTrans();
			
			
			stringclass stri=new stringclass();
			Program pro =new Program();
			IPAddress ip=IPAddress.Parse("127.0.0.1");
			IPEndPoint point=new IPEndPoint(ip,1000);
			
			pro.recvmsg=new Queue();
			pro.sentmsg=new Queue();
			
			Console.WriteLine("");
			
			Console.WriteLine("Select Mode  ===   n:new socket   c:connect socket");
			//------------------------------------mode 1---------mode 2
		//	Console.WriteLine("                   s:send file    r:receive file  ");
			//------------------------------------mode 3---------mode 4
			
			
			
			//----------------------------------Wizard
			
			
			if(args.Length>=1)
			{
				Console.WriteLine("");
				Console.WriteLine("File  => "+args[0]+"  ||-- Loaded");
			}
			
			
			
			
			while(done==0)
			{
				char key=Console.ReadKey().KeyChar;
				Console.Write('\u0008');
				if(key=='n')
				{
					done=1;
					mode=1;
				}
				if(key=='c')
				{
					done=1;
					mode=2;
				}
		/*		if(key=='s')
				{
					done=1;
					mode=3;
				}
				if(key=='r')
				{
					done=1;
					mode=4;
				}
				*/
			}
			done=0;
			
			Console.WriteLine("");
			Console.WriteLine("");
			Console.WriteLine("---Write 'l' in IP 获取本机localhostIP（用于本机测试）");
			Console.WriteLine("---Write 'p' in IP 获取本机内网IP（同一个路由器下的主机）");
			Console.WriteLine("---Write 'w' in IP 获取本机外网IP（不同路由器下的主机）");
			Console.WriteLine("---Write 'd' in IP 连接默认主机");
			
			if(mode==1||mode==3)
			{
				while(done==0)
				{
					Console.Write("MYIP   =>");
		  			string ipstr=Console.ReadLine();
		  			if(ipstr=="l")	
			  		{
			  			ipstr="127.0.0.1";
			  		}
		  			if(ipstr=="d")	
			  		{
			  			ipstr="103.46.128.43";
			  		}
		  			if(ipstr=="p")
		  			{
		  				IPAddress[] list=Dns.GetHostAddresses(Dns.GetHostName());
		  				ipstr=list[1].ToString();
		  				
		  			}
		  			if(ipstr=="w")
		  			{
		  		/*		 try
           				{
           					WebClient clientg = new WebClient();
          			    	clientg.Encoding = System.Text.Encoding.Default;
                			string response = clientg.UploadString("http://iframe.ip138.com/ipcity.asp", "");
             				Match mc = Regex.Match(response, @"location.href=""(.*)""");
            			    if (mc.Success && mc.Groups.Count > 1)
           				    {
                		    response = clientg.UploadString(mc.Groups[1].Value, "");
              			    string[] str1 = response.Split('[');
             			    response = str1[1];
            		        string [] str = response.Split(']');
            		        response = str[0];
            		        Console.Write(response);
           	 	    		}
           				}
           				catch (System.Exception e)
           				{
           					pro.Writeyel("---获取本机外网ip失败 请手动填写（百度搜“本机ip”就有）");
           				}*/
						pro.Writeyel("---获取本机外网ip失败 请手动填写（百度搜“本机ip”就有）");
		  			}
			  		done=1;
			  		try{
			  			ip=IPAddress.Parse(ipstr);
			  		}
			  		catch(Exception)
			  		{
			  			done=0;
			  		}
				}
				done=0;
				
				
				while(done==0)
				{
					Console.Write("MYPORT =>");
			  		string portstr=Console.ReadLine();
			  		done=1;
			  		try{
			  			point=new IPEndPoint(ip,int.Parse(portstr));
			  		}
			  		catch(Exception)
			  		{
			  			done=0;
			  		}
				}
				done=0;
				Thread watx=new Thread(pro.waitingth);
				int ifturnmode=0;
				try{
				client.Bind(point);
				}
				catch(Exception)
				{
					
					
					Console.WriteLine("");
					pro.Writeyel("IP has been used   ===   Connect Socket with IP? (y/n)");
					char input=Console.ReadKey().KeyChar;
					if(input!='y')
					{
						return;
					}
					if(input=='y')
					{
						Console.WriteLine("");
						Console.Write("Connecting");
			
			
						ifturnmode=1;
						watx.Start();
						client.Connect(point);
						watx.Abort();
						Console.WriteLine("");
						Console.WriteLine("");
						chat=client;
					}
				}
				
				if(ifturnmode==0)
				{
					client.Listen(10);
				
					Console.WriteLine("");
					Console.Write("IP  "+point.ToString()+"  Waiting Connect ");
					
					
					Thread wat=new Thread(pro.waitingth);
					wat.Start();
					
					chat= client.Accept();
					wat.Abort();
					Console.WriteLine("");
					Console.WriteLine("");
					Console.Write("\a");
					pro.Writeyel("Connected to IP  "+chat.RemoteEndPoint.ToString());
					Console.Title="ConSocket => "+chat.RemoteEndPoint.ToString();
	//				pro.Writeyel("Connected");
					Console.WriteLine("");
					Console.WriteLine("");
				}
				
				
				
			}
			
			
			if(mode==2||mode==4)
			{
				while(done==0)
				{
					Console.Write("CONIP   =>");
		  			string ipstr=Console.ReadLine();
		  			if(ipstr=="l")	
			  		{
			  			ipstr="127.0.0.1";
			  		}
		  			if(ipstr=="d")	
			  		{
			  			ipstr="103.46.128.43";
			  		}
		  			if(ipstr=="p")
		  			{
		  				IPAddress[] list=Dns.GetHostAddresses(Dns.GetHostName());
		  				ipstr=list[1].ToString();
		  				
		  			}
		  			if(ipstr=="w")
		  			{
		  		/*		 try
           				{
           					WebClient clientg = new WebClient();
          			    	clientg.Encoding = System.Text.Encoding.Default;
                			string response = clientg.UploadString("http://iframe.ip138.com/ipcity.asp", "");
             				Match mc = Regex.Match(response, @"location.href=""(.*)""");
            			    if (mc.Success && mc.Groups.Count > 1)
           				    {
                		    response = clientg.UploadString(mc.Groups[1].Value, "");
              			    string[] str1 = response.Split('[');
             			    response = str1[1];
            		        string [] str = response.Split(']');
            		        response = str[0];
            		        Console.Write(response);
           	 	    		}
           				}
           				catch (System.Exception e)
           				{
           					pro.Writeyel("---获取本机外网ip失败 请手动填写（百度搜“本机ip”就有）");
           				}*/
						pro.Writeyel("---获取本机外网ip失败 请手动填写（百度搜“本机ip”就有）");
		  			}
			  		done=1;
			  		try{
			  			ip=IPAddress.Parse(ipstr);
			  		}
			  		catch(Exception)
			  		{
			  			done=0;
			  		}
				}
				done=0;
				
				
				while(done==0)
				{
					Console.Write("CONPORT =>");
			  		string portstr=Console.ReadLine();
			  		done=1;
			  		try{
			  			point=new IPEndPoint(ip,int.Parse(portstr));
			  		}
			  		catch(Exception)
			  		{
			  			done=0;
			  		}
				}
				done=0;
				
				
				int ifturnmode=0;
				
				Thread watx=new Thread(pro.waitingth);
				try{
					Console.WriteLine("");
					Console.Write("Connecting");
			
			
					
					watx.Start();
					client.Connect(point);
					watx.Abort();
					chat=client;
				}
				catch(Exception)
				{
					watx.Abort();
					Console.WriteLine("");
					pro.Writeyel("IP Not Found   ===   Create Socket with IP? (y/n)");
					char input=Console.ReadKey().KeyChar;
					if(input!='y')
					{
						return;
					}
					if(input=='y')
					{
						client=new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
						client.Bind(point);
						client.Listen(10);
				
						Console.WriteLine("");
						Console.Write("IP  "+point.ToString()+"  Waiting Connect ");
				
				
						Thread wat=new Thread(pro.waitingth);
						wat.Start();
						
						ifturnmode=1;
						
						chat= client.Accept();
						wat.Abort();
						Console.WriteLine("");
						Console.WriteLine("");
						Console.Write("\a");
						pro.Writeyel("Connected to IP  "+chat.RemoteEndPoint.ToString());
						Console.Title="ConSocket => "+chat.RemoteEndPoint.ToString();
				//		pro.Writeyel("Connected");
						Console.WriteLine("");
						Console.WriteLine("");
					}
				}
				
				if(ifturnmode==0)
				{
					Console.WriteLine("");
					Console.WriteLine("");
					Console.Write("\a");
					pro.Writeyel("Connected to IP  "+point.ToString());
					Console.Title="ConSocket => "+chat.RemoteEndPoint.ToString();
					Console.WriteLine("");
					Console.WriteLine("");
				}
				
			}
			
			
//--------------------------------------------IO-----------------------------------------------
			
			if(mode==1||mode==2)
			{
				ceng.chatlaunch(chat);
			}
		/*	if(mode==3)
			{
				if(args.Length==0)
				{
					done=0;
					while(done==0)
					{
						Console.Write("Filepath => ");
						filepath=Console.ReadLine();
						if(File.Exists(filepath))
						{
							done=1;
						}
					}
					done=0;
				}
				if(args.Length>=1)
				{
					filepath=args[0];
				}
				ft.filesend(filepath,chat);
			//	pro.filesend(filepath,chat);
			}
			if(mode==4)
			{
				ft.filerecv("",chat);
			//	pro.filerecv("",chat);
			}
			
			
			
			
			*/
			
			
		    
			while(true)
			{
				
			}
			Console.ReadKey(true);
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
		
		
		
	}
}