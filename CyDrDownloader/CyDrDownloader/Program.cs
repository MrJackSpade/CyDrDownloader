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
            WebClient wex = new WebClient();

            foreach(string arg in args)
            {
                string link = arg.Trim('"');

                string source = wex.DownloadString(link);

                string path = fixPath(source.From("id=\"title\"").From("title=\"").To("\""));

                if(!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                foreach(string chunk in source.Split("image-container"))
                {
                    if (!chunk.Contains("data-src"))
                    {
                        continue;
                    }

                    string fileUrl = chunk.From("data-src=\"").To("\"");

                    string fileDl = Path.Combine(Directory.GetCurrentDirectory(), path, Path.GetFileName(fileUrl));

                    if (!File.Exists(fileDl))
                    {

                        int retry = 3;
                        
                        do
                        {
                            try
                            {
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

                                System.Threading.Thread.Sleep(5000);
                            }

                            
                        } while (true);
                    }
                }
            }
        }
    }
}
