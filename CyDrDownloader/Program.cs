using Penguin.Extensions.Strings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;

namespace CyDrDownloader
{
    class Program
    {
        public static string fixPath(string input)
        {
            string BannedChars = "$:\\/|";

            string output = string.Empty;

            foreach(char c in input)
            {
                if (!BannedChars.Contains(c))
                {
                    output += c;
                }
            }

            return output;
        }
        static void Main(string[] args)
        {
            if(!Directory.Exists("Downloads"))
            {
                Directory.CreateDirectory("Downloads");
            }

            WebClient wex = new WebClient();

            foreach(string arg in args)
            {
                Console.WriteLine("Album: " + args[0]);

                string link = arg.Trim('"');

                Console.WriteLine("Downloading source...");

                string source = wex.DownloadString(link);

                string path = fixPath(source.From("id=\"title\"").From("title=\"").To("\""));

                if(!Directory.Exists(Path.Combine("Downloads", path)))
                {
                    Directory.CreateDirectory(Path.Combine("Downloads", path));
                }

                List<(string fileUrl, string fileDl)> toDownload = new List<(string fileUrl, string fileDl)>();

                foreach (string chunk in source.Split("image-container"))
                {
                    if (!chunk.Contains("data-src"))
                    {
                        continue;
                    }

                    string fileUrl = chunk.From("data-src=\"").To("\"");

                    string fileDl = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine("Downloads", path), Path.GetFileName(fileUrl));

                    toDownload.Add((fileUrl, fileDl));
                }



                int totalCount = toDownload.Count;
                int fileNum = 1;

                Console.WriteLine($"Found {totalCount} files...");

                foreach ((string fileUrl, string fileDl) in toDownload)
                {
                    if (!File.Exists(fileDl))
                    {

                        int retry = 3;
                        
                        do
                        {
                            try
                            {
                                Console.WriteLine($"[{fileNum}/{totalCount}] Downloading '{fileUrl}'...");
                                wex.DownloadFile(fileUrl, fileDl);
                                break;
                            } catch(Exception ex)
                            {
                                if(ex is WebException webex && webex?.Response is HttpWebResponse hwer && hwer.StatusCode == HttpStatusCode.NotFound)
                                {
                                    break;
                                }

                                if(retry-- == 0)
                                {
                                    throw;
                                }

                                Console.WriteLine($"Exception. Retrying in 5 seconds.");
                                System.Threading.Thread.Sleep(5000);
                            }

                            
                        } while (true);

                        
                    } else
                    {
                        Console.WriteLine("Skipping existing file...");
                    }
                    
                    fileNum++;
                }
            }
        }
    }
}
