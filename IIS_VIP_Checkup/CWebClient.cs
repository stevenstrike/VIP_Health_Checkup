using IIS_WakeUp_Website.Data;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace IIS_WakeUp_Website
{
    public static class CWebClient
    {
        private const string HEALTH_LOCATION = "/health/health.gif";
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program).ToString());

        /// <summary>
        /// Wakes up website manual asynchronous.
        /// </summary>
        /// <param name="sitesLst">The sites.</param>
        public static void CheckUpWebsiteWithConsoleCommand(List<String> sitesLst)
        {
            // Setup parallel breaker variables.
            //int currentThreadId = 0;
            //object lockCurrentThread = new object();

            //Parallel.ForEach(sitesLst, (site) =>
            foreach (var site in sitesLst)
            {
                try
                {
                    // Lock the current thread to wait for the previous threads to finish, thus bringing order.
                    //int thisCurrentThread = 0;
                    //lock (lockCurrentThread)
                    //{
                    //    thisCurrentThread = currentThreadId;
                    //    currentThreadId++;
                    //}

                    Log.Info("Accessing : " + site);

                    // wakeup url by fetching webpage using a webclient request.
                    checkUpWebsite(site);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                }
            }
        }

        /// <summary>
        /// Wakes up website with yaml.
        /// </summary>
        /// <param name="filesLst">The files.</param>
        public static void CheckUpWebsiteWithYAML(List<String> filesLst)
        {
            //// Setup parallel breaker variables.
            //int currentThreadId = 0;
            //object lockCurrentThread = new object();

            //// Setup parallel breaker variables.
            //int currentSubThreadId = 0;
            //object lockCurrentSubThread = new object();

            // Parse YAML to Object.
            //Parallel.ForEach(filesLst, (file) =>
            foreach(var file in filesLst)
            {
                // Lock the current thread to wait for the previous threads to finish, thus bringing order.
                //int thisCurrentThread = 0;
                //lock (lockCurrentThread)
                //{
                //    thisCurrentThread = currentThreadId;
                //    currentThreadId++;
                //}

                // Parse YAML File and call method to wake up the website.
                try
                {
                    using (StreamReader reader = File.OpenText(file))
                    {
                        Log.Info("Validating file : " + Path.GetFileName(file));

                        // Deserialize YAML to object, validation is also applied.
                        DTO_Header header = CYamlParser.Deserializer.DeserializeYAML(reader);

                        Log.Info("File OK. Starting VIP Check.");

                        //Parallel.ForEach(header.WebSiteLst, (website) =>
                        foreach (var website in header.WebSiteLst)
                        {
                            // Lock the current thread to wait for the previous threads to finish, thus bringing order.
                            //int thisCurrentSubThread = 0;
                            //lock (lockCurrentSubThread)
                            //{
                            //    thisCurrentSubThread = currentSubThreadId;
                            //    currentSubThreadId++;
                            //}

                            checkUpWebsite(website.Name, website.Url, website.PortLst);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null && !String.IsNullOrEmpty(ex.InnerException.Message))
                    {
                        Log.Error("Error on File: " + Path.GetFileName(file) + " : " + ex.InnerException.Message);
                    }
                    else
                    {
                        Log.Error("Error on File: " + Path.GetFileName(file) + " : " + ex.ToString());
                    }                   
                }
            }
        }

        /// <summary>
        /// Check if the website is accessible.
        /// </summary>
        /// <param name="site">The site.</param>
        private static void checkUpWebsite(String site)
        {
            // Send HTTP request to revive Website (IP or FQDN):Port).
            try
            {
                using (MyWebClient webClient = new MyWebClient())
                {
                    webClient.UseDefaultCredentials = true;

                    string _url = treatUrl(site);

                    using (Stream stream = webClient.OpenRead(new Uri(_url, UriKind.Absolute)))
                    { 
                        // Do Nothing.
                    };
                }
            }
            catch (Exception)
            {
                Log.Error(site + " : KO");
            }
        }

        /// <summary>
        /// Check if the website is accessible.
        /// </summary>
        /// <param name="AppName">Name of the application.</param>
        /// <param name="Url">The URL.</param>
        /// <param name="PortsLst">The list of ports.</param>
        private static void checkUpWebsite(String AppName, String Url, List<String> PortsLst)
        {
            // Setup parallel breaker variables.
            //int currentThreadId = 0;
            object lockCurrentThread = new object();
            Stream myStream = null;

            //Parallel.ForEach(PortsLst, (port) =>
            foreach(var port in PortsLst)
            {
                // Lock the current thread to wait for the previous threads to finish, thus bringing order.
                //int thisCurrentThread = 0;
                //lock (lockCurrentThread)
                //{
                //    thisCurrentThread = currentThreadId;
                //    currentThreadId++;
                //}

                // Send HTTP request to revive Website (IP or FQDN):Port).
                try
                {
                    using (MyWebClient webClient = new MyWebClient())
                    {
                        webClient.UseDefaultCredentials = true;

                        string _url = treatUrl(Url);

                        using (myStream = webClient.OpenRead(new Uri(_url + ":" + port + HEALTH_LOCATION, UriKind.Absolute)))
                        {
                            // Do Nothing.
                        };
                    }
                    Log.Info(Url + ":" + port + HEALTH_LOCATION + " : OK");
                }
                catch (Exception)
                {
                    Log.Error(Url + ":" + port + HEALTH_LOCATION + " : KO");
                }
            }
        }

        /// <summary>
        /// Check if the URL is correct.
        /// If last char is '/', remove it.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        private static string treatUrl(String url)
        {
            Uri uriResult;
            bool isUrl = Uri.TryCreate(url, UriKind.Absolute, out uriResult)
                            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            if (isUrl)
            {
                if (url.Last() == '/')
                {
                    url.Remove(url.Length - 1);
                }
            }
            else
            {
                throw new Exception("Url : " + url + " is not a valid url.");
            }
            return url;
        }

        // Custom WebClient extension to change the timeout.
        private class MyWebClient : WebClient
        {
            private const int WEB_REQUEST_TIMEOUT_MILLISEC = 3 * 1000; // 3s

            protected override WebRequest GetWebRequest(Uri uri)
            {
                WebRequest w = base.GetWebRequest(uri);
                w.Timeout = WEB_REQUEST_TIMEOUT_MILLISEC;
                return w;
            }
        }
    }
}
